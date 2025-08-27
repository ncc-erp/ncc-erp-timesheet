using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using System.Threading;
using Timesheet.APIs.Reports;
using Timesheet.DomainServices;
using Timesheet.Services.Mezon;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class BotReportDailyWorkingTimeWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly UserTimeReport_BotAppService _userTimeReportBot;

        public BotReportDailyWorkingTimeWorker(
            AbpTimer timer,
            UserTimeReport_BotAppService userTimeReportBot
        ) : base(timer)
        {
            _userTimeReportBot = userTimeReportBot;

            Timer.Period = 1000 * 60 * 60;
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
            string enable = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportEnable);
            string everyday = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportEveryday);
            string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportAtHour);
            string dayOfWeek = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportAtDayOfWeek);

            if (enable != "True")
            {
                Logger.Info("RunBotReportJob() skipped: Disabled via settings.");
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
                Logger.Info($"RunBotReportJob() skipped: Today is {now.DayOfWeek}, Configured = {dayOfWeek}");
                return;
            }

            Logger.Info($"RunBotReportJob() running... [Mode: {(isEveryday ? "Everyday" : dayOfWeek)}]");

            ExecuteBotReportWithRetry();

            Logger.Info("RunBotReportJob() finished.");
        }
        private const int MAX_RETRY_ATTEMPTS = 5;
        private const int RETRY_DELAY_MS = 2000;
        private void ExecuteBotReportWithRetry()
        {
            int attempt = 0;
            bool success = false;
            Exception lastException = null;

            while (attempt < MAX_RETRY_ATTEMPTS && !success)
            {
                attempt++;

                try
                {
                    Logger.Info($"ExecuteBotReport() - Attempt {attempt}/{MAX_RETRY_ATTEMPTS}");

                    _userTimeReportBot.ProcessTimesheetCommand("*timesheet hn1 all", DateTime.Today);

                    Logger.Info($"Bot Report executed successfully on attempt {attempt}");
                    success = true;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Logger.Warn($"ExecuteBotReport() failed on attempt {attempt}/{MAX_RETRY_ATTEMPTS}: {ex.Message}");
                    if (attempt < MAX_RETRY_ATTEMPTS)
                    {
                        Logger.Info($"Waiting {RETRY_DELAY_MS}ms before retry...");
                        Thread.Sleep(RETRY_DELAY_MS);
                    }
                }
            }

            if (!success)
            {
                string errorMessage = $"ExecuteBotReport() failed after {MAX_RETRY_ATTEMPTS} attempts. Last error: {lastException?.Message}";
                Logger.Error(errorMessage, lastException);
                NotifyFailure(errorMessage);
            }
        }

        private void NotifyFailure(string errorMessage)
        {
            try
            {
                Logger.Fatal($"CRITICAL: Bot Report Worker failed completely - {errorMessage}");
                _userTimeReportBot.NotifyFailure($" BOT REPORT WORKER FAILED\n" +
                    $" Error: {errorMessage}\n" +
                    $" Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $" Attempted: {MAX_RETRY_ATTEMPTS} times\n");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send failure notification: {ex.Message}", ex);
            }
        }
    }
}
