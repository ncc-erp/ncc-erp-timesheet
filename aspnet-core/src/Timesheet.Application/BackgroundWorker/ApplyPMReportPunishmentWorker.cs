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
    public class ApplyPMReportPunishmentWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IUserPunishmentServices _userPunishmentService;

        public ApplyPMReportPunishmentWorker(
            AbpTimer timer,
            IUserPunishmentServices userPunishmentService
        ) : base(timer)
        {
            _userPunishmentService = userPunishmentService;

            Timer.Period = 1000 * 60 * 60;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            DateTime now = DateTimeUtils.GetNow();
            try
            {
                ApplyPMReportPunishmentJob(now);
            }
            catch (Exception ex)
            {
                Logger.Error("ApplyPMReportPunishmentJob() error: " + ex.Message, ex);
            }
        }

        private void ApplyPMReportPunishmentJob(DateTime now)
        {
            string enable = SettingManager.GetSettingValueForApplication(AppSettingNames.PMReportPunishEnable);
            string hourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.PMReportPunishAtHour);
            string dayOfWeek = SettingManager.GetSettingValueForApplication(AppSettingNames.PMReportPunishAtDayOfWeek);

            if (enable != "True")
            {
                Logger.Info("ApplyPMReportPunishmentJob() skipped: Disabled via settings.");
                return;
            }

            if (!int.TryParse(hourStr, out int configuredHour))
            {
                Logger.Error("ApplyPMReportPunishmentJob() error: Invalid hour setting.");
                return;
            }

            if (configuredHour != now.Hour)
            {
                Logger.Info($"ApplyPMReportPunishmentJob() skipped: Current hour = {now.Hour}, Configured = {configuredHour}");
                return;
            }

            if (!string.Equals(now.DayOfWeek.ToString(), dayOfWeek, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"ApplyPMReportPunishmentJob() skipped: Today is {now.DayOfWeek}, Configured = {dayOfWeek}");
                return;
            }

            Logger.Info("ApplyPMReportPunishmentJob() running...");
            _userPunishmentService.ApplyPMReportPunishmentsAsync().GetAwaiter().GetResult();
            Logger.Info("ApplyPMReportPunishmentJob() finished.");
        }
    }
}
