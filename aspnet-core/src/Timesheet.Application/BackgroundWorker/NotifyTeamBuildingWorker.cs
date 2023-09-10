using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Timesheet.DomainServices;
using Timesheet.Services.Komu;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class NotifyTeamBuildingWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly KomuService _komuService;
        private readonly ISendMessageRequestPendingTeamBuildingToHRServices _sendMessageRequestPendingTeamBuildingToHRServices;
        private bool _isRunning = false;
        private int _intervalMinutes = 60; //60 minutes;
        public NotifyTeamBuildingWorker(AbpTimer timer, KomuService komuService, ISendMessageRequestPendingTeamBuildingToHRServices sendMessageRequestPendingTeamBuildingToHRServices) : base(timer)
        {
            _komuService = komuService;
            _sendMessageRequestPendingTeamBuildingToHRServices = sendMessageRequestPendingTeamBuildingToHRServices;
            Timer.Period = 1000 * 60 * _intervalMinutes;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            try
            {
                if(!_isRunning)
                {
                    _isRunning = true;
                    SendMessageRequestPendingTeamBuildingToHR();
                    _isRunning = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error("SendMessageRequestPendingTeamBuildingToHR() error: " + e.Message);
                _isRunning = false;
            }
        }

        private void SendMessageRequestPendingTeamBuildingToHR()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.SendMessageRequestPendingTeamBuildingToHREnableWorker);
            if (notifyEnableWorker != "true")
            {
                Logger.Info("SendMessageRequestPendingTeamBuildingToHR() stop: notifyEnableWorker=" + notifyEnableWorker);
                return;
            }

            string notifyToChannels = SettingManager.GetSettingValueForApplication(AppSettingNames.SendMessageRequestPendingTeamBuildingToHRToChannels);
            if (string.IsNullOrEmpty(notifyToChannels))
            {
                Logger.Info("SendMessageRequestPendingTeamBuildingToHR() stop: config App.SendMessageRequestPendingTeamBuildingToHRToChannels is null or empty.");
                return;
            }

            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();

            if (countListChannel <= 0)
            {
                Logger.Info("SendMessageRequestPendingTeamBuildingToHR() stop: countListChannel=" + countListChannel);
                return;
            }

            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.SendMessageRequestPendingTeamBuildingToHRAtHour);

            string[] arrListHour = notifyAtHourConfig.Split(',');
            bool needToNotify = false;

            foreach (string hour in arrListHour)
            {
                try
                {
                    string[] hourAndMinute = hour.Trim().Split(':');
                    int configMinute = 0, configHour = 0;
                    if (hourAndMinute.Length == 2)
                    {
                        configMinute = int.Parse(hourAndMinute[1].Trim());
                        configHour = int.Parse(hourAndMinute[0].Trim());
                    }
                    else if (hourAndMinute.Length == 1)
                    {
                        configHour = int.Parse(hour.Trim());
                    }

                    if (configHour.Equals(dateNow.Hour) && (configMinute.Equals(dateNow.Minute) || (dateNow.Minute > configMinute && (dateNow.Minute - configMinute) < _intervalMinutes)))
                    {
                        needToNotify = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("SendMessageRequestPendingTeamBuildingToHR() => Parse config value:" + hour + " to hour:minute error: " + ex.Message);
                }
            }

            if (!needToNotify)
            {
                Logger.Info("SendMessageRequestPendingTeamBuildingToHR() stop: notifyAtHourConfig=" + notifyAtHourConfig + " date run=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss") + " next run=" + dateNow.AddMinutes(_intervalMinutes).ToString("dd/MM/yyyy HH:mm:ss"));
                return;
            }

            Logger.Info("SendMessageRequestPendingTeamBuildingToHR() notifyToChannels=" + notifyToChannels + " countListChannel=" + countListChannel + " notifyAtHourConfig=" + notifyAtHourConfig + " checkDate=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss"));

            var listRequestPending = _sendMessageRequestPendingTeamBuildingToHRServices.GetListRequestPendingTeamBuilding();

            var sb = new StringBuilder();
            foreach (var item in listRequestPending)
            {
                if(item.Requests.Count > 0)
                {
                    if (item.Requests.Count == 1)
                    {
                        sb.AppendLine($"HR: {item.KomuAccountTag()} please disburse request TeamBuilding for **{item.Requests.Count()}** request");
                    }
                    else
                    {
                        sb.AppendLine($"HR: {item.KomuAccountTag()} please disburse request TeamBuilding for **{item.Requests.Count()}** requests");
                    }

                    sb.AppendLine($"```");

                    foreach (var request in item.Requests)
                    {
                        sb.AppendLine($"PM: {request.RequesterEmail} - Title request: {request.Title} - Note: {request.Note}");
                    }

                    sb.AppendLine($"```");

                    if (countListChannel > 0)
                    {
                        for (var i = 0; i < countListChannel; i++)
                        {
                            _komuService.NotifyToChannel(sb.ToString(), arrListChannel[i].Trim());
                        }
                    }
                    sb.Clear();
                }
            }
        }
    }
}
