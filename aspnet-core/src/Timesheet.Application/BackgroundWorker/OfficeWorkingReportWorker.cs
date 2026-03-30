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
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.BackgroundWorker
{
    public class OfficeWorkingReportWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IOfficeWorkingReportServices _officeWorkingReportService;
        private readonly ILogger<OfficeWorkingReportWorker> _logger;
        private readonly IWorkScope _workScope;

        public OfficeWorkingReportWorker(
            AbpTimer timer,
            IOfficeWorkingReportServices officeWorkingReportService,
            ILogger<OfficeWorkingReportWorker> logger,
            IWorkScope workScope
        ) : base(timer)
        
        {
            _officeWorkingReportService = officeWorkingReportService;
            _logger = logger;
            _workScope = workScope;
            Timer.Period = 1000 * 60 * 60;
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
                { "HN1", (long)OfficeBranch.HN1 },
                { "HN2", (long)OfficeBranch.HN2 },
                { "SG1", (long)OfficeBranch.SG1 },
                { "SG2", (long)OfficeBranch.SG2 },
                { "ĐN", (long)OfficeBranch.DN },
                { "VINH", (long)OfficeBranch.VINH },
                { "QN", (long)OfficeBranch.QN },
                { "HN3", (long)OfficeBranch.HN3 }
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
                    //_logger.LogInformation("OfficeWorkingReportWorker: Đã tắt");
                    return;
                }

                string everydayStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingEveryday);
                string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportAtHour);

                if (!int.TryParse(hourStr, out int configuredHour))
                {
                    throw new Exception("Please configure the run hour (0-23) and minute (0-59)");
                }

                bool isEveryday = string.Equals(everydayStr, "True", StringComparison.OrdinalIgnoreCase);
                if (now.Hour != configuredHour)
                {
                    //Logger.Info($"RunBotReportJob() skipped: Current hour = {now.Hour}, Configured = {configuredHour}, Current minute = {now.Minute}, Configured minute = {configuredMinute}");
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

                var ids = ConvertBranchCodesToOfficeIds(officeIdsStr);
                if (ids.Count == 0)
                {
                    return;
                }

                foreach (var officeId in ids)
                {
                    var input = new SendTopOfficeWorkingTimeNotificationDto
                    {
                        OfficeId = officeId,
                        Limit = limit,
                        MezonUrl = mezonUrl
                    };
                    _officeWorkingReportService.SendTopOfficeUsersNotification(input).GetAwaiter().GetResult();
                }
                Logger.Info("RunOfficeBotReportJob() finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during processing at: {now}");
            }
        }
    }
}