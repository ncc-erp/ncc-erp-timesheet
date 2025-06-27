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
        public async Task<List<UserPunishmentDetailDto>> GetDetailTimekeeping(
          int year, int month, int? day,
          long? userId, long? branchId,
          bool? isPunished, bool? isComplain,
          UserPunishmentType? statusPunish = null)
        {
            var upList = await WorkScope.GetAll<UserPunishment>()
              .Include(up => up.User)
              .ThenInclude(u => u.Branch)
              .Where(up => up.DateAt.Year == year && up.DateAt.Month == month)
              .ToListAsync();

            var tkList = await WorkScope.GetAll<Timekeeping>()
              .Where(t => t.DateAt.Year == year && t.DateAt.Month == month)
              .ToListAsync();

            var joined = from up in upList
                         join t in tkList
                         on (up.UserId, up.DateAt) equals (t.UserId, t.DateAt)
                         into gj
                         from t in gj.DefaultIfEmpty()
                         where (!userId.HasValue || up.UserId == userId) &&
                           (!day.HasValue || day.Value < 0 || up.DateAt.Day == day.Value) &&
                           (!branchId.HasValue || up.User.BranchId == branchId) &&
                           (!isPunished.HasValue || (up.Type != UserPunishmentType.NoPunish) == isPunished.Value) &&
                           (!isComplain.HasValue || (string.IsNullOrEmpty(up.UserNote) != isComplain.Value)) &&
                           (!statusPunish.HasValue || up.Type == statusPunish.Value)
                         select new UserPunishmentDetailDto
                         {
                             userpunishmentId = up.Id,
                             UserId = up.UserId,
                             UserName = up.User.FullName,
                             UserType = up.User.Type,
                             UserEmail = up.User.EmailAddress,
                             Date = up.DateAt,
                             Count = up.Count,
                             MoneyPunish = up.TotalMoney,
                             UserNote = up.UserNote,
                             NoteReply = up.NoteReply,
                             PunishmentSystemId = up.PunishmentSystemId,
                             CheckIn = t?.CheckIn,
                             CheckOut = t?.CheckOut,
                             RegisterCheckIn = t?.RegisterCheckIn,
                             RegisterCheckOut = t?.RegisterCheckOut,
                             TrackerTime = t?.TrackerTime,
                             ResultCheckIn = t != null ?
                             (double?)CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn) :
                             null,
                             ResultCheckOut = t != null ?
                             (double?)CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut) :
                             null,
                             AvatarPath = up.User.AvatarPath,
                             BranchName = up.User.Branch?.DisplayName,
                             BranchColor = up.User.Branch?.Color,
                             BranchId = up.User.BranchId,
                             IsPunished = up.Type != UserPunishmentType.NoPunish,
                             StatusPunish = up.Type,
                             EditByUserId = up.LastModifierUserId
                         };

            joined = (isComplain == true) ?
              joined.OrderByDescending(d => d.IsPunished)
              .ThenByDescending(d => d.ResultCheckIn)
              .ThenByDescending(d => d.Date) :
              joined.OrderByDescending(d => d.Date)
              .ThenByDescending(d => d.IsPunished)
              .ThenByDescending(d => d.ResultCheckIn);

            return joined.ToList();
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

        private async Task<int> GetMoneyPunishByType(CheckInCheckOutPunishmentType StatusPunish)
        {
            var checkInCheckOutPunishmentSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInCheckOutPunishmentSetting);
            var rs = JsonConvert.DeserializeObject<List<CheckInCheckOutPunishmentSettingDto>>(checkInCheckOutPunishmentSetting);
            var listPunish = rs.Select(item => new
            {
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
            await timekeepingServices.CheckIsPunished(t);
            await timekeepingServices.CheckIsPunishedByRule(t, LimitedMinute, DateTimeUtils.ConvertHHmmssToMinutes(input.TrackerTime));
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

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_UserNote)]
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
