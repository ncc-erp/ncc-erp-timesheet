using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Microsoft.EntityFrameworkCore.Internal;
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
    public class NotifyHRTheEmployeeMayHaveLeftWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly KomuService _komuService;
        private readonly INotifyHRTheEmployeeMayHaveLeftServices _notifyHRTheEmployeeMayHaveLeftServices;
        private bool _isRunning = false;
        private int _intervalMinutes = 60; //60 minutes;
        public NotifyHRTheEmployeeMayHaveLeftWorker(AbpTimer timer, KomuService komuService, INotifyHRTheEmployeeMayHaveLeftServices notifyHRTheEmployeeMayHaveLeftServices) : base(timer)
        {
            _komuService = komuService;
            _notifyHRTheEmployeeMayHaveLeftServices = notifyHRTheEmployeeMayHaveLeftServices;
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
                    NotifyHRTheEmployeeMayHaveLeft();
                    _isRunning = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error("NotifyHRTheEmployeeMayHaveLeft() error: " + e.Message);
                _isRunning = false;
            }
        }

        private void NotifyHRTheEmployeeMayHaveLeft()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftEnableWorker);
            if (notifyEnableWorker != "true")
            {
                Logger.Info("NotifyHRTheEmployeeMayHaveLeft() stop: notifyEnableWorker=" + notifyEnableWorker);
                return;
            }

            string notifyToChannels = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftToChannels);
            if (string.IsNullOrEmpty(notifyToChannels))
            {
                Logger.Info("NotifyHRTheEmployeeMayHaveLeft() stop: config App.NotifyHRTheEmployeeMayHaveLeftToChannels is null or empty.");
                return;
            }

            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();

            if (countListChannel <= 0)
            {
                Logger.Info("NotifyHRTheEmployeeMayHaveLeft() stop: countListChannel=" + countListChannel);
                return;
            }

            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftAtHour);

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
                    Logger.Error("NotifyHRTheEmployeeMayHaveLeft() => Parse config value:" + hour + " to hour:minute error: " + ex.Message);
                }
            }

            if (!needToNotify)
            {
                Logger.Info("NotifyHRTheEmployeeMayHaveLeft() stop: notifyAtHourConfig=" + notifyAtHourConfig + " date run=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss") + " next run=" + dateNow.AddMinutes(_intervalMinutes).ToString("dd/MM/yyyy HH:mm:ss"));
                return;
            }

            Logger.Info("NotifyHRTheEmployeeMayHaveLeft() notifyToChannels=" + notifyToChannels + " countListChannel=" + countListChannel + " notifyAtHourConfig=" + notifyAtHourConfig + " checkDate=" + dateNow.ToString("dd/MM/yyyy HH:mm:ss"));

            var listNotifys = _notifyHRTheEmployeeMayHaveLeftServices.GetListEmployeeMayHaveLeft();

            var sb = new StringBuilder();
            var strTimePeriod = (SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftTimePeriod)).Replace("-", "");

            sb.AppendLine($"HR: {listNotifys.HRInfos.Select(item => item.HRKomuAccountTag()).Join()}");
            sb.AppendLine("");

            listNotifys.ProjectInfos.ForEach(item =>
            {
                sb.AppendLine($"PM: {item.PMs.Select(item2 => item2.PMKomuAccountTag()).Join()}");

                sb.AppendLine($"Please check these employees have not checkin/checkout, have not requested for off/remote/onsite, and have not logged timesheet in the past **{strTimePeriod}** days:");

                sb.AppendLine($"```");
                item.Employees.ForEach(employee =>
                {
                    sb.AppendLine($"{employee.FullName} - {employee.EmailAddress}");
                });
                sb.AppendLine($"```");
            });

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
