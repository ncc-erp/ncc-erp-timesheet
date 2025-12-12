
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Timesheet.APIs.ReviewDetails;
using Abp.Threading.Timers;
using Abp.Domain.Uow;
using Amazon.Runtime.Internal.Util;
using Abp.Threading.BackgroundWorkers;
using Abp.Dependency;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.DomainServices;
using Timesheet.Services.Komu;
using System.Data;
using Abp.Configuration;
using Ncc.Configuration;
using System.Linq;
using Timesheet.Entities;
using Ncc.IoC;
using Ncc.Authorization.Users;
using System.Net.Mail;
using Ncc;
using Amazon.Util.Internal;
using Timesheet.Extension;
using Timesheet.Configuration.Dto;
using System.Threading.Tasks;
using Abp;
using Timesheet.Helper;

namespace Timesheet.BackgroundWorker  
{
    public class NotifyReviewInternWorker: PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ReviewDetailAppService _reviewDetailAppService;
        private readonly ReviewInternServices _reviewInternService;
        private readonly KomuService _komuService;
        private readonly UserServices _userServices;
        private readonly ConfigurationAppService _configurationAppService;
        private bool _isSendMailToHeadPm = true;
        private bool _isSendMailToGD = true;
        private string notifyAtHourConfig;

        public NotifyReviewInternWorker(AbpTimer timer, ReviewDetailAppService reviewDetailAppService, 
            ReviewInternServices reviewInternServices, 
            KomuService komuService, 
            UserServices userServices,
            ConfigurationAppService configurationAppService,
            ISettingManager settingManager) : base(timer)
        {
            _reviewDetailAppService = reviewDetailAppService;
            _reviewInternService = reviewInternServices;
            _komuService = komuService;
            _userServices = userServices;
            _configurationAppService = configurationAppService;
            SettingManager = settingManager;
            UpdateTimerPeriod();
        }

        private void UpdateTimerPeriod()
        {
            int intervalMinutes = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternIntervalMinutes));
            Timer.Period = 1000 * 60 * intervalMinutes;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            try
            {
                UpdateTimerPeriod();
            }
            catch (Exception ex)
            {
                Logger.Error(("UpdateTimerPeriod() error: " + ex.Message));
            }

            try
            {
                var task = UpdateTimeCronjob();
                task.Wait();
            }
            catch (Exception e)
            {
                Logger.Error(("UpdateTimeCronjob() error: " + e.Message));
            }

            try
            {
                notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyAtHourType);
            }
            catch (Exception e)
            {
                Logger.Error(("UpdateNotifyAtHourConfig() error: " + e.Message));
            }

            if (_isSendMailToHeadPm)
            {
                try
                {
                    bool isSendMail = SendMailToHeadPmReviewWorker();
                    if (isSendMail)
                    {
                        Logger.Info("SendMailToHeadPmReviewWorker() success ");
                        _isSendMailToHeadPm = false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("SendMailToHeadPmReviewWorker() error: " + e.Message);
                }
            }

            if (_isSendMailToGD)
            {
                try
                {
                    bool isSendMail = SendMailToPresident();
                    if (isSendMail)
                    {
                        Logger.Info("Send mail to President success");
                        _isSendMailToGD = false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("SendMailToPresident() error: " + e.Message);
                }
            }

            try
            {
                NotifyHeadPMRewviewWorker();
                Logger.Info("NotifyHeadPmReviewWorker() success ");
            }
            catch (Exception e)
            {
                Logger.Error("NotifyHeadPmReviewWorker() error: " + e.Message);
            }

            try
            {
                SendMessageToPmDueDate();
            }
            catch (Exception e)
            {
                Logger.Error("SendMessageToPmDueDate() error: " + e.Message);
            }
            try
            {
                NotifyToPresident();
                Logger.Info("NotifyToPresident() success");
            }
            catch (Exception e)
            {
                Logger.Error("NotifyToPresident() error: " + e.Message);
            }
            try
            {
                ResetData();
            }
            catch (Exception e)
            {
                Logger.Error("ResetData() error: " + e.Message);
            }
        }

        private async Task UpdateTimeCronjob()
        {
            string notifyReviewInternEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternEnableWorker);
            if (notifyReviewInternEnableWorker == "false")
            {
                Logger.Info("UpdateTimeCronjob() stop: notifyReviewInternEnableWorker=" + notifyReviewInternEnableWorker);
                return;
            }

            DateTime now = DateTime.Now;

            int updateTimeCronjobReviewInternOnDdate = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.UpdateTimeCronjobOnDate));
            if (updateTimeCronjobReviewInternOnDdate != now.Day)
            {
                Logger.Info("UpdateTimeCronjob() stop: updateTimeCronjobReviewInternOnDdate= " + updateTimeCronjobReviewInternOnDdate);
                return;
            }

            string updateTimeCronjobReviewInternAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.UpdateTimeCronjobAtHour);
            if (updateTimeCronjobReviewInternAtHour != now.Hour.ToString())
            {
                Logger.Info("UpdateTimeCronjob() stop: updateTimeCronjobReviewInternAtHour= " + updateTimeCronjobReviewInternAtHour);
                return;
            }

            int reviewDeadline = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyReviewDeadline));
            DateTime deadlineDate = new DateTime(now.Year, now.Month, reviewDeadline);
            NotifyReviewInternViaMezonAndEmailConfigDto NRITVMAEConfigDto = await _configurationAppService.GetNRITVMAEConfig();
            switch (deadlineDate.DayOfWeek)
            {
                case DayOfWeek.Friday:
                case DayOfWeek.Saturday: 
                    NRITVMAEConfigDto.NotifyHeadPMReviewInternOnDate = "8"; 
                    NRITVMAEConfigDto.NotifyPresidentReviewInternOnDate = "9";
                    break;
                case DayOfWeek.Sunday:
                    NRITVMAEConfigDto.NotifyHeadPMReviewInternOnDate = "7"; 
                    NRITVMAEConfigDto.NotifyPresidentReviewInternOnDate = "8";
                    break;
                case DayOfWeek.Thursday:
                    NRITVMAEConfigDto.NotifyHeadPMReviewInternOnDate = "6"; 
                    NRITVMAEConfigDto.NotifyPresidentReviewInternOnDate = "9";
                    break;
                default:
                    NRITVMAEConfigDto.NotifyHeadPMReviewInternOnDate = "6"; 
                    NRITVMAEConfigDto.NotifyPresidentReviewInternOnDate = "7";
                    break;
            }

            NotifyReviewInternViaMezonAndEmailConfigDto newNRITVMAEConfigDto = await _configurationAppService.SetNRITVMAEConfig(NRITVMAEConfigDto);
            Logger.Info("UpdateTimeCronjob() successfully");
        }

        private bool SendMailToHeadPmReviewWorker()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyReviewInternEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternEnableWorker);
            if (notifyReviewInternEnableWorker == "false")
            {
                Logger.Info("SendMailToHeadPmReviewWorker() stop: notifyReviewInternEnableWorker=" + notifyReviewInternEnableWorker);
                return false;
            }

            int notifyReviewInternAtHour = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternAtHour));
            if (dateNow.Hour != notifyReviewInternAtHour)
            {
                Logger.Info("SendMailToHeadPmReviewWorker() stop: notifyReviewInternAtHour=" + notifyReviewInternAtHour);
                return false;
            }

            int notifyHeadPmReviewInternOnDate = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMReviewInternOnDate));
            if (dateNow.Day != notifyHeadPmReviewInternOnDate)
            {
                Logger.Info("SendMailToHeadPmReviewWorker() stop: notifyHeadPmReviewInternOnDate = " + notifyHeadPmReviewInternOnDate);
                return false;
            }

            long reviewId = _reviewInternService.LastIdReviewIntern();
            bool check = _reviewDetailAppService.GetReviewInternStatusSummary(reviewId, ReviewInternStatus.PmReviewed).hasAllSameStatus;
            if (!check) return false;

            string headPmMail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPmMail);

            _reviewDetailAppService.SendMailToNotifyTransition(headPmMail, ReviewInternStatus.PmReviewed, reviewId, notifyHeadPmReviewInternOnDate);
            return true;
        }

        private void NotifyHeadPMRewviewWorker()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyReviewInternEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternEnableWorker);
            if (notifyReviewInternEnableWorker == "false")
            {
                Logger.Info("NotifyHeadPMRewviewWorker() stop: notifyReviewInternEnableWorker=" + notifyReviewInternEnableWorker);
                return;
            }

            string headPmEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPmMail);
            string usernameHeadPm = headPmEmail.Split('@')[0];
            (int startHour, int endHour, bool isFullday) = GetReviewInternTimeRange(headPmEmail);
            if (dateNow.Hour < startHour || dateNow.Hour > endHour)
            {
                Logger.Info("NotifyHeadPMRewviewWorker() stop at hour:" + dateNow.Hour);
                return;
            }
            int notifyHeadPmReviewInternOnDate = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMReviewInternOnDate));
            if (dateNow.Day != notifyHeadPmReviewInternOnDate)
            {
                Logger.Info("SendMailToHeadPmReviewWorker() stop: notifyHeadPmReviewInternOnDate = " + notifyHeadPmReviewInternOnDate);
                return;
            }

            long reviewId = _reviewInternService.LastIdReviewIntern();
            int totalPendingInterns = _reviewDetailAppService.GetReviewInternStatusSummary(reviewId, ReviewInternStatus.PmReviewed).totalPendingInterns;
            if (totalPendingInterns == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine($"[Review Intern] PMs have finished evaluating interns, please review.");
            _komuService.SendMessageReviewInternToUser(sb.ToString(), usernameHeadPm.Trim());
            sb.Clear();
        }

        private void SendMessageToPmDueDate()
        {
            var today = DateTimeUtils.GetNow();
            string notifyReviewInternEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternEnableWorker);
            if (notifyReviewInternEnableWorker == "false")
            {
                Logger.Info("SendMessageToPmDueDate() stop: notifyReviewInternEnableWorker=" + notifyReviewInternEnableWorker);
                return;
            }

            (int startHour, int endHour, bool isFullday) = TimeHelper.GetTimeRangeNotifyPmReviewIntern(notifyAtHourConfig);
            if (isFullday && (today.Hour < startHour || today.Hour > endHour))
            {
                Logger.Error("NotifyReviewerIntern() stop: fullday - notifyAtHourConfig=" + notifyAtHourConfig);
                return;
            }

            int reviewDeadline = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyReviewDeadline));
            DateTime deadlineDate = new DateTime(today.Year, today.Month, reviewDeadline);

            bool isWeekend = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday;
            bool isMondayAfterFifth = today.DayOfWeek == DayOfWeek.Monday && (today.AddDays(-2).Day == reviewDeadline || today.AddDays(-1).Day == reviewDeadline);

            string notifyPenaltyFee = SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyPenaltyFee);

            if ((today.Day != reviewDeadline || isWeekend) && !isMondayAfterFifth) return;

            long reviewId = _reviewInternService.LastIdReviewIntern();
            var listPMNotReview = _reviewInternService.GetListPmNotReview(reviewId);

            if (listPMNotReview.Count == 0) return; 

            var sb = new StringBuilder();
            var interns = new StringBuilder();
            foreach (var item in listPMNotReview)
            {
                if (!isFullday && (today.Hour < Convert.ToInt16(item.StartWorkingAt.Split(':')[0]) || today.Hour > Convert.ToInt16(item.EndWorkingAt.Split(':')[0]))) 
                {
                    Logger.Error("NotifyReviewerIntern() stop: working_time - notifyAtHourConfig=" + notifyAtHourConfig);
                    continue;
                }
                sb.AppendLine($"PM: {item.KomuAccountTag()} Please complete reviewing **{item.InterShips.Count}** interns before " +
                                $"**{DateTimeUtils.ToString(deadlineDate.Date)}** (**{notifyPenaltyFee}đ/intern** if you miss. Don't lose your money):");
                interns.AppendLine($"```");
                foreach (var interShip in item.InterShips)
                {
                    interns.AppendLine($"[{interShip.BranchDisplayName}] {interShip.FullName} - {CommonUtils.UserLevelName(interShip.Level)}");
                }
                interns.AppendLine($"```");
                _komuService.SendMessageReviewInternToUser(sb.ToString(), item.UserName.Trim(), interns.ToString());

                interns.Clear();
                sb.Clear();
            }
        }

        private bool SendMailToPresident()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyReviewInternEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternEnableWorker);
            if (notifyReviewInternEnableWorker == "false")
            {
                Logger.Info("SendMailToPresident() stop: notifyReviewInternEnableWorker=" + notifyReviewInternEnableWorker);
                return false;
            }

            int notifyReviewInternAtHour = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternAtHour));
            if (dateNow.Hour != notifyReviewInternAtHour)
            {
                Logger.Error("SendMailToPresident() stop: notifyReviewInternAtHour= " + notifyReviewInternAtHour);
                return false;
            }

            int dateSendMailToPresident = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentReviewInternOnDate));
            if (dateNow.Day != dateSendMailToPresident)
            {
                Logger.Error("SendMailToPresident() stop: dateSendMailToPresident= " + dateSendMailToPresident);
                return false;
            }
            long reviewId = _reviewInternService.LastIdReviewIntern();
            bool check = _reviewDetailAppService.GetReviewInternStatusSummary(reviewId, ReviewInternStatus.Reviewed).hasAllSameStatus;
            if (!check)
            {
                Logger.Error("The PM or headPM has not finished evaluating the interns.");
                return false;
            }
            string presidentEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentEmail);

            _reviewDetailAppService.SendMailToNotifyTransition(presidentEmail, ReviewInternStatus.Reviewed, reviewId, dateSendMailToPresident);
            _reviewDetailAppService.SendDirectMessageToNotifyTransition(presidentEmail, ReviewInternStatus.Reviewed, reviewId, dateSendMailToPresident);
            return true;
        }

        private void NotifyToPresident()
        {
            var dateNow = DateTimeUtils.GetNow();
            string notifyReviewInternEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyReviewInternEnableWorker);
            if (notifyReviewInternEnableWorker == "false")
            {
                Logger.Info("NotifyToPresident() stop: notifyReviewInternEnableWorker=" + notifyReviewInternEnableWorker);
                return;
            }

            int notifyPresidentReviewInternOnDate = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentReviewInternOnDate));
            if (dateNow.Day != notifyPresidentReviewInternOnDate)
            {
                Logger.Error("NotifyToPresident() stop: notifyPresidentReviewInternOnDate=" + notifyPresidentReviewInternOnDate);
                return;
            }

            string presidentEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentEmail);
            string username = presidentEmail.Split('@')[0];
            (int startHour, int endHour, bool isFullday) = GetReviewInternTimeRange(presidentEmail);
            if (dateNow.Hour < startHour || dateNow.Hour > endHour)
            {
                Logger.Error("NotifyToPresident() stop at " + dateNow.Hour);
                return;
            }
            long reviewId = _reviewInternService.LastIdReviewIntern();
            int totalPendingInterns = _reviewDetailAppService.GetReviewInternStatusSummary(reviewId, ReviewInternStatus.Reviewed).totalPendingInterns;
            if (totalPendingInterns == 0)
            {
                Logger.Error("The PM or headPM has not finished evaluating the interns.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[Review Intern] HeadPm have finished evaluating interns, please review.");
            _komuService.SendMessageReviewInternToUser(sb.ToString(), username.Trim());

            sb.Clear();
            
        }

        private void ResetData()
        {
            var today = DateTimeUtils.GetNow();
            if (today.Day != 1)  return;
            _isSendMailToHeadPm = true;
            _isSendMailToGD = true;
        }

        private (int startHour, int endHour, bool isFullday) GetReviewInternTimeRange(string receiverEmail)
        { 
            (int startHour, int endHour, bool isFullday) = TimeHelper.GetTimeRangeNotifyPmReviewIntern(notifyAtHourConfig);
            if (!isFullday)
            {
                var user = _userServices.GetUserByEmail(receiverEmail);
                startHour = Convert.ToInt16(user.MorningStartAt.Split(':')[0]);
                endHour = Convert.ToInt16(user.AfternoonEndAt.Split(':')[0]);
            }
            return (startHour, endHour, isFullday);
        }
    }
}
