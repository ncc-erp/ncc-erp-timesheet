using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using Timesheet.Services.Mezon;
using Timesheet.Uitls;
using Timesheet.DomainServices;

namespace Timesheet.BackgroundWorker
{
    public class BotReportWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IUserPunishmentServices _userPunishmentService;

        public BotReportWorker(
            AbpTimer timer,
            IUserPunishmentServices userPunishmentService
        ) : base(timer)
        {
            _userPunishmentService = userPunishmentService;

            Timer.Period = 1000 * 60 ;
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
            
            ExecuteBotReport();
            
            Logger.Info("RunBotReportJob() finished.");
        }

        private void ExecuteBotReport()
        {
            try
            {
                _userPunishmentService.ApplyPMReportPunishmentsAsync().GetAwaiter().GetResult();
                
                Logger.Info("Bot Report executed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error($"ExecuteBotReport() error: {ex.Message}", ex);
            }
        }
    }
}
