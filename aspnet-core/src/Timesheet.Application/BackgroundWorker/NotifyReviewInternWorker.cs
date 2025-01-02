
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

namespace Timesheet.BackgroundWorker  
{
    public class NotifyReviewInternWorker: PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ReviewDetailAppService _reviewDetailAppService;
        private readonly ReviewInternServices _reviewInternService;
        private readonly KomuService _komuService;
        private int _intervalMinutes = 30;// 30 minutes
        private bool _isSendMailToHeadPm = true;
        private bool _isSendMailToGD = true;

        public NotifyReviewInternWorker(AbpTimer timer, ReviewDetailAppService reviewDetailAppService, ReviewInternServices reviewInternServices, KomuService komuService) : base(timer)
        {
            _reviewDetailAppService = reviewDetailAppService;
            _reviewInternService = reviewInternServices;
            _komuService = komuService;
            Timer.Period = 1000 * 60 * _intervalMinutes;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
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

        private bool SendMailToHeadPmReviewWorker()
        {
            var dateNow = DateTimeUtils.GetNow();
            string sendMailToHeadPmEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMAndPresidentReviewInternEnableWorker);
            if (sendMailToHeadPmEnableWorker == "false")
            {
                Logger.Info("SendMailToHeadPmReviewWorker() stop: sendMailToHeadPmEnableWorker=" + sendMailToHeadPmEnableWorker);
                return false;
            }

            int sendMailToHeadPmAtHour = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMAndPresidentReviewInternAtHour));
            if (dateNow.Hour != sendMailToHeadPmAtHour)
            {
                Logger.Info("SendMailToHeadPmReviewWorker() stop: sendMailToHeadPmAtHour=" + sendMailToHeadPmAtHour);
                return false;
            }

            long reviewId = _reviewInternService.LastIdReviewIntern();
            bool check = _reviewDetailAppService.HasRemainingInternReviewed(reviewId, ReviewInternStatus.PmReviewed);
            if (!check) return false;

            string headPmMail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPmMail);
            string usernameHeadPm = headPmMail.Split('@')[0];

            int dateSendMailToHeadPm = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMAndPresidentReviewInternOnDate));
            bool isWeekend = dateNow.DayOfWeek == DayOfWeek.Saturday || dateNow.DayOfWeek == DayOfWeek.Sunday;
            bool isSixthDay = dateNow.Day == dateSendMailToHeadPm;
            bool isMondayAfterSixth = dateNow.DayOfWeek == DayOfWeek.Monday && (dateNow.AddDays(-2).Day == dateSendMailToHeadPm || dateNow.AddDays(-1).Day == dateSendMailToHeadPm);

            if ((isSixthDay && !isWeekend) || isMondayAfterSixth)
            {
                 _reviewDetailAppService.SendMailToNotifyTransition(headPmMail, usernameHeadPm, ReviewInternStatus.PmReviewed, reviewId);
                return true;
            }

            return false;
        }
        
        private void NotifyHeadPMRewviewWorker()
        {
            var dateNow = DateTimeUtils.GetNow();
            int sendMailToHeadPmAtHour = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMAndPresidentReviewInternAtHour));
            if (dateNow.Hour < sendMailToHeadPmAtHour) return;

            long reviewId = _reviewInternService.LastIdReviewIntern();
            bool check = _reviewDetailAppService.HasRemainingInternReviewed(reviewId, ReviewInternStatus.PmReviewed);
            if (!check) return;

            int dateSendMailToHeadPm = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMAndPresidentReviewInternOnDate));
            bool isWeekend = dateNow.DayOfWeek == DayOfWeek.Saturday || dateNow.DayOfWeek == DayOfWeek.Sunday;
            bool isSixthDay = dateNow.Day == dateSendMailToHeadPm;
            bool isMondayAfterSixth = dateNow.DayOfWeek == DayOfWeek.Monday && (dateNow.AddDays(-2).Day == dateSendMailToHeadPm || dateNow.AddDays(-1).Day == dateSendMailToHeadPm);

            if ((isSixthDay && !isWeekend) || isMondayAfterSixth)
            {
                string headPmEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPmMail);
                string usernameHeadPm = headPmEmail.Split('@')[0];

                var sb = new StringBuilder();
                sb.AppendLine($"[Review Intern] PMs have finished evaluating interns, please review.");
                _komuService.SendMessageToUser(sb.ToString(), usernameHeadPm.Trim());
                sb.Clear();
            }
        }

        private void SendMessageToPmDueDate()
        {
            var today = DateTimeUtils.GetNow();

            int notifyAtHourConfig = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyAtHour));

            if (today.Hour < notifyAtHourConfig)
            {
                Logger.Error("NotifyReviewerIntern() stop: notifyAtHourConfig=" + notifyAtHourConfig);
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
            foreach (var item in listPMNotReview)
            {
                sb.AppendLine($"PM: {item.KomuAccountTag()} please complete reviewing **{item.InterShips.Count}** interns before " +
                                $"**{DateTimeUtils.ToString(deadlineDate.Date)}** (**{notifyPenaltyFee}đ/intern** if you miss. Don't lose your money):");
                sb.AppendLine($"```");
                foreach (var interShip in item.InterShips)
                {
                    sb.AppendLine($"{interShip.FullName} [{interShip.BranchDisplayName}] ({CommonUtils.UserLevelName(interShip.Level)})");
                }
                sb.AppendLine($"```");

                _komuService.SendMessageToUser(sb.ToString(), item.UserName.Trim());
                sb.Clear();
            }
        }

        private bool SendMailToPresident()
        {
            var dateNow = DateTimeUtils.GetNow();
            int dateSendMailToHeadPm = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMAndPresidentReviewInternOnDate));
            if (dateNow.Day < dateSendMailToHeadPm)
            {
                Logger.Error("The mailing date must be after the date " + dateSendMailToHeadPm);
                return false;
            }
            if (dateNow.Hour < 10)
            {
                Logger.Error("Mail must be sent after 10am");
                return false;
            }
            long reviewId = _reviewInternService.LastIdReviewIntern();
            bool check = _reviewDetailAppService.HasRemainingInternReviewed(reviewId, ReviewInternStatus.Reviewed);
            if (!check)
            {
                Logger.Error("The PM or headPM has not finished evaluating the interns.");
                return false;
            }
            bool isWeekend = dateNow.DayOfWeek == DayOfWeek.Saturday || dateNow.DayOfWeek == DayOfWeek.Sunday;

            if (isWeekend) return false;

            string presidentEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentEmail);
            string username = presidentEmail.Split('@')[0];

            _reviewDetailAppService.SendMailToNotifyTransition(presidentEmail, username, ReviewInternStatus.Reviewed, reviewId);
            return true;
        }

        private void NotifyToPresident()
        {
            var dateNow = DateTimeUtils.GetNow();
            int dateSendMailToHeadPm = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyHeadPMAndPresidentReviewInternOnDate));
            if (dateNow.Day < dateSendMailToHeadPm)
            {
                Logger.Error("The mailing date must be after the date " + dateSendMailToHeadPm);
                return;
            }
            if (dateNow.Hour < 10)
            {
                Logger.Error("Mail must be sent after 10am");
                return;
            }
            long reviewId = _reviewInternService.LastIdReviewIntern();
            bool check = _reviewDetailAppService.HasRemainingInternReviewed(reviewId, ReviewInternStatus.Reviewed);
            if (!check)
            {
                Logger.Error("The PM or headPM has not finished evaluating the interns.");
                return;
            }

            bool isWeekend = dateNow.DayOfWeek == DayOfWeek.Saturday || dateNow.DayOfWeek == DayOfWeek.Sunday;

            if (isWeekend) return;

            string presidentEmail = SettingManager.GetSettingValueForApplication(AppSettingNames.NotifyPresidentEmail);
            string username = presidentEmail.Split('@')[0];

            var sb = new StringBuilder();
            sb.AppendLine($"[Review Intern] HeadPm have finished evaluating interns, please review.");
            _komuService.SendMessageToUser(sb.ToString(), username.Trim());
            sb.Clear();
            
        }

        private void ResetData()
        {
            var today = DateTimeUtils.GetNow();
            if (today.Day != 1)  return;
            _isSendMailToHeadPm = true;
            _isSendMailToGD = true;
        }
    }
}
