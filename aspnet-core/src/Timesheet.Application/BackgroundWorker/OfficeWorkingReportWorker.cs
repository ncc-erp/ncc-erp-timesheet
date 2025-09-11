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
            Timer.Period = 1000 * 60; 
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            var now = DateTimeUtils.GetNow();

            try
            {
                string enable = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportEnable);

                if (!string.Equals(enable, "True", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string everydayStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingEveryday);
                string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportAtHour);

                if (!int.TryParse(hourStr, out int configuredHour))
                {
                    configuredHour = 14;
                }

                bool isEveryday = string.Equals(everydayStr, "True", StringComparison.OrdinalIgnoreCase);
                if (now.Hour != configuredHour || now.Minute != 0)
                {
                    return;
                }
                if (_lastSentDate.Date == now.Date)
                {
                    return;
                }
                if (!isEveryday)
                {
                    if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return;
                    }
                }

                string officeIdsStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportOfficeIds);
                string limitStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportLimit);
                string mezonUrl = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);

                int limit = 20; 
                if (int.TryParse(limitStr, out var parsedLimit))
                {
                    limit = parsedLimit <= 0 ? int.MaxValue : parsedLimit;
                }

                bool allSuccessful = true;
                if (string.Equals(officeIdsStr?.Trim(), "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending combined report for all offices");
                        allSuccessful = false;
                    }
                }
                else
                {
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

                    foreach (var officeId in ids)
                    {
                        try
                        {
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
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending report for office {officeId}");
                            allSuccessful = false;
                        }
                    }
                }
                if (allSuccessful)
                {
                    _lastSentDate = now.Date;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OfficeWorkingReportWorker error at {Time}", now);
            }
        }
    }
}