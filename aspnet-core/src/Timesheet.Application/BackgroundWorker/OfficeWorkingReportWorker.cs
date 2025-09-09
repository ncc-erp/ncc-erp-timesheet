using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Microsoft.Extensions.Logging;
using Ncc.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.Reports;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class OfficeWorkingReportWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly OfficeWorkingReportAppService _reportAppService;
        private readonly ILogger<OfficeWorkingReportWorker> _logger;
        private DateTime _lastSentDate = DateTime.MinValue;

        public OfficeWorkingReportWorker(
            AbpTimer timer,
            OfficeWorkingReportAppService reportAppService,
            ILogger<OfficeWorkingReportWorker> logger
        ) : base(timer)
        {
            _reportAppService = reportAppService;
            _logger = logger;
            Timer.Period = 1000 * 60; // every minute
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            var now = DateTimeUtils.GetNow();
            _logger.LogInformation($"OfficeWorkingReportWorker checking at {now:HH:mm:ss dd/MM/yyyy}");

            try
            {
                string enable = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportEnable);
                _logger.LogInformation($"OfficeWorkingReportEnable = {enable}");

                if (!string.Equals(enable, "True", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Office working report is disabled");
                    return;
                }

                string everydayStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingEveryday);
                string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportAtHour);

                _logger.LogInformation($"Configured time: {hourStr}:00, Current time: {now.Hour}:{now.Minute:D2}, Everyday: {everydayStr}");

                if (!int.TryParse(hourStr, out int configuredHour))
                {
                    configuredHour = 14;
                }

                bool isEveryday = string.Equals(everydayStr, "True", StringComparison.OrdinalIgnoreCase);

                // Check exact hour match and run only at minute 0
                if (now.Hour != configuredHour || now.Minute != 0)
                {
                    return;
                }

                // Check if already sent today
                if (_lastSentDate.Date == now.Date)
                {
                    _logger.LogInformation($"Report already sent today: {_lastSentDate:yyyy-MM-dd}");
                    return;
                }

                // Check if should run today
                if (!isEveryday)
                {
                    // Only run on weekdays (Monday to Friday)
                    if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        _logger.LogInformation($"Skipping report on weekend: {now.DayOfWeek}");
                        return;
                    }
                }

                _logger.LogInformation($"Triggering office working reports at {now:HH:mm dd/MM/yyyy}");

                string officeIdsStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportOfficeIds);
                string limitStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportLimit);
                string mezonUrl = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);

                _logger.LogInformation($"Settings - OfficeIds: {officeIdsStr}, Limit: {limitStr}, MezonUrl: {(!string.IsNullOrEmpty(mezonUrl) ? "SET" : "EMPTY")}");

                int limit = 20; 
                if (int.TryParse(limitStr, out var parsedLimit))
                {
                    if (parsedLimit <= 0)
                    {
                        limit = int.MaxValue; 
                        _logger.LogInformation("Using unlimited mode (show all records)");
                    }
                    else
                    {
                        limit = parsedLimit;
                        _logger.LogInformation($"Using limit: {limit}");
                    }
                }

                bool allSuccessful = true;

                // Check if configured for all offices
                if (string.Equals(officeIdsStr?.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Processing ALL offices with allOffices=true");
                    
                    try
                    {
                        _logger.LogInformation("Sending combined report for all offices...");

                        Task.Run(async () =>
                        {
                            await _reportAppService.SendTopOfficeUsersNotification(
                                officeId: null,
                                limit: limit,
                                reportDate: now.Date,
                                mezonUrl: mezonUrl,
                                userId: null,
                                startDate: null,
                                endDate: null,
                                showAll: false,
                                allOffices: true
                            );
                        }).GetAwaiter().GetResult();

                        _logger.LogInformation("Successfully sent combined report for all offices");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending combined report for all offices");
                        allSuccessful = false;
                    }
                }
                else
                {
                    // Process specific office IDs
                    var ids = (officeIdsStr ?? string.Empty)
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => long.TryParse(s, out _))
                        .Select(long.Parse)
                        .ToList();

                    if (ids.Count == 0)
                    {
                        _logger.LogWarning("OfficeWorkingReportWorker: No office ids configured.");
                        return;
                    }

                    _logger.LogInformation($"Processing {ids.Count} specific offices: [{string.Join(", ", ids)}]");

                    foreach (var officeId in ids)
                    {
                        try
                        {
                            _logger.LogInformation($"Sending report for office {officeId}...");

                            Task.Run(async () =>
                            {
                                await _reportAppService.SendTopOfficeUsersNotification(
                                    officeId: officeId,
                                    limit: limit,
                                    reportDate: now.Date,
                                    mezonUrl: mezonUrl,
                                    userId: null,
                                    startDate: null,
                                    endDate: null
                                );
                            }).GetAwaiter().GetResult();

                            _logger.LogInformation($"Successfully sent report for office {officeId}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending report for office {officeId}");
                            allSuccessful = false;
                        }
                    }
                }

                // Only mark as sent if all reports were successful
                if (allSuccessful)
                {
                    _lastSentDate = now.Date;
                    _logger.LogInformation($"All reports sent successfully. Marked as sent for {now.Date:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OfficeWorkingReportWorker error at {Time}", now);
            }
        }
    }
}