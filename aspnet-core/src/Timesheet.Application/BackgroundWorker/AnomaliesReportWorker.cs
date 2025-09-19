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
            string enable = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportEnable);
            string everyday = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportEveryday);
            string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportAtHour);
            string dayOfWeek = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportAtDayOfWeek);
            var projectIdsString = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportProjectIds);
            var branchCodesString = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportBranchCodes);
            string botUri = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportWebhookUrl) ?? string.Empty;
            string minHoursStr = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportMinHours);
            string topNStr = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportTopN);

            if (enable != "True")
            {
                Logger.Info("RunBotReportJob() skipped: Disabled via settings (Enable = false).");
                return;
            }

            if (!int.TryParse(hourStr, out int configuredHour))
            {
                Logger.Error("RunBotReportJob() error: Invalid hour setting.");
                return;
            }

            if (configuredHour != now.Hour)
            {
                Logger.Info($"RunBotReportJob() skipped: Current hour = {now.Hour}, Configured = {configuredHour}");
                return;
            }

            bool isEveryday = everyday == "True";

            if (!isEveryday && !string.Equals(now.DayOfWeek.ToString(), dayOfWeek, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Weekly report skipped: Today is {now.DayOfWeek}, Configured = {dayOfWeek}");
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

            var projectIds = projectIdsString != null
                ? projectIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => long.TryParse(s.Trim(), out _))
                    .Select(s => long.Parse(s.Trim())).ToList()
                : new List<long>();

            double? minHours = double.TryParse(minHoursStr, out var parsedMinHours) ? parsedMinHours : (double?)null;
            int? topN = int.TryParse(topNStr, out var parsedTopN) ? parsedTopN : (int?)null;

            Logger.Debug($"Parsed projectIds: [{string.Join(", ", projectIds)}]");

            Logger.Info($"Running Daily report for branches: {string.Join(", ", branchCodes)}");
            ExecuteBotReport(new BotReportSettingDto
            {
                enable = true,
                everyday = isEveryday,
                hour = configuredHour,
                dayofweek = dayOfWeek,
                botUri = botUri,
                branchCodes = branchCodes,
                minHours = minHours,
                topN = topN,
                projectIds = projectIds
            }, isWeekly: false);

            if (!string.IsNullOrEmpty(dayOfWeek) &&
                string.Equals(now.DayOfWeek.ToString(), dayOfWeek, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Running Weekly report for branches: {string.Join(", ", branchCodes)}");
                ExecuteBotReport(new BotReportSettingDto
                {
                    enable = true,
                    everyday = isEveryday,
                    hour = configuredHour,
                    dayofweek = dayOfWeek,
                    botUri = botUri,
                    branchCodes = branchCodes,
                    minHours = minHours,
                    topN = topN,
                    projectIds = projectIds
                }, isWeekly: true);
            }
            else
            {
                Logger.Info($"Weekly report skipped: Today is {now.DayOfWeek}, Configured = {dayOfWeek}");
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