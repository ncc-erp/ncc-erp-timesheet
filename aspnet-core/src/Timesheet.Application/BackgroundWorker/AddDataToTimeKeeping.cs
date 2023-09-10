using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Microsoft.AspNetCore.Http;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.TeamBuildingDetails;
using Timesheet.APIs.TeamBuildingDetails.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.FaceIdService;
using Timesheet.Services.HRM;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;
using Timesheet.UploadFilesService;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.BackgroundWorker
{
    public class AddDataToTimeKeeping : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<MyTimesheet, long> _myTimesheetRepository;
        private readonly IRepository<UnlockTimesheet, long> _unlockTimesheetRepository;
        private readonly ITimekeepingServices _timekeepingService;
        private readonly IReviewInternServices _reviewInternService;
        private readonly KomuService _komuService;
        private readonly TeamBuildingDetailsAppService _teamBuildingDetailsAppService;
        private readonly FaceIdService _faceIdService;
        private readonly IWorkScope _workScope;
        private readonly UploadAvatarService _filesService;
        private readonly ProjectService _projectService;
        private readonly HRMService _hrmService;
        public readonly IRetroServices _retroService;
        private readonly IApproveTimesheetServices _approveTimesheetServices;
        private readonly GenerateDataTeamBuildingServices _generateDataTeamBuildingServices;
        public AddDataToTimeKeeping(AbpTimer timer,
            IRepository<MyTimesheet, long> myTimesheetRepository,
                        IRepository<UnlockTimesheet, long> unlockTimesheetRepository,
                        ITimekeepingServices timekeepingService,
                        IReviewInternServices reviewInternService,
                        KomuService komuService,
                        TeamBuildingDetailsAppService teamBuildingDetailsAppService,
                        FaceIdService faceIdService,
                        IWorkScope workScope,
                        UploadAvatarService filesService,
                        ProjectService projectService,
                        HRMService hrmService,
                        IRetroServices retroServices,
                        IApproveTimesheetServices approveTimesheetServices,
                        GenerateDataTeamBuildingServices generateDataTeamBuildingServices
            ) : base(timer)
        {
            _myTimesheetRepository = myTimesheetRepository;
            _unlockTimesheetRepository = unlockTimesheetRepository;
            _timekeepingService = timekeepingService;
            _reviewInternService = reviewInternService;
            _komuService = komuService;
            _teamBuildingDetailsAppService = teamBuildingDetailsAppService;
            _faceIdService = faceIdService;
            _workScope = workScope;
            _filesService = filesService;
            _projectService = projectService;
            _hrmService = hrmService;
            _retroService = retroServices;
            _approveTimesheetServices = approveTimesheetServices;
            _generateDataTeamBuildingServices = generateDataTeamBuildingServices;
            Timer.Period = 1000 * 60 * 60; //1h
            //Timer.Period = 1000 * 30; //1h
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            DateTime now = DateTimeUtils.GetNow();
            try
            {
                AddTimkeeping(now);
            }
            catch (Exception e)
            {
                Logger.Error(e.StackTrace);
            }
            try
            {
                SubmitTimesheet(now);
            }
            catch (Exception e)
            {
                Logger.Error(e.StackTrace);
            }
            try
            {
                LockTimesheet(now);
            }
            catch (Exception e)
            {
                Logger.Error(e.StackTrace);
            }
            try
            {
                NotifyReviewerIntern();
            }
            catch (Exception e)
            {
                Logger.Error("NotifyReviewerIntern() error: " + e.Message);
            }
            try
            {
                AutoUpdateAvatar(now);
            }
            catch (Exception e)
            {
                Logger.Error("AutoUpdateAvt() error: " + e.Message);
            }
            //try
            //{
            //    NoticePunishUserCheckInOut(now);
            //}
            //catch (Exception e)
            //{
            //    Logger.Error("SendNoticeUserPunishedCheckIn() error: " + e.Message);
            //}
            try
            {
                AddTeamBuilding(now);
            }
            catch (Exception e)
            {
                Logger.Error("AddTeamBuilding() error: " + e.Message);
            }
            try
            {
                NotifyRetro();
            }
            catch (Exception e)
            {
                Logger.Error("NotifyRetro() error: " + e.Message);
            }
            try
            {
                NotifyApproveTimesheet();
            }
            catch (Exception e)
            {
                Logger.Error("NotifyApproveTimesheet() error: " + e.Message);
            }
        }

        private void AddTimkeeping(DateTime now)
        {
            DateTime yesterday = now.AddDays(-1).Date;
            string getDataAt = SettingManager.GetSettingValueForApplication(AppSettingNames.CheckInInternalAtHour);
            Logger.Info("AddDataToTimeKeeping.DoWork() getDataAt=" + getDataAt + ", now.Hour=" + now.Hour);
            if (now.Hour == int.Parse(getDataAt))
            {
                _timekeepingService.AddTimekeepingByDay(yesterday);
            }
        }

        private void SubmitTimesheet(DateTime now)
        {
            string AutoSubmitTSAt = SettingManager.GetSettingValueForApplication(AppSettingNames.AutoSubmitAt);
            string autoSubmitTimesheet = SettingManager.GetSettingValueForApplication(AppSettingNames.AutoSubmitTimesheet);
            string autoSubmitTSAtHour = SettingManager.GetSettingValueForApplication(AppSettingNames.AutoSubmitAtHour);
            DayOfWeek submitDay = Enum.Parse<DayOfWeek>(AutoSubmitTSAt);

            DateTime startDate = DateTimeUtils.GetStartDateForAutoSubmitTS(now);
            DateTime endDate = DateTimeUtils.GetEndDateForAutoSubmitTS(startDate);

            Logger.Info("SubmitTimesheet() check |Now.DayOfWeek = " + now.DayOfWeek.ToString() + ",now.Hour=" + now.Hour
                + "| AutoSubmitTSAt = " + AutoSubmitTSAt + " |autoSubmitTimesheet=" + autoSubmitTimesheet
                + " |monday=" + startDate + " |endDate=" + endDate);

            if (autoSubmitTimesheet == "true"
                && (now.DayOfWeek.ToString() == AutoSubmitTSAt && now.Hour == int.Parse(autoSubmitTSAtHour))
                || (now.Day == 1 && now.Hour == 0))
            {
                Logger.Info("SubmitTimesheet() started");
                var timesheetLogs = _myTimesheetRepository
                    .GetAllList()
                    .Where(t => t.DateAt >= startDate.Date && t.DateAt.Date <= endDate && t.Status == TimesheetStatus.None)
                    .ToList();

                foreach (var timesheet in timesheetLogs)
                {
                    timesheet.Status = TimesheetStatus.Pending;
                    timesheet.LastModificationTime = now;
                }

                CurrentUnitOfWork.SaveChanges();
                Logger.Info("SubmitTimesheet() end timesheetLogs.Count = " + timesheetLogs.Count);
            }
        }

        private void LockTimesheet(DateTime now)
        {
            string lockDayAfterUnlock = SettingManager.GetSettingValueForApplication(AppSettingNames.LockDayAfterUnlock);
            string lockHourAfterUnlock = SettingManager.GetSettingValueForApplication(AppSettingNames.LockHourAfterUnlock);
            Logger.Info("LockTimeSheet() check |date = " + now.DayOfWeek.ToString() + ", now.Hour=" + now.Hour
                + "| lockDayAfterUnlock = " + lockDayAfterUnlock + " |LockHourAfterUnlock=" + lockHourAfterUnlock);


            if (now.DayOfWeek.ToString() == lockDayAfterUnlock && now.Hour == int.Parse(lockHourAfterUnlock))
            {
                Logger.Info("LockTimeSheet() started");
                var timesheetLogs = _unlockTimesheetRepository.GetAllList().ToList();

                foreach (var timesheet in timesheetLogs)
                {
                    timesheet.IsDeleted = true;
                    timesheet.DeletionTime = now;
                }
                CurrentUnitOfWork.SaveChanges();
                Logger.Info("LockTimeSheet() end timesheetLogs.Count = " + timesheetLogs.Count);
            }
        }

        private void NotifyReviewerIntern()
        {
            Logger.Info("NotifyReviewerIntern() start");

            string notifyEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyEnableWorker);

            if (notifyEnableWorker != "true")
            {
                Logger.Info("NotifyReviewerIntern() stop: notifyEnableWorker=" + notifyEnableWorker);
                return;
            }

            var NRITNotifyOnDates = SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyOnDates);
            string[] notifyOnDates = NRITNotifyOnDates.Split(',');
            var today = DateTimeUtils.GetNow();
            Logger.Info("NotifyReviewerIntern() NRITNotifyOnDates=" + NRITNotifyOnDates + ",today.Day=" + today.Day + ", today.Hour=" + today.Hour);
            if (!notifyOnDates.Contains(today.Day.ToString()))
            {
                Logger.Info("NotifyReviewerIntern() stop: today is not in notifyOnDates=>stop");
                return;
            }

            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyAtHour);

            if (notifyAtHourConfig != today.Hour.ToString())
            {
                Logger.Info("NotifyReviewerIntern() stop: notifyAtHourConfig=" + notifyAtHourConfig);
                return;
            }

            string notifyToChannels = SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyToChannels);
            Logger.Info("NotifyReviewerIntern() notifyToChannels=" + notifyToChannels);

            long reviewId = _reviewInternService.LastIdReviewIntern();

            var listPMNotReview = _reviewInternService.GetListPmNotReview(reviewId);

            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();

            Logger.Info("NotifyReviewerIntern() countListChannel=" + countListChannel);

            int reviewDeadline = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyReviewDeadline));
            DateTime deadlineDate = new DateTime(today.Year, today.Month, reviewDeadline);
            string notifyPenaltyFee = SettingManager.GetSettingValueForApplication(AppSettingNames.NRITNotifyPenaltyFee);

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

        private void AutoUpdateAvatar(DateTime now)
        {
            var hourRun = 11;

            if (now.Hour != hourRun)
            {
                Logger.Info($"AutoUpdateAvatar(): Time != {hourRun}h");
                return;
            }

            var checkInImages = _faceIdService.GetAllImages();

            if (checkInImages == null || checkInImages.Count == 0)
            {
                Logger.Info("checkInImages null or empty => stop");
                return;
            }

            var noAvatarUsers = _workScope.GetAll<User>()
                .Where(x => x.AvatarPath.IsNullOrEmpty())
                .ToList();

            var usersAndImages = (from img in checkInImages
                                  join u in noAvatarUsers
                                  on img.Email equals u.EmailAddress
                                  select new
                                  {
                                      user = u,
                                      image = img.GetUrl(),
                                  }).ToList();

            if (usersAndImages == null || usersAndImages.Count() == 0)
            {
                Logger.Info("usersAndImages null or empty!");
                return;
            }

            using (WebClient webClient = new WebClient())
            {
                foreach (var userInfo in usersAndImages)
                {
                    try
                    {
                        byte[] dataArr = webClient.DownloadData(userInfo.image);
                        IFormFile file = FileUtils.CreateIFormFile(dataArr);
                        userInfo.user.AvatarPath = _filesService.UploadAvatarAsync(file).Result;
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Fail: {userInfo.user.EmailAddress}, Image: {userInfo.image} error: {e.Message}");
                        continue;
                    }
                }
            }
            CurrentUnitOfWork.SaveChanges();
            var avartarUserInfos = usersAndImages
                .Where(x => x.user.AvatarPath != null)
                .Select(x => new UpdateAvatarDto
                {
                    AvatarPath = x.user.AvatarPath,
                    EmailAddress = x.user.EmailAddress
                }).ToList();

            _projectService.UpdateAllAvatarToProject(avartarUserInfos);
            _hrmService.UpdateAllAvatarToHRM(avartarUserInfos);
        }

        private void NoticePunishUserCheckInOut(DateTime now)
        {
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                Logger.Info($"NoticePunishUserCheckInOut() {now.DayOfWeek.ToString()} => stop");
                return;
            }

            string enableRun = SettingManager.GetSettingValueForApplication(AppSettingNames.EnableNofityKomuCheckInOutPunishment);
            if (enableRun != "true")
            {
                Logger.Info($"NoticePunishUserCheckInOut() enable run = {enableRun} => stop");
                return;
            }

            string runAtHourStr = SettingManager.GetSettingValueForApplication(AppSettingNames.NofityKomuCheckInOutPunishmentAtHour);
            int runAtHour = 10;
            int.TryParse(runAtHourStr, out runAtHour);
            if (now.Hour != runAtHour)
            {
                Logger.Info($"NoticePunishUserCheckInOut() now.Hour ({now.Hour}) != runAtHour ({runAtHour}) => stop");
                return;
            }

            var yesterday = now.DayOfWeek == DayOfWeek.Monday ? now.AddDays(-3) : now.AddDays(-1);

            _timekeepingService.NoticePunishUserCheckInOut(yesterday);
        }

        private void NotifyRetro()
        {
            Logger.Info("NotifyRetro() start");
            string retroNotifyEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.RetroNotifyEnableWorker);

            if (retroNotifyEnableWorker != "true")
            {
                Logger.Info("NotifyRetro() stop: retroNotifyEnableWorker=" + retroNotifyEnableWorker);
                return;
            }

            var retroNotifyOnDates = SettingManager.GetSettingValueForApplication(AppSettingNames.RetroNotifyOnDates);
            string[] notifyOnDates = retroNotifyOnDates.Split(',');
            var today = DateTimeUtils.GetNow();
            Logger.Info("NotifyRetro() RetroNotifyOnDates=" + retroNotifyOnDates + ",today.Day=" + today.Day + ", today.Hour=" + today.Hour);

            if (!notifyOnDates.Contains(today.Day.ToString()))
            {
                Logger.Info("NotifyRetro() stop: today is not in retroNotifyOnDates=>stop");
                return;
            }

            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.RetroNotifyAtHour);

            if (notifyAtHourConfig != today.Hour.ToString())
            {
                Logger.Info("NotifyRetro() stop: notifyAtHourConfig=" + notifyAtHourConfig);
                return;
            }

            string notifyToChannels = SettingManager.GetSettingValueForApplication(AppSettingNames.RetroNotifyToChannels);
            Logger.Info("NotifyRetro() notifyToChannels=" + notifyToChannels);

            long retroId = _retroService.LastIdRetro();
            var listPMNotRetro = _retroService.GetListPmNotRetro(retroId);
            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();
            Logger.Info("NotifyRetro() countListChannel=" + countListChannel);

            int retroDeadline = Convert.ToInt16(SettingManager.GetSettingValueForApplication(AppSettingNames.RetroNotifyDeadline));
            DateTime deadlineDate = new DateTime(today.Year, today.Month, retroDeadline);
            var sb = new StringBuilder();

            foreach (var item in listPMNotRetro)
            {
                if (item.Projects.Count == 1)
                {
                    sb.AppendLine($"PM: {item.KomuAccountTag()} please complete retro for **{item.Projects.Count}** project before " +
                              $"**{DateTimeUtils.ToString(deadlineDate.Date)}**");
                }
                else
                {
                    sb.AppendLine($"PM: {item.KomuAccountTag()} please complete retro for **{item.Projects.Count}** projects before " +
                              $"**{DateTimeUtils.ToString(deadlineDate.Date)}**");
                }

                sb.AppendLine($"```");

                foreach (var project in item.Projects)
                {
                    sb.AppendLine($"{project.Name} [{project.Code}]");
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
        private void AddTeamBuilding(DateTime now)
        {
            DateTime lastMonth = now.AddMonths(-1);
            string getDataAt = SettingManager.GetSettingValueForApplication(AppSettingNames.GenerateDataOnDate);
            int day;

            if (!int.TryParse(getDataAt, out day))
                day = 6;

            if (day != now.Day)
            {
                Logger.Info($"AddTeamBuilding() now.Day ({now.Day}) != GenerateDataOnDate ({day}) => stop");
                return;
            }

            var applyDate = new InputGenerateDataTeamBuildingDto
            {
                Month = lastMonth.Month,
                Year = now.Year,
                Day = day,
            };

            Logger.Info("AddTeamBuilding.DoWork() getDataAt=" + getDataAt + ", now.Date=" + now.Date);
            _generateDataTeamBuildingServices.AddDataToTeamBuildingDetail(applyDate);
        }

        private void NotifyApproveTimesheet()
        {
            Logger.Info("NotifyApproveTimesheet() start");

            string notifyEnableWorker = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveTimesheetNotifyEnableWorker);

            if (notifyEnableWorker != "true")
            {
                Logger.Info("NotifyApproveTimesheet() stop: notifyEnableWorker=" + notifyEnableWorker);
                return;
            }

            var ApproveNotifyOnDates = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveTimesheetNotifyOnDates);
            string[] notifyOnDates = ApproveNotifyOnDates.Split(',');
            var today = DateTimeUtils.GetNow();
            Logger.Info("NotifyApproveTimesheet() notifyOnDates=" + ApproveNotifyOnDates + ",today.Day=" + today.Day + ", today.Hour=" + today.Hour);

            if (!notifyOnDates.Contains(today.Day.ToString()))
            {
                Logger.Info("NotifyApproveTimesheet() stop: today is not in notifyOnDates=>stop");
                return;
            }

            string notifyAtHourConfig = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveTimesheetNotifyAtHour);

            if (notifyAtHourConfig != today.Hour.ToString())
            {
                Logger.Info("NotifyApproveTimesheet() stop: notifyAtHourConfig=" + notifyAtHourConfig);
                return;
            }

            string notifyToChannels = SettingManager.GetSettingValueForApplication(AppSettingNames.ApproveTimesheetNotifyToChannels);
            Logger.Info("NotifyApproveTimesheet() notifyToChannels=" + notifyToChannels);

            var listPMNotApprove = _approveTimesheetServices.GetListPmNotApproveTimesheet();

            string[] arrListChannel = notifyToChannels.Split(',');
            int countListChannel = arrListChannel.Count();

            Logger.Info("NotifyApproveTimesheet() countListChannel=" + countListChannel);

            var sb = new StringBuilder();
            foreach (var item in listPMNotApprove)
            {
                if (item.CountUsers == 1)
                {
                    sb.AppendLine($"PM: {item.KomuAccountTag()} please approve timesheet **{item.CountUsers}** user");
                }
                else
                {
                    sb.AppendLine($"PM: {item.KomuAccountTag()} please approve timesheet **{item.CountUsers}** users");
                }

                sb.AppendLine($"```");

                foreach (var user in item.Users)
                {
                    sb.AppendLine($"{user.FullName} - Submit Week for Approval: {user.DateAt.ToString("dd/MM/yyyy")}");
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

