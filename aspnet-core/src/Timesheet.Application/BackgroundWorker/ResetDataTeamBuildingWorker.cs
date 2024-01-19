using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using Timesheet.APIs.TeamBuildingDetails;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class ResetDataTeamBuildingWorker: PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IWorkScope _workScope;
        private readonly TeamBuildingDetailsAppService _teamBuildingDetailsAppService;
        public ResetDataTeamBuildingWorker (AbpTimer timer,
            IWorkScope workScope,
            TeamBuildingDetailsAppService teamBuildingDetailsAppService
            ): base ( timer )
        {
            _workScope = workScope;
            _teamBuildingDetailsAppService = teamBuildingDetailsAppService;
            Timer.Period = 1000 * 60 * 60;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            DateTime now = DateTimeUtils.GetNow();
            try
            {
                ResetDataTeamBuilding(now);
            }catch(Exception ex)
            {
                Logger.Error("ResetDataTeamBuilding() error: " + ex.Message);
            }
        }

        private void ResetDataTeamBuilding(DateTime now)
        {
            string resetDataTeamBuildingEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.ResetDataTeamBuildingEnableWorker);
            string resetDataTeamBuildingAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.ResetDataTeamBuildingAtHour);
            string resetDataTeamBuildingOnDateAndMonth = SettingManager.GetSettingValueForApplication(AppSettingNames.ResetDataTeamBuildingOnDateAndMonth);

            if (resetDataTeamBuildingEnableWorker != "true")
            {
                Logger.Error("ResetDataTeamBuilding() stop: resetDataTeamBuildingEnableWorker = " + resetDataTeamBuildingEnableWorker);
                return;
            }
            int.TryParse(resetDataTeamBuildingAtHour, out int hour);
            if (hour != now.Hour)
            {
                Logger.Error("ResetDataTeamBuilding() stop: resetDataTeamBuildingAtHour = " + resetDataTeamBuildingAtHour);
                return;
            }
            string Date = resetDataTeamBuildingOnDateAndMonth.Split('/', '-')[0];
            int.TryParse(Date, out int date);
            if (date != now.Day)
            {
                Logger.Error("ResetDataTeamBuilding() stop: resetDataTeamBuildingOnDate = " + date);
                return;
            }
            int index = resetDataTeamBuildingOnDateAndMonth.IndexOfAny(new char[] { '/', '-' });
            string Month = resetDataTeamBuildingOnDateAndMonth.Substring(index + 1);
            int.TryParse(Month, out int month);
            if (month != now.Month)
            {
                Logger.Error("ResetDataTeamBuilding() stop: resetDataTeamBuildingInMonth = " + Month);
                return;
            }
            _teamBuildingDetailsAppService.DeleteAll();
        }
    }
}
