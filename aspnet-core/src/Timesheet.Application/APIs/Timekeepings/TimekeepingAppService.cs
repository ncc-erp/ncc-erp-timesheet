using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using DocumentFormat.OpenXml.Spreadsheet;
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
                     where t.DateAt.Year == year && t.DateAt.Month == month &&
                       (!userId.HasValue || t.UserId == userId) &&
                       (!branchId.HasValue || t.User.BranchId == branchId) &&
                       t.User.IsActive &&
                       (viewAll || t.UserId == AbpSession.UserId)
                     group t by new
                     {
                         t.UserId,
                         t.UserEmail,
                         t.User.Branch.Color,
                         t.User.Branch.DisplayName,
                         t.User.AvatarPath,
                         t.User.FullName,
                         t.User.Type
                     }
            into g
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
            var q = (from t in WorkScope.GetAll<Timekeeping>()
                     join u in WorkScope.GetAll<User>() on t.LastModifierUserId equals u.Id into tu
                     where (!userId.HasValue || t.UserId == userId) &&
              (t.DateAt.Year == year && t.DateAt.Month == month) &&
              (!day.HasValue || day.Value < 0 || day == t.DateAt.Day) &&
              (!branchId.HasValue || branchId == t.User.BranchId) &&
              (!isPunished.HasValue || isPunished == t.IsPunishedCheckIn) &&
              (!isComplain.HasValue || String.IsNullOrEmpty(t.UserNote) != isComplain) &&
              (!statusPunish.HasValue || t.StatusPunish == statusPunish)
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
                         AvatarPath = t.User.AvatarPath,
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

        private UserPunishmentSummaryDto CalculateUserPunishmentSummary(
            List<UserPunishment> userPunishments,
            List<Timekeeping> timekeepings)
        {
            return new UserPunishmentSummaryDto
            {
                TotalLate = userPunishments.Where(x => x.Type == UserPunishmentType.Late).Sum(x => x.TotalMoney),
                TotalNoCheckIn = userPunishments.Where(x => x.Type == UserPunishmentType.NoCheckIn).Sum(x => x.TotalMoney),
                TotalNoCheckOut = userPunishments.Where(x => x.Type == UserPunishmentType.NoCheckOut).Sum(x => x.TotalMoney),
                TotalLateAndNoCheckOut = userPunishments.Where(x => x.Type == UserPunishmentType.LateAndNoCheckOut).Sum(x => x.TotalMoney),
                TotalNoCheckInAndNoCheckOut = userPunishments.Where(x => x.Type == UserPunishmentType.NoCheckInAndNoCheckOut).Sum(x => x.TotalMoney),
                TotalDaily = userPunishments.Where(x => x.Type == UserPunishmentType.Daily).Sum(x => x.TotalMoney),
                TotalMention = userPunishments.Where(x => x.Type == UserPunishmentType.Mention).Sum(x => x.TotalMoney),
                TotalTracker20k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_20k).Sum(x => x.TotalMoney),
                TotalTracker50k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_50k).Sum(x => x.TotalMoney),
                TotalTracker100k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_100k).Sum(x => x.TotalMoney),
                TotalTracker200k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_200k).Sum(x => x.TotalMoney),
                TotalAllDailyMoney = timekeepings.Sum(x => x.CountPunishDaily * 20000),
                TotalAllMentionMoney = timekeepings.Sum(x => x.CountPunishMention * 10000),
                TotalMonthlyPunishment = timekeepings.Sum(x => x.MoneyPunish)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimeSheet_ViewMyTardinessDetail)]
        public async Task<List<GetTimekeepingUserDto>> GetMyDetails(int year, int month)
        {
            var userId = AbpSession.UserId;
            var timekeepings = await WorkScope.GetAll<Timekeeping>()
              .Include(x => x.User)
              .Where(x => x.UserId == userId && x.DateAt.Year == year && x.DateAt.Month == month)
              .ToListAsync();
            var userPunishments = await WorkScope.GetAll<Timesheet.Entities.UserPunishment>()
              .Where(x => x.UserId == userId && x.DateAt.Year == year && x.DateAt.Month == month)
              .ToListAsync();
            var summary = CalculateUserPunishmentSummary(userPunishments, timekeepings);
            return (from t in timekeepings
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
                        Status = t.IsPunishedCheckIn ? PunishmentStatus.Punish : PunishmentStatus.Normal,
                        EditByUserId = t.LastModifierUserId,
                        UserNote = t.UserNote,
                        NoteReply = t.NoteReply,
                        StatusPunish = t.StatusPunish,
                        MoneyPunish = t.MoneyPunish,
                        TrackerTime = t.TrackerTime,
                        DailyPunish = t.CountPunishDaily * 20000,
                        MentionPunish = t.CountPunishMention * 10000,
                        TotalMonthlyPunishment = summary.TotalMonthlyPunishment,
                        TotalLatePunish = summary.TotalLate,
                        TotalNoCheckInPunish = summary.TotalNoCheckIn,
                        TotalNoCheckOutPunish = summary.TotalNoCheckOut,
                        TotalLateAndNoCheckOutPunish = summary.TotalLateAndNoCheckOut,
                        TotalNoCheckInAndNoCheckOutPunish = summary.TotalNoCheckInAndNoCheckOut,
                        TotalDailyPunish = summary.TotalDaily,
                        TotalMentionPunish = summary.TotalMention,
                        TotalTracker20kPunish = summary.TotalTracker20k,
                        TotalTracker50kPunish = summary.TotalTracker50k,
                        TotalTracker100kPunish = summary.TotalTracker100k,
                        TotalTracker200kPunish = summary.TotalTracker200k,
                        TotalAllDailyMoney = summary.TotalAllDailyMoney,
                        TotalAllMentionMoney = summary.TotalAllMentionMoney,
                    }).OrderByDescending(t => t.Date).ToList();
        }

        private async Task<int> GetMoneyPunishByType(CheckInCheckOutPunishmentType StatusPunish)
        {
            var checkInCheckOutPunishmentSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInCheckOutPunishmentSetting);
            var rs = JsonConvert.DeserializeObject<List<CheckInCheckOutPunishmentSettingDto>>(checkInCheckOutPunishmentSetting);
            var listPunish = rs.Select(item => new {
                Id = item.Id,
                Name = item.Name,
                Money = item.Money,
            }).ToList();
            var moneyForPunish = listPunish.Where(x => x.Id == StatusPunish).Select(x => x.Money).FirstOrDefault();
            return moneyForPunish;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_Edit)]
        [HttpPost]
        public async Task<Timekeeping> UpdateTimekeeping(Timekeeping input)
        {
            if (string.IsNullOrEmpty(input.RegisterCheckIn) ||
              string.IsNullOrEmpty(input.RegisterCheckOut))
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
            await timekeepingServices.CheckIsPunished(t);
            await timekeepingServices.CheckIsPunishedByRule(t, LimitedMinute, DateTimeUtils.ConvertHHmmssToMinutes(input.TrackerTime));
            await WorkScope.GetRepo<Timekeeping>().UpdateAsync(t);
            return t;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_UserNote)]
        [HttpPost]
        public async Task<Timekeeping> UserKhieuLai(TimekeepingUserNoteDto input)
        {
            if (input.PunishmentType == UserPunishmentType.NoPunish)
            {
                throw new UserFriendlyException("Vui lòng chọn loại phạt trước khi khiếu lại!");
            }
            var t = await WorkScope.GetAsync<Timekeeping>(input.Id);
            if (t != null && t.UserId != AbpSession.UserId.Value)
            {
                throw new UserFriendlyException("Bạn chỉ có thể khiếu lại cho bản ghi của mình");
            }
            if ((UserPunishmentType)t.StatusPunish != input.PunishmentType)
            {
                throw new UserFriendlyException("Bạn phải chọn đúng loại phạt mà bạn đang bị phạt!");
            }
            try
            {
                var punishmentTypeName = GetPunishmentTypeName(input.PunishmentType);
                var fullUserNote = $"[{punishmentTypeName}] {input.UserNote}";
                t.UserNote = fullUserNote;
                await WorkScope.GetRepo<Timekeeping>().UpdateAsync(t);
                await UpdateUserPunishmentNote(t.UserId.Value, t.DateAt, input.PunishmentType, fullUserNote, punishmentTypeName);
                return t;
            }
            catch (Exception ex)
            {
                Logger.Error("Error: " + ex.Message);
                throw new UserFriendlyException("An internal error occurred during your request!");
            }

        }

        private string GetPunishmentTypeName(UserPunishmentType punishmentType)
        {
            var punishmentTypeNames = new Dictionary<UserPunishmentType,
              string> {
          {
            UserPunishmentType.Late, "Late"
          },
          {
            UserPunishmentType.NoCheckIn,
            "No check-in"
          },
          {
            UserPunishmentType.NoCheckOut,
            "No check-out"
          },
          {
            UserPunishmentType.LateAndNoCheckOut,
            "Late & No check-out"
          },
          {
            UserPunishmentType.NoCheckInAndNoCheckOut,
            "No check-in & No check-out"
          },
          {
            UserPunishmentType.Daily,
            "Daily"
          },
          {
            UserPunishmentType.Mention,
            "Mention"
          },
          {
            UserPunishmentType.Tracker_20k,
            "Tracker 20k"
          },
          {
            UserPunishmentType.Tracker_50k,
            "Tracker 50k"
          },
          {
            UserPunishmentType.Tracker_100k,
            "Tracker 100k"
          },
          {
            UserPunishmentType.Tracker_200k,
            "Tracker 200k"
          }
        };
            return punishmentTypeNames.ContainsKey(punishmentType) ?
              punishmentTypeNames[punishmentType] :
              punishmentType.ToString();
        }

        private async Task UpdateUserPunishmentNote(long userId, DateTime dateAt, UserPunishmentType punishmentType, string userNote, string punishmentTypeName)
        {
            try
            {
                var userPunishment = await WorkScope.GetAll<Timesheet.Entities.UserPunishment>()
                  .FirstOrDefaultAsync(x => x.UserId == userId &&
                    x.DateAt.Date == dateAt.Date &&
                    x.Type == punishmentType);
                if (userPunishment != null)
                {
                    userPunishment.UserNote = userNote;
                    userPunishment.LastModificationTime = DateTime.Now;
                    userPunishment.LastModifierUserId = AbpSession.UserId;
                    await WorkScope.GetRepo<Timesheet.Entities.UserPunishment>().UpdateAsync(userPunishment);
                    Logger.Info($"Updated UserPunishment UserNote for UserId: {userId}, Date: {dateAt:yyyy-MM-dd}, Type: {punishmentType}");
                }
                else
                {
                    Logger.Warn($"UserPunishment not found for UserId: {userId}, Date: {dateAt:yyyy-MM-dd}, Type: {punishmentType}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating UserPunishment UserNote: {ex.Message}");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_UserNote)]
        [HttpPost]
        public async Task<Timekeeping> TraLoiKhieuLai(TimekeepingDto input)
        {
            var t = await WorkScope.GetAsync<Timekeeping>(input.Id);
            if (input.StatusPunish == CheckInCheckOutPunishmentType.NoPunish)
            {
                throw new UserFriendlyException("Vui lòng chọn loại phạt trước khi trả lời khiếu nại!");
            }
            if (t.StatusPunish != input.StatusPunish)
            {
                throw new UserFriendlyException("Bạn phải chọn đúng loại phạt mà bản ghi đang bị phạt!");
            }
            t.NoteReply = input.NoteReply;
            if (t.StatusPunish == CheckInCheckOutPunishmentType.NoPunish)
            {
                t.IsPunishedCheckIn = false;
            }
            if (new List<CheckInCheckOutPunishmentType> {
          CheckInCheckOutPunishmentType.Late,
          CheckInCheckOutPunishmentType.NoCheckIn,
          CheckInCheckOutPunishmentType.NoCheckOut,
          CheckInCheckOutPunishmentType.LateAndNoCheckOut,
          CheckInCheckOutPunishmentType.NoCheckInAndNoCheckOut
        }.Contains(t.StatusPunish))
            {
                t.IsPunishedCheckIn = true;
            }
            t.MoneyPunish = await GetMoneyPunishByType(t.StatusPunish);
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