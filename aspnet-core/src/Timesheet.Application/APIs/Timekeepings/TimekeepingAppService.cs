using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Timesheet.APIs.ReviewDetails.Dto;
using Timesheet.APIs.Timekeepings.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using static Sieve.Extensions.MethodInfoExtended;

namespace Timesheet.APIs.Timekeepings
{
    [AbpAuthorize]
    public class TimekeepingAppService : AppServiceBase
    {
        private readonly ITimekeepingServices timekeepingServices;
        //private readonly FaceIdService faceIdService;
        public TimekeepingAppService(TimekeepingServices timekeepingServices, IWorkScope workScope) : base(workScope)
        {
            //this.faceIdService = faceIdService;
            this.timekeepingServices = timekeepingServices;
        }


        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_View)]
        [HttpPost]
        public async Task<GridResult<GetTardinessUserDto>> GetAllPagging(GridParam input, int year, int month, long? branchId, long? userId)
        {
            var m = Int64.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));
            var viewAll = PermissionChecker.IsGranted(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_View);
            var tk = from t in WorkScope.GetAll<Timekeeping>()
                     where t.DateAt.Year == year && t.DateAt.Month == month
                        && (!userId.HasValue || t.UserId == userId)
                        && (!branchId.HasValue || t.User.BranchId == branchId)
                        && t.User.IsActive
                        && (viewAll || t.UserId == AbpSession.UserId)
                     group t by new { t.UserId, t.UserEmail, t.User.Branch.Color, t.User.Branch.DisplayName, t.User.AvatarPath, t.User.FullName, t.User.Type } into g
                     select new GetTardinessUserDto
                     {
                         UserId = g.Key.UserId,
                         UserName = g.Key.FullName,
                         UserType = g.Key.Type,
                         UserEmail = g.Key.UserEmail,
                         AvatarPath = g.Key.AvatarPath,
                         BranchColor = g.Key.Color,
                         BranchDisplayName = g.Key.DisplayName,
                         NumberOfTardies = g.Count(x => x.IsPunishedCheckIn),
                         NumberOfLeaveEarly = g.Count(x => x.IsPunishedCheckOut)
                     };
            return await tk.GetGridResult(tk, input);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_View)]
        [HttpGet]
        public async Task<List<GetTimekeepingUserDto>> GetDetailTimekeeping(int year, int month, int? day, long? userId, long? branchId, bool? isPunished, bool? isComplain, CheckInCheckOutPunishmentType? statusPunish)
        {
            //var m = Int64.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));
            //var viewAll = PermissionChecker.IsGranted(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_View);
            var q =  (from t in WorkScope.GetAll<Timekeeping>()
                     join u in WorkScope.GetAll<User>() on t.LastModifierUserId equals u.Id into tu
                     where (!userId.HasValue || t.UserId == userId)
                       && (t.DateAt.Year == year && t.DateAt.Month == month)
                       && (!day.HasValue || day.Value < 0 || day == t.DateAt.Day)
                       && (!branchId.HasValue || branchId == t.User.BranchId)
                       && (!isPunished.HasValue || isPunished == t.IsPunishedCheckIn)
                       && (!isComplain.HasValue || String.IsNullOrEmpty(t.UserNote) != isComplain)
                       &&(!statusPunish.HasValue || (statusPunish.Value == CheckInCheckOutPunishmentType.NoDaily && t.CountPunishDaily > 0) || (statusPunish.Value == CheckInCheckOutPunishmentType.NoReplyMention && t.CountPunishMention > 0) || t.StatusPunish == statusPunish)
                     select new GetTimekeepingUserDto
                     {
                         UserId = t.UserId,
                         UserName = t.User.FullName,
                         UserType = t.User.Type,
                         UserEmail = t.UserEmail,
                         Branch = t.User.BranchOld,
                         BranchColor = t.User.Branch.Color,
                         BranchDisplayName = t.User.Branch.DisplayName,
                         BranchId = t.User.Branch.Id,
                         AvatarPath =  t.User.AvatarPath,
                         Date = t.DateAt,
                         TimekeepingId = t.Id,
                         RegistrationTimeStart = t.RegisterCheckIn,
                         RegistrationTimeEnd = t.RegisterCheckOut,
                         CheckIn = t.CheckIn,
                         CheckOut = t.CheckOut,
                         ResultCheckIn = CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn),
                         ResultCheckOut = CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut),
                         EditByUserName = tu.FirstOrDefault().UserName,
                         Status = t.IsPunishedCheckIn ? PunishmentStatus.Punish : PunishmentStatus.Normal,
                         EditByUserId = t.LastModifierUserId,
                         UserNote = t.UserNote,
                         NoteReply = t.NoteReply,
                         TrackerTime = t.TrackerTime,
                         StatusPunish = t.StatusPunish,
                         MoneyPunish = t.MoneyPunish,
                         DailyPunish = t.CountPunishDaily,
                         MentionPunish = t.CountPunishMention,
                     });

            if (isComplain.HasValue && isComplain.Value)
            {
                q = q.OrderByDescending(t => t.Status).ThenByDescending(s => s.ResultCheckIn).ThenByDescending(s => s.Date);
            }
            else
            {
                q = q.OrderByDescending(t => t.Date).ThenByDescending(s => s.Status).ThenByDescending(s => s.ResultCheckIn);
            }
            return await q.ToListAsync();
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimeSheet_ViewMyTardinessDetail)]
        public async Task<List<GetTimekeepingUserDto>> GetMyDetails(int year, int month)
        {
            return await (from t in WorkScope.GetAll<Timekeeping>().Where(s => s.DateAt.Year == year && s.DateAt.Month == month && s.UserId == AbpSession.UserId)
                          join u in WorkScope.GetAll<User>() on t.LastModifierUserId equals u.Id into uu

                          select new GetTimekeepingUserDto
                          {
                              UserId = t.UserId,
                              UserName = t.User.FullName,
                              UserType = t.User.Type,
                              UserEmail = t.UserEmail,
                              Branch = t.User.BranchOld,
                              AvatarPath = t.User.AvatarPath,
                              Date = t.DateAt,
                              TimekeepingId = t.Id,
                              RegistrationTimeStart = t.RegisterCheckIn,
                              RegistrationTimeEnd = t.RegisterCheckOut,
                              CheckIn = t.CheckIn,
                              CheckOut = t.CheckOut,
                              ResultCheckIn = CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn),
                              ResultCheckOut = CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut),
                              EditByUserName = uu.FirstOrDefault().UserName,
                              Status = t.IsPunishedCheckIn ? PunishmentStatus.Punish : PunishmentStatus.Normal,
                              EditByUserId = t.LastModifierUserId,
                              UserNote = t.UserNote,
                              NoteReply = t.NoteReply,
                              StatusPunish = t.StatusPunish,
                              MoneyPunish = t.MoneyPunish,
                              TrackerTime = t.TrackerTime,
                              DailyPunish = t.CountPunishDaily,
                              MentionPunish = t.CountPunishMention
                          }).OrderByDescending(t => t.Date).ToListAsync();
        }

        private async Task<CheckInOutTimeDto> CaculateRemoteInfo(Timekeeping timekeeping)
        {
            var t = new CheckInOutTimeDto { };
            t.isRemote = false;
            // Mốc phạt KomuTracker: 0-2h, 2-4h, 4-6h, 6-7h
            t.TrackerTimeLevel = new List<float>() { 2 * 60, 4 * 60, 6 * 60, 7 * 60 }; ;

            var AbsenceUsers = WorkScope.GetAll<AbsenceDayDetail>().Include(s => s.Request)
                .Where(s => s.DateAt.Date == timekeeping.DateAt.Date
                && s.Request.Status == RequestStatus.Approved
                && s.Request.UserId == timekeeping.UserId)
                .Select(s => new MapAbsenceUserDto
                {
                    UserId = s.Request.UserId,
                    DateType = s.DateType,//morning, afternoon, fullday, custom
                    AbsenceTime = s.AbsenceTime,//dau. giua, cuoi
                    Hour = s.Hour,
                    Type = s.Request.Type,
                }).OrderBy(s=>s.Type).ToList();

            var user = WorkScope.Get<User>(timekeeping.UserId.Value);

            foreach (var absenceUser in AbsenceUsers)
            {
                if(absenceUser.Type == RequestType.Off && absenceUser.DateType == DayType.Custom)
                {
                    // Mốc phạt KomuTracker: 0-2h, 2-3h, 3-4h, 4 -> (7 - Thời gian DM/VS)h
                    t.TrackerTimeLevel = new List<float>() { 2 * 60, 3 * 60, 4 * 60, (7 - (float)absenceUser.Hour) * 60 };
                }
                else if(absenceUser.Type == RequestType.Remote)
                {
                    t.isRemote = true;
                    switch (absenceUser.DateType)
                    {
                        case DayType.Fullday:
                            {
                                break;
                            }
                        case DayType.Morning:
                            {
                                // Mốc phạt KomuTracker: 0-1h, 1-2h, 2-3h, 3-4h
                                t.TrackerTimeLevel = new List<float>() { 1 * 60, 2 * 60, 3 * 60, 4 * 60 };
                                break;
                            }
                        case DayType.Afternoon:
                            {
                                // Mốc phạt KomuTracker: 0-1h, 1-2h, 2-2.5h, 2.5-3h
                                t.TrackerTimeLevel = new List<float>() { 1 * 60, 2 * 60, 2.5f * 60, 3 * 60 };
                                break;
                            }
                    }
                }
            }
            return t;
        }

        private async Task SetMoneyPunishByType(Timekeeping timekeeping)
        {
            var checkInCheckOutPunishmentSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInCheckOutPunishmentSetting);
            var rs = JsonConvert.DeserializeObject<List<CheckInCheckOutPunishmentSettingDto>>(checkInCheckOutPunishmentSetting);
            var MoneyPunish = rs.Where(x => x.Id == timekeeping.StatusPunish).Select(x => x.Money).FirstOrDefault();
            if (timekeeping.CountPunishDaily > 0) MoneyPunish += rs.Where(x => x.Id == CheckInCheckOutPunishmentType.NoDaily).Select(x => x.Money).FirstOrDefault() * timekeeping.CountPunishDaily;
            if (timekeeping.CountPunishMention > 0) MoneyPunish += rs.Where(x => x.Id == CheckInCheckOutPunishmentType.NoReplyMention).Select(x => x.Money).FirstOrDefault() * timekeeping.CountPunishMention;

            var AbsenceUsers = await CaculateRemoteInfo(timekeeping);
            var trackerTime = DateTimeUtils.ConvertHHmmssToMinutes(timekeeping.TrackerTime);
            if (AbsenceUsers != null && AbsenceUsers.isRemote)
            {
                if (trackerTime <= AbsenceUsers.TrackerTimeLevel[0])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime0).Select(x => x.Money).FirstOrDefault();
                }
                else if (trackerTime <= AbsenceUsers.TrackerTimeLevel[1])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime1).Select(x => x.Money).FirstOrDefault();
                }
                else if (trackerTime <= AbsenceUsers.TrackerTimeLevel[2])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime2).Select(x => x.Money).FirstOrDefault();
                }
                else if (trackerTime <= AbsenceUsers.TrackerTimeLevel[3])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime3).Select(x => x.Money).FirstOrDefault();
                }
                else timekeeping.MoneyPunish = 0;
            }
            else timekeeping.MoneyPunish = 0;

            timekeeping.MoneyPunish += MoneyPunish;
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_Edit)]
        [HttpPost]
        public async Task<Timekeeping> UpdateTimekeeping(Timekeeping input)
        {
            if (string.IsNullOrEmpty(input.RegisterCheckIn)
                || string.IsNullOrEmpty(input.RegisterCheckOut))
            {
                throw new UserFriendlyException("RegisterCheckIn or RegisterCheckOut is null or empty");
            }
            if (input.Id <= 0)
            {
                throw new UserFriendlyException("Timekeeping id = 0 was not found!");
            }
            DateTime time1 = new DateTime();
            if (!(String.IsNullOrEmpty(input.CheckIn) || DateTime.TryParseExact(input.CheckIn, "HH:mm", null, System.Globalization.DateTimeStyles.None, out time1)) &&
                !(String.IsNullOrEmpty(input.CheckOut) || DateTime.TryParseExact(input.CheckOut, "HH:mm", null, System.Globalization.DateTimeStyles.None, out time1)))
            {
                throw new UserFriendlyException("Wrong time format (must be HH:mm)");
            }
            var LimitedMinute = Int32.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));
            var t = await WorkScope.GetAsync<Timekeeping>(input.Id);
            t.CheckIn = input.CheckIn;
            t.CheckOut = input.CheckOut;
            t.RegisterCheckIn = input.RegisterCheckIn;
            t.RegisterCheckOut = input.RegisterCheckOut;
            t.TrackerTime = input.TrackerTime;

            var AbsenceUsers = await CaculateRemoteInfo(t);

            await timekeepingServices.CheckIsPunished(t);
            await timekeepingServices.CheckIsPunishedByRule(t, LimitedMinute, DateTimeUtils.ConvertHHmmssToMinutes(input.TrackerTime), AbsenceUsers);
            await WorkScope.GetRepo<Timekeeping>().UpdateAsync(t);
            return t;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_UserNote)]
        [HttpPost]
        public async Task<Timekeeping> UserKhieuLai(TimekeepingUserNoteDto input)
        {
            var t = await WorkScope.GetAsync<Timekeeping>(input.Id);
            if (t != null && t.UserId != AbpSession.UserId.Value)
            {
                throw new UserFriendlyException("Bạn chỉ có thể khiếu lại cho bản ghi của mình");
            }

            try
            {
                t.UserNote = input.UserNote;
                await WorkScope.GetRepo<Timekeeping>().UpdateAsync(t);
                return t;
            } 
            catch (Exception ex)
            {
                Logger.Error("Error: " + ex.Message);
                throw new UserFriendlyException("An internal error occurred during your request!");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_ReplyUserNote)]
        [HttpPost]
        public async Task<Timekeeping> TraLoiKhieuLai(TimekeepingDto input)
        {
            var t = await WorkScope.GetAsync<Timekeeping>(input.Id);
            ObjectMapper.Map<TimekeepingDto, Timekeeping>(input, t);
            if (t.StatusPunish == CheckInCheckOutPunishmentType.NoPunish)
            {
                t.IsPunishedCheckIn = false;
            }
            if (new List<CheckInCheckOutPunishmentType> 
            { 
                CheckInCheckOutPunishmentType.Late,
                CheckInCheckOutPunishmentType.NoCheckIn,
                CheckInCheckOutPunishmentType.NoCheckOut,
                CheckInCheckOutPunishmentType.LateAndNoCheckOut,
                CheckInCheckOutPunishmentType.NoCheckInAndNoCheckOut 
            }.Contains(t.StatusPunish))

            {
                t.IsPunishedCheckIn = true;
            }
            await SetMoneyPunishByType(t);
            await WorkScope.GetRepo<Timekeeping>().UpdateAsync(t);
            return t;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_GetData)]
        [HttpPost]
        public async Task<List<Timekeeping>> AddTimekeepingByDay(string date)
        {
            if (string.IsNullOrEmpty(date))
                throw new UserFriendlyException(String.Format("Selected date is null!"));
            DateTime selectedDate = DateTime.Parse(date);
            if (selectedDate.Date > DateTimeUtils.GetNow().Date)
            {
                throw new UserFriendlyException(String.Format("The selected date cannot greater than the current date!"));
            }
            return await timekeepingServices.AddTimekeepingByDay(selectedDate.Date);
        }

        [HttpGet]
        public async Task<object> NoticePunishUserCheckInOut(DateTime date)
        {
            return await timekeepingServices.NoticePunishUserCheckInOut(date);
        }
    }
}
