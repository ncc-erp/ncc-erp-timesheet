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
using System.Web.Http.Results;
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
        public async Task<List<UserPunishmentDetailDto>> GetDetailTimekeeping(
          int year, int month, int? day,
          long? userId, long? branchId,
          bool? isPunished, bool? isComplain,
          UserPunishmentType? statusPunish = null)
        {
            if (statusPunish == UserPunishmentType.NoPunish || (statusPunish == null && (isPunished == false)))
            {
                var query = from t in WorkScope.GetAll<Timekeeping>()
                            join u in WorkScope.GetAll<User>() on t.UserId equals u.Id
                            where t.DateAt.Year == year &&
                              t.DateAt.Month == month &&
                              t.StatusPunish == CheckInCheckOutPunishmentType.NoPunish &&
                              t.CountPunishDaily == 0 &&
                              t.CountPunishMention == 0 &&
                              (!userId.HasValue || t.UserId == userId) &&
                              (!day.HasValue || day < 0 || t.DateAt.Day == day) &&
                              (!branchId.HasValue || u.BranchId == branchId) &&
                              (!isPunished.HasValue || t.IsPunishedCheckIn == isPunished) &&
                              (!isComplain.HasValue || string.IsNullOrEmpty(t.UserNote) != isComplain)
                            join tu in WorkScope.GetAll<User>() on t.LastModifierUserId equals tu.Id into tuGroup
                            from tu in tuGroup.DefaultIfEmpty()
                            select new UserPunishmentDetailDto
                            {
                                UserId = t.UserId,
                                UserName = u.FullName,
                                UserType = u.Type,
                                UserEmail = u.EmailAddress,
                                Branch = u.BranchOld,
                                BranchColor = u.Branch.Color,
                                BranchDisplayName = u.Branch.DisplayName,
                                BranchId = u.Branch.Id,
                                AvatarPath = u.AvatarPath,
                                Date = t.DateAt,
                                TimekeepingId = t.Id,
                                UserpunishmentId = 0,
                                RegistrationTimeStart = t.RegisterCheckIn,
                                RegistrationTimeEnd = t.RegisterCheckOut,
                                CheckIn = t.CheckIn,
                                CheckOut = t.CheckOut,
                                ResultCheckIn = CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn),
                                ResultCheckOut = CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut),
                                EditByUserName = tu != null ? tu.UserName : string.Empty,
                                EditByUserId = t.LastModifierUserId,
                                UserNote = t.UserNote,
                                NoteReply = t.NoteReply,
                                TrackerTime = t.TrackerTime,
                                StatusPunish = UserPunishmentType.NoPunish,
                                MoneyPunish = 0,
                                Count = 0,
                                IsPunished = false,
                                PunishmentSystemId = null
                            };

                var noPunishResult = await query.ToListAsync();

                return isComplain == true ?
                  noPunishResult.OrderByDescending(d => d.IsPunished)
                  .ThenByDescending(d => d.ResultCheckIn)
                  .ThenByDescending(d => d.Date)
                  .ToList() :
                  noPunishResult.OrderByDescending(d => d.Date)
                  .ThenByDescending(d => d.IsPunished)
                  .ThenByDescending(d => d.ResultCheckIn)
                  .ToList();
            }

            var upList = await WorkScope.GetAll<UserPunishment>()
              .Include(up => up.User)
              .ThenInclude(u => u.Branch)
              .Where(up => up.DateAt.Year == year && up.DateAt.Month == month)
              .ToListAsync();

            var tkList = await WorkScope.GetAll<Timekeeping>()
              .Where(t => t.DateAt.Year == year && t.DateAt.Month == month)
              .ToListAsync();

            var userList = await WorkScope.GetAll<User>()
              .Select(u => new {
                  u.Id,
                  u.UserName
              })
              .ToListAsync();

            var joined = from up in upList
                         join t in tkList
                         on (up.UserId, up.DateAt) equals (t.UserId, t.DateAt)
                         into gj
                         from t in gj.DefaultIfEmpty()

                         let lastModId = t != null ? (long?)t.LastModifierUserId : null

                         join tu in userList
                         on lastModId equals tu.Id
                         into mgj
                         from tu in mgj.DefaultIfEmpty()

                         where (!userId.HasValue || up.UserId == userId) &&
                           (!day.HasValue || day < 0 || up.DateAt.Day == day) &&
                           (!branchId.HasValue || up.User.BranchId == branchId) &&
                           (!isPunished.HasValue || (up.Type != UserPunishmentType.NoPunish) == isPunished.Value) &&
                           (!isComplain.HasValue || (string.IsNullOrEmpty(up.UserNote) != isComplain.Value)) &&
                           (!statusPunish.HasValue || up.Type == statusPunish.Value)
                         select new UserPunishmentDetailDto
                         {
                             TimekeepingId = t?.Id,
                             UserpunishmentId = up.Id,
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
                             RegistrationTimeStart = t?.RegisterCheckIn,
                             RegistrationTimeEnd = t?.RegisterCheckOut,
                             TrackerTime = t?.TrackerTime,
                             ResultCheckIn = t != null ? (double?)CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn) : null,
                             ResultCheckOut = t != null ? (double?)CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut) : null,
                             AvatarPath = up.User.AvatarPath,
                             BranchDisplayName = up.User.Branch?.DisplayName,
                             BranchColor = up.User.Branch?.Color,
                             BranchId = up.User.BranchId,
                             IsPunished = up.Type != UserPunishmentType.NoPunish,
                             StatusPunish = up.Type,
                             EditByUserId = t?.LastModifierUserId,
                             EditByUserName = tu?.UserName
                         };

            var finalResult = joined.ToList();

            if (statusPunish == null && (isPunished == null || isPunished == false))
            {
                var noPunishQuery = from t in WorkScope.GetAll<Timekeeping>()
                                    join u in WorkScope.GetAll<User>() on t.UserId equals u.Id
                                    where t.DateAt.Year == year &&
                                      t.DateAt.Month == month &&
                                      t.StatusPunish == CheckInCheckOutPunishmentType.NoPunish &&
                                      t.CountPunishDaily == 0 &&
                                      t.CountPunishMention == 0 &&
                                      (!userId.HasValue || t.UserId == userId) &&
                                      (!day.HasValue || day < 0 || t.DateAt.Day == day) &&
                                      (!branchId.HasValue || u.BranchId == branchId) &&
                                      (!isPunished.HasValue || t.IsPunishedCheckIn == isPunished) &&
                                      (!isComplain.HasValue || string.IsNullOrEmpty(t.UserNote) != isComplain)
                                    join tu in WorkScope.GetAll<User>() on t.LastModifierUserId equals tu.Id into tuGroup
                                    from tu in tuGroup.DefaultIfEmpty()
                                    select new UserPunishmentDetailDto
                                    {
                                        UserId = t.UserId,
                                        UserName = u.FullName,
                                        UserType = u.Type,
                                        UserEmail = u.EmailAddress,
                                        Branch = u.BranchOld,
                                        BranchColor = u.Branch.Color,
                                        BranchDisplayName = u.Branch.DisplayName,
                                        BranchId = u.Branch.Id,
                                        AvatarPath = u.AvatarPath,
                                        Date = t.DateAt,
                                        TimekeepingId = t.Id,
                                        UserpunishmentId = 0,
                                        RegistrationTimeStart = t.RegisterCheckIn,
                                        RegistrationTimeEnd = t.RegisterCheckOut,
                                        CheckIn = t.CheckIn,
                                        CheckOut = t.CheckOut,
                                        ResultCheckIn = CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn),
                                        ResultCheckOut = CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut),
                                        EditByUserName = tu != null ? tu.UserName : string.Empty,
                                        EditByUserId = t.LastModifierUserId,
                                        UserNote = t.UserNote,
                                        NoteReply = t.NoteReply,
                                        TrackerTime = t.TrackerTime,
                                        StatusPunish = UserPunishmentType.NoPunish,
                                        MoneyPunish = 0,
                                        Count = 0,
                                        IsPunished = false,
                                        PunishmentSystemId = null
                                    };

                var noPunishResult = await noPunishQuery.ToListAsync();
                finalResult = finalResult.Union(noPunishResult).ToList();
            }

            return isComplain == true ?
              finalResult.OrderByDescending(d => d.IsPunished)
              .ThenByDescending(d => d.ResultCheckIn)
              .ThenByDescending(d => d.Date)
              .ToList() :
              finalResult.OrderByDescending(d => d.Date)
              .ThenByDescending(d => d.IsPunished)
              .ThenByDescending(d => d.ResultCheckIn)
              .ToList();
        }


        private UserPunishmentSummaryDto CalculateUserPunishmentSummary(
            List<UserPunishment> userPunishments,
            List<Timekeeping> timekeepings)
        {
            return new UserPunishmentSummaryDto
          {
        //        TotalLate = userPunishments.Where(x => x.Type == UserPunishmentType.Late).Sum(x => x.TotalMoney),
        //        TotalNoCheckIn = userPunishments.Where(x => x.Type == UserPunishmentType.NoCheckIn).Sum(x => x.TotalMoney),
        //        TotalNoCheckOut = userPunishments.Where(x => x.Type == UserPunishmentType.NoCheckOut).Sum(x => x.TotalMoney),
        //        TotalLateAndNoCheckOut = userPunishments.Where(x => x.Type == UserPunishmentType.LateAndNoCheckOut).Sum(x => x.TotalMoney),
        //        TotalNoCheckInAndNoCheckOut = userPunishments.Where(x => x.Type == UserPunishmentType.NoCheckInAndNoCheckOut).Sum(x => x.TotalMoney),
        //        TotalDaily = userPunishments.Where(x => x.Type == UserPunishmentType.Daily).Sum(x => x.TotalMoney),
        //        TotalMention = userPunishments.Where(x => x.Type == UserPunishmentType.Mention).Sum(x => x.TotalMoney),
        //        TotalTracker20k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_20k).Sum(x => x.TotalMoney),
        //        TotalTracker50k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_50k).Sum(x => x.TotalMoney),
        //        TotalTracker100k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_100k).Sum(x => x.TotalMoney),
        //        TotalTracker200k = userPunishments.Where(x => x.Type == UserPunishmentType.Tracker_200k).Sum(x => x.TotalMoney),
        //        TotalAllDailyMoney = timekeepings.Sum(x => x.CountPunishDaily * 20000),
        //        TotalAllMentionMoney = timekeepings.Sum(x => x.CountPunishMention * 10000),
             TotalMonthlyPunishment = timekeepings.Sum(x => x.MoneyPunish)
            };
        }

        //        private UserPunishmentDailyDto CalculateUserPunishmentDaily(
        //List<UserPunishment> userPunishments,
        //DateTime date)
        //        {
        //            var dailyPunishments = userPunishments.Where(x => x.DateAt.Date == date.Date).ToList();

        //            return new UserPunishmentDailyDto
        //            {
        //                DailyLate = dailyPunishments.Where(x => x.Type == UserPunishmentType.Late).Sum(x => x.TotalMoney),
        //                DailyNoCheckIn = dailyPunishments.Where(x => x.Type == UserPunishmentType.NoCheckIn).Sum(x => x.TotalMoney),
        //                DailyNoCheckOut = dailyPunishments.Where(x => x.Type == UserPunishmentType.NoCheckOut).Sum(x => x.TotalMoney),
        //                DailyLateAndNoCheckOut = dailyPunishments.Where(x => x.Type == UserPunishmentType.LateAndNoCheckOut).Sum(x => x.TotalMoney),
        //                DailyNoCheckInAndNoCheckOut = dailyPunishments.Where(x => x.Type == UserPunishmentType.NoCheckInAndNoCheckOut).Sum(x => x.TotalMoney),
        //                DailyDaily = dailyPunishments.Where(x => x.Type == UserPunishmentType.Daily).Sum(x => x.TotalMoney),
        //                DailyMention = dailyPunishments.Where(x => x.Type == UserPunishmentType.Mention).Sum(x => x.TotalMoney),
        //                DailyTracker20k = dailyPunishments.Where(x => x.Type == UserPunishmentType.Tracker_20k).Sum(x => x.TotalMoney),
        //                DailyTracker50k = dailyPunishments.Where(x => x.Type == UserPunishmentType.Tracker_50k).Sum(x => x.TotalMoney),
        //                DailyTracker100k = dailyPunishments.Where(x => x.Type == UserPunishmentType.Tracker_100k).Sum(x => x.TotalMoney),
        //                DailyTracker200k = dailyPunishments.Where(x => x.Type == UserPunishmentType.Tracker_200k).Sum(x => x.TotalMoney),
        //                TotalDayPunishment = dailyPunishments.Sum(x => x.TotalMoney),
        //                TotalDayPunishmentTotal = dailyPunishments.GroupBy(x => x.DateAt.Date)
        //                                         .Select(g => g.Sum(x => x.TotalMoney))
        //                                         .Sum()
        //            };
        //        }

        //[AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimeSheet_ViewMyTardinessDetail)]
        //public async Task<List<GetTimekeepingUserDto>> GetMyDetails(int year, int month)
        //{
        //    var userId = AbpSession.UserId;
        //    var timekeepings = await WorkScope.GetAll<Timekeeping>()
        //      .Include(x => x.User)
        //      .Where(x => x.UserId == userId && x.DateAt.Year == year && x.DateAt.Month == month)
        //      .ToListAsync();

        //    var userPunishments = await WorkScope.GetAll<Timesheet.Entities.UserPunishment>()
        //      .Where(x => x.UserId == userId && x.DateAt.Year == year && x.DateAt.Month == month)
        //      .ToListAsync();

        //    var summary = CalculateUserPunishmentSummary(userPunishments, timekeepings);
        //    var totalMonthPunishment = userPunishments.Sum(x => x.TotalMoney);

        //    // Group UserPunishments by DateAt để dễ dàng truy xuất theo ngày
        //    var userPunishmentsByDate = userPunishments
        //        .GroupBy(x => x.DateAt.Date)
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    return (from t in timekeepings
        //            let dailySummary = CalculateUserPunishmentDaily(userPunishments, t.DateAt)
        //            let datePunishments = userPunishmentsByDate.ContainsKey(t.DateAt.Date)
        //                ? userPunishmentsByDate[t.DateAt.Date]
        //                : new List<UserPunishment>()
        //            let hasUserPunishmentData = datePunishments.Any()
        //            select new GetTimekeepingUserDto
        //            {
        //                UserId = t.UserId,
        //                UserName = t.User.FullName,
        //                UserType = t.User.Type,
        //                UserEmail = t.UserEmail,
        //                Branch = t.User.BranchOld,
        //                AvatarPath = t.User.AvatarPath,
        //                Date = t.DateAt,
        //                TimekeepingId = t.Id,
        //                UserPunishmentId = datePunishments.FirstOrDefault()?.Id ?? 0,

        //                // Luôn lấy từ Timekeeping
        //                RegistrationTimeStart = t.RegisterCheckIn,
        //                RegistrationTimeEnd = t.RegisterCheckOut,
        //                CheckIn = t.CheckIn,
        //                CheckOut = t.CheckOut,
        //                ResultCheckIn = CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn),
        //                ResultCheckOut = CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut),
        //                TrackerTime = t.TrackerTime,

        //                // Các thông tin khác từ Timekeeping
        //                Status = t.IsPunishedCheckIn ? PunishmentStatus.Punish : PunishmentStatus.Normal,
        //                EditByUserId = t.LastModifierUserId,
        //                // Combine tất cả UserNote từ các punishments
        //                UserNote = hasUserPunishmentData ? string.Join(" | ", datePunishments.Select(x => x.UserNote).Where(x => !string.IsNullOrEmpty(x))) : t.UserNote,
        //                NoteReply = hasUserPunishmentData ? string.Join(" | ", datePunishments.Select(x => x.NoteReply).Where(x => !string.IsNullOrEmpty(x))) : t.NoteReply,
        //                StatusPunish = t.StatusPunish,

        //                // Ưu tiên UserPunishment, fallback về Timekeeping - lấy type có priority cao nhất
        //                UserPunishmentType = hasUserPunishmentData ? datePunishments.OrderByDescending(x => (int)x.Type).FirstOrDefault()?.Type ?? default(UserPunishmentType) : default(UserPunishmentType),

        //                // Tiền phạt: Tổng tất cả UserPunishment trong ngày
        //                MoneyPunish = hasUserPunishmentData ? datePunishments.Sum(x => x.TotalMoney) : t.MoneyPunish,

        //                // DailyPunish và MentionPunish: Tổng từ tất cả UserPunishment theo type
        //                DailyPunish = hasUserPunishmentData ?
        //                    datePunishments.Where(x => x.Type == UserPunishmentType.Daily).Sum(x => x.TotalMoney) :
        //                    t.CountPunishDaily * 20000,

        //                MentionPunish = hasUserPunishmentData ?
        //                    datePunishments.Where(x => x.Type == UserPunishmentType.Mention).Sum(x => x.TotalMoney) :
        //                    t.CountPunishMention * 10000,

        //                // Populate UserPunishments list
        //                UserPunishments = datePunishments.Select(up => new UserPunishmentDetailDto
        //                {
        //                    UserpunishmentId = up.Id,
        //                    StatusPunish = up.Type,
        //                    MoneyPunish = up.TotalMoney,
        //                    UserNote = up.UserNote,
        //                    NoteReply = up.NoteReply,
        //                    UserId = up.UserId,
        //                    Count = up.Count,
        //                    Date = up.DateAt,

        //                    // Thêm thông tin từ Timekeeping cho UserPunishmentDetailDto
        //                    TimekeepingId = t.Id,
        //                    CheckIn = t.CheckIn,
        //                    CheckOut = t.CheckOut,
        //                    RegistrationTimeStart = t.RegisterCheckIn,
        //                    RegistrationTimeEnd = t.RegisterCheckOut,
        //                    TrackerTime = t.TrackerTime,
        //                    ResultCheckIn = CommonUtils.SubtractHHmm(t.CheckIn, t.RegisterCheckIn),
        //                    ResultCheckOut = CommonUtils.SubtractHHmm(t.RegisterCheckOut, t.CheckOut),

        //                    // Tính toán DailyPunish và MentionPunish cho từng UserPunishment
        //                    DailyPunish = up.Type == UserPunishmentType.Daily ? up.TotalMoney : 0,
        //                    MentionPunish = up.Type == UserPunishmentType.Mention ? up.TotalMoney : 0,

        //                    // Thông tin User từ Timekeeping
        //                    UserName = t.User.FullName,
        //                    UserEmail = t.UserEmail,
        //                    UserType = t.User.Type,
        //                    AvatarPath = t.User.AvatarPath,
        //                    Branch = t.User.BranchOld,
        //                }).ToList(),

        //                // Daily summary - vẫn giữ nguyên
        //                DailyLatePunish = dailySummary.DailyLate,
        //                DailyNoCheckInPunish = dailySummary.DailyNoCheckIn,
        //                DailyNoCheckOutPunish = dailySummary.DailyNoCheckOut,
        //                DailyLateAndNoCheckOutPunish = dailySummary.DailyLateAndNoCheckOut,
        //                DailyNoCheckInAndNoCheckOutPunish = dailySummary.DailyNoCheckInAndNoCheckOut,
        //                DailyDailyPunish = dailySummary.DailyDaily,
        //                DailyMentionPunish = dailySummary.DailyMention,
        //                DailyTracker20kPunish = dailySummary.DailyTracker20k,
        //                DailyTracker50kPunish = dailySummary.DailyTracker50k,
        //                DailyTracker100kPunish = dailySummary.DailyTracker100k,
        //                DailyTracker200kPunish = dailySummary.DailyTracker200k,
        //                TotalDayPunishment = dailySummary.TotalDayPunishment,
        //                TotalDayPunishmentTotal = totalMonthPunishment
        //            }).OrderByDescending(t => t.Date).ToList();
        //}
        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimeSheet_ViewMyTardinessDetail)]
        public async Task<List<GetTimekeepingUserDto>> GetMyDetails(int year, int month)
        {
            // Lấy dữ liệu từ cả hai bảng
            var tkList = await WorkScope.GetAll<Timekeeping>()
              .Include(t => t.User).ThenInclude(u => u.Branch)
              .Where(t =>
                t.DateAt.Year == year &&
                t.DateAt.Month == month &&
                t.UserId == AbpSession.UserId)
              .ToListAsync();

            var upList = await WorkScope.GetAll<UserPunishment>()
              .Include(up => up.User).ThenInclude(u => u.Branch)
              .Where(up =>
                up.DateAt.Year == year &&
                up.DateAt.Month == month &&
                up.UserId == AbpSession.UserId)
              .ToListAsync();

            var userIds = tkList.Select(t => t.LastModifierUserId)
              .Union(upList.Select(up => up.CreatorUserId))
              .Where(id => id.HasValue)
              .Select(id => id.Value)
              .Distinct()
              .ToList();

            var users = await WorkScope.GetAll<User>()
              .Where(u => userIds.Contains(u.Id))
              .Select(u => new {
                  u.Id,
                  u.UserName
              })
              .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var result = new List<GetTimekeepingUserDto>();

            // Tạo lookup từ UserPunishment để dễ dàng tìm kiếm
            var upLookup = upList.ToLookup(up => new {
                Date = up.DateAt.Date,
                UserId = (long)up.UserId
            });

            // Xử lý từng record trong Timekeeping
            foreach (var tk in tkList)
            {
                // Chỉ xử lý các record có UserId
                if (!tk.UserId.HasValue)
                    continue;

                var matchingUps = upLookup[new
                {
                    Date = tk.DateAt.Date,
                    UserId = tk.UserId.Value
                }].ToList();

                // Tính tổng tiền phạt trong ngày và tổng tiền phạt trong tháng
                var (totalDayPunishment, totalMonthPunishmentTotal) = CalculatePunishmentTotals(upList, tk.DateAt, tk.UserId.Value);

                if (matchingUps.Any())
                {
                    // Có record tương ứng trong UserPunishment
                    foreach (var up in matchingUps)
                    {
                        result.Add(new GetTimekeepingUserDto
                        {
                            TimekeepingId = tk.Id,
                            UserPunishmentId = up.Id,
                            UserId = tk.UserId,
                            UserName = tk.User.FullName,
                            UserType = tk.User.Type,
                            UserEmail = tk.UserEmail,
                            Branch = tk.User.BranchOld,
                            AvatarPath = tk.User.AvatarPath,
                            Date = tk.DateAt,
                            RegistrationTimeStart = tk.RegisterCheckIn,
                            RegistrationTimeEnd = tk.RegisterCheckOut,
                            CheckIn = tk.CheckIn,
                            CheckOut = tk.CheckOut,
                            ResultCheckIn = CommonUtils.SubtractHHmm(tk.CheckIn, tk.RegisterCheckIn),
                            ResultCheckOut = CommonUtils.SubtractHHmm(tk.RegisterCheckOut, tk.CheckOut),
                            EditByUserName = tk.LastModifierUserId.HasValue ? users.ContainsKey(tk.LastModifierUserId.Value) ? users[tk.LastModifierUserId.Value] : "" : "",
                            Status = tk.IsPunishedCheckIn ? PunishmentStatus.Punish : PunishmentStatus.Normal,
                            EditByUserId = tk.LastModifierUserId,
                            UserNote = up.UserNote ?? tk.UserNote,
                            NoteReply = up.NoteReply ?? tk.NoteReply,

                            // Punishment info
                            UserPunishmentType = up.Type,
                            MoneyPunish = up.TotalMoney,
                            TrackerTime = tk.TrackerTime,
                            DailyPunish = tk.CountPunishDaily,
                            MentionPunish = tk.CountPunishMention,

                            // Branch info
                            BranchColor = tk.User.Branch?.Color,
                            BranchDisplayName = tk.User.Branch?.DisplayName,
                            BranchId = tk.User.BranchId,

                            StatusPunish = tk.StatusPunish,

                            // Tổng tiền phạt
                            TotalDayPunishment = totalDayPunishment,
                            TotalMonthPunishmentTotal = totalMonthPunishmentTotal
                        });
                    }
                }
                else
                {
                    // Không có record tương ứng trong UserPunishment
                    result.Add(new GetTimekeepingUserDto
                    {
                        TimekeepingId = tk.Id,
                        UserId = tk.UserId,
                        UserName = tk.User.FullName,
                        UserType = tk.User.Type,
                        UserEmail = tk.UserEmail,
                        Branch = tk.User.BranchOld,
                        AvatarPath = tk.User.AvatarPath,
                        Date = tk.DateAt,
                        RegistrationTimeStart = tk.RegisterCheckIn,
                        RegistrationTimeEnd = tk.RegisterCheckOut,
                        CheckIn = tk.CheckIn,
                        CheckOut = tk.CheckOut,
                        ResultCheckIn = CommonUtils.SubtractHHmm(tk.CheckIn, tk.RegisterCheckIn),
                        ResultCheckOut = CommonUtils.SubtractHHmm(tk.RegisterCheckOut, tk.CheckOut),
                        EditByUserName = tk.LastModifierUserId.HasValue ? users.ContainsKey(tk.LastModifierUserId.Value) ? users[tk.LastModifierUserId.Value] : "" : "",
                        Status = tk.IsPunishedCheckIn ? PunishmentStatus.Punish : PunishmentStatus.Normal,
                        EditByUserId = tk.LastModifierUserId,
                        UserNote = tk.UserNote,
                        NoteReply = tk.NoteReply,

                        // Punishment info
                        MoneyPunish = tk.MoneyPunish,
                        TrackerTime = tk.TrackerTime,
                        DailyPunish = tk.CountPunishDaily,
                        MentionPunish = tk.CountPunishMention,

                        // Branch info
                        BranchColor = tk.User.Branch?.Color,
                        BranchDisplayName = tk.User.Branch?.DisplayName,
                        BranchId = tk.User.BranchId,

                        StatusPunish = tk.StatusPunish,

                        // Tổng tiền phạt
                        TotalDayPunishment = totalDayPunishment,
                        TotalMonthPunishmentTotal = totalMonthPunishmentTotal
                    });
                }
            }

            // Kiểm tra các record trong UserPunishment mà không có trong Timekeeping
            var tkLookup = tkList.Where(tk => tk.UserId.HasValue)
              .ToLookup(tk => new {
                  Date = tk.DateAt.Date,
                  UserId = tk.UserId.Value
              });

            foreach (var up in upList)
            {
                var key = new
                {
                    Date = up.DateAt.Date,
                    UserId = up.UserId
                };
                if (!tkLookup.Contains(key))
                {
                    // Tính tổng tiền phạt trong ngày và tổng tiền phạt trong tháng
                    var (totalDayPunishment, totalMonthPunishmentTotal) = CalculatePunishmentTotals(upList, up.DateAt, up.UserId);

                    // Record chỉ có trong UserPunishment
                    result.Add(new GetTimekeepingUserDto
                    {
                        TimekeepingId = 0,
                        UserPunishmentId = up.Id,
                        UserId = up.UserId,
                        UserName = up.User.FullName,
                        UserType = up.User.Type,
                        UserEmail = up.User.EmailAddress,
                        Branch = up.User.BranchOld,
                        AvatarPath = up.User.AvatarPath,
                        Date = up.DateAt,
                        RegistrationTimeStart = null,
                        RegistrationTimeEnd = null,
                        CheckIn = null,
                        CheckOut = null,
                        ResultCheckIn = null,
                        ResultCheckOut = null,
                        EditByUserName = up.LastModifierUserId.HasValue ? users.ContainsKey(up.LastModifierUserId.Value) ? users[up.LastModifierUserId.Value] : "" : "",
                        Status = PunishmentStatus.Punish,
                        EditByUserId = up.LastModifierUserId,
                        UserNote = up.UserNote,
                        NoteReply = up.NoteReply,

                        // Punishment info
                        UserPunishmentType = up.Type,
                        MoneyPunish = up.TotalMoney,

                        // Branch info
                        BranchColor = up.User.Branch?.Color,
                        BranchDisplayName = up.User.Branch?.DisplayName,
                        BranchId = up.User.BranchId,

                        StatusPunish = CheckInCheckOutPunishmentType.NoPunish,

                        // Tổng tiền phạt
                        TotalDayPunishment = totalDayPunishment,
                        TotalMonthPunishmentTotal = totalMonthPunishmentTotal
                    });
                }
            }

            return result.OrderByDescending(t => t.Date).ToList();
        }

        private (int totalDayPunishment, decimal totalMonthPunishmentTotal) CalculatePunishmentTotals(List<UserPunishment> userPunishments, DateTime date, long userId)
        {
            // Tính tổng tiền phạt trong ngày (tổng các loại phạt như ant, daily, late trong một ngày)
            int totalDayPunishment = userPunishments
              .Where(up => up.DateAt.Date == date.Date && up.UserId == userId)
              .Sum(up => up.TotalMoney);

            // Tính tổng tiền phạt trong tháng
            decimal totalMonthPunishmentTotal = userPunishments
              .Where(up => up.DateAt.Year == date.Year && up.DateAt.Month == date.Month && up.UserId == userId)
              .Sum(up => up.TotalMoney);

            return (totalDayPunishment, totalMonthPunishmentTotal);
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
        public async Task<UserComplaintResultDto> UserKhieuLai(SubmitUserComplaintDto input)
        {
            try
            {
                // Lấy thông tin UserPunishment dựa trên ID
                var userPunishment = await WorkScope.GetAll<UserPunishment>()
                    .Where(x => x.Id == input.UserPunishmentId)
                    .FirstOrDefaultAsync();

                if (userPunishment == null)
                {
                    throw new UserFriendlyException("Không tìm thấy bản ghi phạt tương ứng!");
                }

                // Kiểm tra xem người dùng có quyền khiếu nại bản ghi này không
                if (userPunishment.UserId != AbpSession.UserId.Value)
                {
                    throw new UserFriendlyException("Bạn chỉ có thể khiếu nại cho bản ghi của mình");
                }

                // Cập nhật ghi chú khiếu nại
                userPunishment.UserNote = input.UserNote;
                await WorkScope.UpdateAsync(userPunishment);

                // Tìm bản ghi Timekeeping tương ứng (nếu có)
                var timekeeping = await WorkScope.GetAll<Timekeeping>()
                    .Where(t => t.UserId == userPunishment.UserId &&
                           t.DateAt.Date == userPunishment.DateAt.Date)
                    .FirstOrDefaultAsync();

                // Cập nhật ghi chú trong Timekeeping nếu có
                if (timekeeping != null)
                {
                    timekeeping.UserNote = input.UserNote;
                    await WorkScope.UpdateAsync(timekeeping);
                }

                // Lấy tên loại phạt
                var punishmentTypeName = GetPunishmentTypeName(userPunishment.Type);

                await CurrentUnitOfWork.SaveChangesAsync();

                // Trả về kết quả
                return new UserComplaintResultDto
                {
                    UserPunishmentId = userPunishment.Id,
                    TimekeepingId = timekeeping?.Id,
                    UserNote = input.UserNote,
                    PunishmentType = userPunishment.Type,
                    PunishmentTypeName = punishmentTypeName,
                    Success = true,
                    Message = "Đã cập nhật khiếu nại thành công"
                };
            }
            catch (UserFriendlyException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in UserKhieuLai: " + ex.Message, ex);
                throw new UserFriendlyException("Đã xảy ra lỗi khi xử lý khiếu nại!");
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