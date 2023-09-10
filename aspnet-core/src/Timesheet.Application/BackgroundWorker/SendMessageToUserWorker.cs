using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.DomainServices;
using Timesheet.Services.Komu;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class SendMessageToUserWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly KomuService _komuService;
        private readonly ISendMessageToUserServices _sendMessageToUserServices;
        private bool _isRunning = false;
        private int _intervalMinutes = 60; //60 minutes;
        public SendMessageToUserWorker(AbpTimer timer, KomuService komuService, ISendMessageToUserServices sendMessageToUserServices) : base(timer)
        {
            _komuService = komuService;
            _sendMessageToUserServices = sendMessageToUserServices;
            Timer.Period = 1000 * 60 * _intervalMinutes;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            try
            {
                if (!_isRunning)
                {
                    _isRunning = true;                  
                    SendMessageToPunishUser();
                    _isRunning = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error("SendMessageToPunishUser() error: " + e.Message);
                _isRunning = false;
            }
        }

        private void SendMessageToPunishUser()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.SendMessageToPunishUserEnableWorker);
            if (notifyEnableWorker != "true")
            {
                Logger.Info("SendMessageToPunishUser() stop: notifyEnableWorker=" + notifyEnableWorker);
                return;
            }

            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.SendMessageToPunishUserAtHour);

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
                    Logger.Error("SendMessageToPunishUser() => Parse config value:" + hour + " to hour:minute error: " + ex.Message);
                }
            }

            if (!needToNotify)
            {
                Logger.Info("SendMessageToPunishUser() stop: notifyAtHourConfig=" + notifyAtHourConfig + " date run=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss") + " next run=" + dateNow.AddMinutes(_intervalMinutes).ToString("dd/MM/yyyy HH:mm:ss"));
                return;
            }

            Logger.Info("SendMessageToPunishUser() notifyAtHourConfig=" + notifyAtHourConfig + " checkDate=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss"));

            var listNotifys = _sendMessageToUserServices.GetListUserPunish();

            var sb = new StringBuilder();

            foreach (var item in listNotifys)
            {
                var strMoneyPunish = item.MoneyPunish.ToString("N0");
                sb.AppendLine($"You have been punished **{strMoneyPunish} VNĐ** on **{item.DateAt.ToString("dd/MM/yyyy")}**, for **{item.NotePunish}**.");
                sb.AppendLine($"If this information is **NOT Accurate**, please submit a complaint on the Timesheet.");
                _komuService.SendMessageToUser(sb.ToString(), item.UserName.Trim());

                sb.Clear();
            }
        }
    }
}
