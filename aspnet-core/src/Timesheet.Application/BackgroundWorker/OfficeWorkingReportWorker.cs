using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Microsoft.Extensions.Logging;
using Ncc.Configuration;
using Ncc.IoC;   
using System;
using System.Collections.Generic;  
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
        private readonly IWorkScope _workScope;
        private DateTime _lastSentDate = DateTime.MinValue;

        public OfficeWorkingReportWorker(
            AbpTimer timer,
            OfficeWorkingReportAppService reportAppService,
            ILogger<OfficeWorkingReportWorker> logger,
            IWorkScope workScope
        ) : base(timer)
        
        {
            _reportAppService = reportAppService;
            _logger = logger;
            _workScope = workScope;
            Timer.Period = 1000 * 60;
        }

        private List<long> ConvertBranchCodesToOfficeIds(string branchCodesStr)
        {
            if (string.IsNullOrWhiteSpace(branchCodesStr))
                return new List<long>();

            var codes = branchCodesStr
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToUpper())
                .ToList();

            var codeToIdMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
            {
                { "HN1", 1 },
                { "HN2", 2 },
                { "SG1", 3 },
                { "SG2", 4 },
                { "ĐN", 5 },
                { "VINH", 6 },
                { "QN", 7 },
                { "HN3", 8 }
            };

            return codes
                .Where(code => codeToIdMap.ContainsKey(code))
                .Select(code => codeToIdMap[code])
                .Distinct()
                .ToList();
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
                    _logger.LogInformation("OfficeWorkingReportWorker: Đã tắt");
                    return;
                }

                string everydayStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingEveryday);
                string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportAtHour);
                string minuteStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportAtMinute);

                if (!int.TryParse(hourStr, out int configuredHour) || !int.TryParse(minuteStr, out int configuredMinute))
                {
                    throw new Exception(" Vui lòng thiết lập giờ chạy (0-23) và phút (0-59).");
                }

                bool isEveryday = string.Equals(everydayStr, "True", StringComparison.OrdinalIgnoreCase);
                if (now.Hour != configuredHour || now.Minute != configuredMinute)
                {
                    // Logger.Info($"RunBotReportJob() skipped: Current hour = {now.Hour}, Configured = {configuredHour}, Current minute = {now.Minute}, Configured minute = {configuredMinute}");
                    return;
                }

                if (_lastSentDate.Date == now.Date)
                {
                    return;
                }

                if (!isEveryday && (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday))
                {
                    return;
                }

                string officeIdsStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportOfficeIds);
                string limitStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportLimit);
                string mezonUrl = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);

                int limit = 20; 
                if (int.TryParse(limitStr, out var parsedLimit) && parsedLimit > 0)
                {
                    limit = parsedLimit;
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
                        _logger.LogError(ex, "Lỗi khi gửi báo cáo tổng hợp");
                        allSuccessful = false;
                    }
                }
                else
                {
                    var ids = (officeIdsStr ?? string.Empty)
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Select(part => long.TryParse(part, out long numId) ? numId : (long?)null)
                        .Where(id => id.HasValue)
                        .Select(id => id.Value)
                        .ToList();

                    if (ids.Count == 0)
                    {
                        ids = ConvertBranchCodesToOfficeIds(officeIdsStr);
                    }

                    if (ids.Count == 0)
                    {
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
                            _logger.LogError(ex, $"Lỗi khi gửi báo cáo cho văn phòng {officeId}");
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
                _logger.LogError(ex, $"Lỗi trong quá trình xử lý: {now}");
            }
        }
    }
}