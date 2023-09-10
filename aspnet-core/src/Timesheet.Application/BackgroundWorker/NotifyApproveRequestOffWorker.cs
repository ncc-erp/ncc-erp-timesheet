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
        private int _intervalMinutes = 5;//5 minutes
        public NotifyApproveRequestOffWorker(AbpTimer timer, IApproveRequestOffServices approveRequestOffServices, RequestDayAppService requestDayAppService) : base(timer)
        {
            _approveRequestOffServices = approveRequestOffServices;
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
                    NotifyApproveRequestOff();
                    _isRunning = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error("NotifyApproveRequestOff() error: " + e.Message);
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

            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveRequestOffNotifyAtHour);

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

                    // Vì background job chạy có thời gian delay theo _intervalMinutes nên có thể xảy ra trường hợp job chạy sẽ lệch với giờ:phút ở config
                    // nên khi check thời gian thì cần cộng thêm _intervalMinutes vào phút để đảm bảo job chạy đúng theo khung giờ được config
                    // vd: config là 17:00 mà job chạy vào khoảng 17:01-17:{_intervalMinutes} thì cũng sẽ gửi notify
                    if (configHour.Equals(dateNow.Hour) && (configMinute.Equals(dateNow.Minute) || (dateNow.Minute > configMinute && (dateNow.Minute - configMinute) < _intervalMinutes)))
                    {
                        needToNotify = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("NotifyApproveRequestOff() => Parse config value:" + hour + " to hour:minute error: " + ex.Message);
                }

            }

            if (!needToNotify)
            {
                Logger.Info("NotifyApproveRequestOff() stop: notifyAtHourConfig=" + notifyAtHourConfig + " date run=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss") + " next run=" + dateNow.AddMinutes(_intervalMinutes).ToString("dd/MM/yyyy HH:mm:ss"));
                return;
            }

            Logger.Info("NotifyApproveRequestOff() notifyToChannels=" + notifyToChannels + " countListChannel=" + countListChannel + " notifyAtHourConfig=" + notifyAtHourConfig + " checkDate=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss"));

            var listRequestOff = _approveRequestOffServices.GetListPmNotApproveRequestOff();

            _approveRequestOffServices.NotifyRequestOffPending(listRequestOff, notifyToChannels);

            _approveRequestOffServices.SendMessageToUserRequestOffPending(listRequestOff);
        }
    }
}
