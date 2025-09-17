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
            Timer.Period = 1000 * 30;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            DateTime now = DateTimeUtils.GetNow();
            if (now.Minute == 0)
                {
                try
                {
                    Logger.Info($"BotReportWorker running at {now:yyyy-MM-dd HH:mm:ss}");
                    RunBotReportJob(now);
                }
                catch (Exception ex)
                {
                    Logger.Error("RunBotReportJob() error: " + ex.Message, ex);
                }
            }
        }

        private void RunBotReportJob(DateTime now)
        {
            var projectIdsString = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportProjectIds);
            Logger.Debug($"Raw projectIds setting: '{projectIdsString}'");

            var branchCodesString = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportBranchCodes);
            Logger.Debug($"Raw branchCodes setting: '{branchCodesString}'");

            var anomaliesSetting = new BotReportSettingDto
            {
                enable = bool.TryParse(
                    SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportEnable),
                    out var enable) ? enable : false,
                everyday = bool.TryParse(
                    SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportEveryday),
                    out var everyday) ? everyday : false,
                hour = int.TryParse(
                    SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportAtHour),
                    out var hourParsed) ? hourParsed : 0,
                dayofweek = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportAtDayOfWeek),
                botUri = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportWebhookUrl) ?? string.Empty,
                branchCodes = branchCodesString != null
                    ? branchCodesString
                        .Replace("[", "")
                        .Replace("]", "")
                        .Replace("\"", "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList()
                    : new List<string>(),
                minHours = double.TryParse(
                    SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportMinHours),
                    out var minHours) ? minHours : (double?)null,
                topN = int.TryParse(
                    SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportTopN),
                    out var topN) ? topN : (int?)null,
                projectIds = projectIdsString != null
                    ? projectIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Where(s => long.TryParse(s.Trim(), out _))
                        .Select(s => long.Parse(s.Trim())).ToList()
                    : new List<long>()
            };

            Logger.Debug($"Parsed projectIds: [{string.Join(", ", anomaliesSetting.projectIds)}]");

            if (!anomaliesSetting.enable)
            {
                Logger.Info("RunBotReportJob() skipped: Disabled via settings (Enable = false).");
                return;
            }

            if (anomaliesSetting.hour != now.Hour)
            {
                Logger.Info($"RunBotReportJob() skipped: Current hour = {now.Hour}, Configured = {anomaliesSetting.hour}");
                return;
            }

            Logger.Info($"Running Daily report for branches: {string.Join(", ", anomaliesSetting.branchCodes)}");
            ExecuteBotReport(anomaliesSetting, isWeekly: false);
            ExecuteBotReport(anomaliesSetting, isWeekly: true);

            if (!string.IsNullOrEmpty(anomaliesSetting.dayofweek) &&
                string.Equals(now.DayOfWeek.ToString(), anomaliesSetting.dayofweek, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Running Weekly report for branches: {string.Join(", ", anomaliesSetting.branchCodes)}");
                ExecuteBotReport(anomaliesSetting, isWeekly: true);
            }
            else
            {
                Logger.Info($"Weekly report skipped: Today is {now.DayOfWeek}, Configured = {anomaliesSetting.dayofweek}");
            }

            Logger.Info("RunBotReportJob() finished.");
        }

        private void ExecuteBotReport(BotReportSettingDto anomaliesSetting, bool isWeekly)
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