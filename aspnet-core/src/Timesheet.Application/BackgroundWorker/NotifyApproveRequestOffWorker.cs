using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Ncc.Configuration;
using System;
using System.Linq;
using Timesheet.APIs.RequestDays;
using Timesheet.DomainServices.Dto;
using Timesheet.Uitls;

namespace Timesheet.BackgroundWorker
{
    public class NotifyApproveRequestOffWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IApproveRequestOffServices _approveRequestOffServices;
        private bool _isRunning = false;
        private int _intervalMinutes = 30;
        public NotifyApproveRequestOffWorker(AbpTimer timer, IApproveRequestOffServices approveRequestOffServices, RequestDayAppService requestDayAppService) : base(timer)
        {
            _approveRequestOffServices = approveRequestOffServices;
            Timer.Period = 1000 * 60 * _intervalMinutes;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            if(!_isRunning)
            {
                _isRunning = true;

                try
                {
                    NotifyApproveRequestOff();
                }
                catch (Exception e)
                {
                    Logger.Error("NotifyApproveRequestOff() error: " + e.Message);
                    _isRunning = false;
                }
                try
                {
                    SendMessageToUserDueDate();
                }
                catch (Exception e)
                {
                    Logger.Error("SendMessageToUserDueDate() error: " + e.Message);
                    _isRunning = false;
                }

                _isRunning = false;
            }
        }
        private void NotifyApproveRequestOff()
        {
            var dateNow = DateTimeUtils.GetNow();
            if ((dateNow.DayOfWeek == DayOfWeek.Saturday && dateNow.Hour > 12) || dateNow.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }

            string notifyEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveRequestOffNotifyEnableWorker);
            if (notifyEnableWorker != "true")
            {
                Logger.Info("NotifyApproveRequestOff() stop: notifyEnableWorker=" + notifyEnableWorker);
                return;
            }

            string notifyToChannels = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveRequestOffNotifyToChannels);
            if (string.IsNullOrEmpty(notifyToChannels))
            {
                Logger.Info("NotifyApproveRequestOff() stop: config App.ApproveRequestOffNotifyToChannels is null or empty.");
                return;
            }

            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();

            if (countListChannel <= 0)
            {
                Logger.Info("NotifyApproveRequestOff() stop: countListChannel=" + countListChannel);
                return;
            }

            Logger.Info($"NotifyApproveRequestOff() running at {dateNow:dd/MM/yyyy HH:mm:ss}");

            var listRequestOff = _approveRequestOffServices.GetListPmNotApproveRequestOff();

            _approveRequestOffServices.NotifyRequestOffPending(listRequestOff, notifyToChannels);

            _approveRequestOffServices.SendMessageToUserRequestOffPending(listRequestOff);
        }

        private void SendMessageToUserDueDate()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveRequestOffSendUserAtHour);
            if (dateNow.Hour >= int.Parse(notifyAtHourConfig))
            {
                var listRequestOff = _approveRequestOffServices.GetListPmNotApproveRequestOff();
                var listRequestOffDueDate = listRequestOff.Where(s => s.DateAt.Date == dateNow.Date).ToList();
                if (listRequestOffDueDate.Count > 0)
                {
                    _approveRequestOffServices.SendMessageToUserRequestOffPending(listRequestOffDueDate);
                }
                else
                {
                    Logger.Info("SendMessageToUserDueDate() stop: notifyAtHourConfig=" + notifyAtHourConfig + " date run=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss") + " next run=" + dateNow.AddMinutes(_intervalMinutes).ToString("dd/MM/yyyy HH:mm:ss"));
                    return;
                }
            }
            else
            {
                Logger.Info("SendMessageToUserDueDate() stop: notifyAtHourConfig=" + notifyAtHourConfig + " date run=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss") + " next run=" + dateNow.AddMinutes(_intervalMinutes).ToString("dd/MM/yyyy HH:mm:ss"));
                return;
            }
        }
    }
}
