using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.Core;
using Timesheet.DomainServices.Dto;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class AnomaliesReportWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IAbsenceDayServices _absenceDayServices;

        public AnomaliesReportWorker(
            AbpTimer timer,
            IAbsenceDayServices absenceDayServices
        ) : base(timer)
        {
            _absenceDayServices = absenceDayServices;
            Timer.Period = 1000 * 60;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            DateTime now = DateTimeUtils.GetNow();
            try
            {
                RunBotReportJob(now);
            }
            catch (Exception ex)
            {
                Logger.Error("RunBotReportJob() error: " + ex.Message, ex);
            }
        }

        private void RunBotReportJob(DateTime now)
        {
            string enable = SettingManager.GetSettingValueForApplication(AppSettingNames.AnomaliesReportEnable);
            string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.AnomaliesReportAtHour);
            string minuteStr = SettingManager.GetSettingValueForApplication(AppSettingNames.AnomaliesReportAtMinute);
            string dayOfWeek = SettingManager.GetSettingValueForApplication(AppSettingNames.AnomaliesReportAtDayOfWeek);
            var branchCodesString = SettingManager.GetSettingValueForApplication(AppSettingNames.AnomaliesReportBranchCodes);
            string botUri = SettingManager.GetSettingValueForApplication(AppSettingNames.AnomaliesReportWebhookUrl) ?? string.Empty;

            if (enable != "True")
            {
                return;
            }

            if (!int.TryParse(hourStr, out int configuredHour) || !int.TryParse(minuteStr, out int configuredMinute))
            {
                Logger.Error("RunBotReportJob() error: Invalid hour setting.");
                return;
            }

            if (configuredHour != now.Hour || configuredMinute != now.Minute)
            {
                //Logger.Info($"RunBotReportJob() skipped: Current hour = {now.Hour}, Configured = {configuredHour}, Current minute = {now.Minute}, Configured = {configuredMinute}");
                return;
            }

            var branchCodes = branchCodesString != null
                ? branchCodesString
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("\"", "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList()
                : new List<string>();

            Logger.Info($"Running Daily report for branches: {string.Join(", ", branchCodes)}");
            ExecuteBotReport(new AnomaliesReportSettingDto
            {
                enable = true,
                hour = configuredHour,
                minute = configuredMinute,
                dayofweek = dayOfWeek,
                botUri = botUri,
                branchCodes = branchCodes
            }, isWeekly: false);

            if (!string.IsNullOrEmpty(dayOfWeek) &&
                string.Equals(now.DayOfWeek.ToString(), dayOfWeek, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Running Weekly report for branches: {string.Join(", ", branchCodes)}");
                ExecuteBotReport(new AnomaliesReportSettingDto
                {
                    enable = true,
                    hour = configuredHour,
                    minute = configuredMinute,
                    dayofweek = dayOfWeek,
                    botUri = botUri,
                    branchCodes = branchCodes
                }, isWeekly: true);
            }
            else
            {
                Logger.Info($"Weekly report skipped: Today is {now.DayOfWeek}, Configured = {dayOfWeek}");
            }

            Logger.Info("RunBotReportJob() finished.");
        }

        private void ExecuteBotReport(AnomaliesReportSettingDto anomaliesSetting, bool isWeekly)
        {
            try
            {
                _absenceDayServices.SendDailyAnomaliesToMezon(anomaliesSetting, isWeekly).GetAwaiter().GetResult();
                Logger.Info($"{(isWeekly ? "Weekly" : "Daily")} Bot Report executed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"ExecuteBotReport({(isWeekly ? "Weekly" : "Daily")}) error: {ex.Message}", ex);
            }
        }
    }
}