using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities.Enum;
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
using System.Transactions;
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
        public TimekeepingAppService(TimekeepingServices timekeepingServices, IWorkScope workScope) : base(workScope)
        {
            this.timekeepingServices = timekeepingServices;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_View)]
        [HttpPost]
        public async Task<TardinessResultDto> GetAllPagging(GridParam input, int year, int month, long? branchId, long? userId)
        {
            var m = Int64.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));
            var viewAll = PermissionChecker.IsGranted(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_View);

            var punishmentAmounts = WorkScope.GetAll<UserPunishment>()
                .Where(p => p.DateAt.Year == year && p.DateAt.Month == month &&
                       (!userId.HasValue || p.UserId == userId.Value) &&
                       (!branchId.HasValue || p.User.BranchId == branchId.Value) &&
                       p.User.IsActive &&
                       (viewAll || p.UserId == AbpSession.UserId.Value))
                .GroupBy(p => p.UserId)
                .Select(g => new {
                    UserId = g.Key,
                    TotalPunishmentAmount = g.Sum(p => p.TotalMoney)
                })
                .ToDictionary(k => k.UserId, v => v.TotalPunishmentAmount);

            var tk = from t in WorkScope.GetAll<Timekeeping>()
                     where t.DateAt.Year == year && t.DateAt.Month == month &&
                       (!userId.HasValue || t.UserId == userId.Value) &&
                       (!branchId.HasValue || t.User.BranchId == branchId.Value) &&
                       t.User.IsActive &&
                       (viewAll || t.UserId == AbpSession.UserId.Value)
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
                         NumberOfLeaveEarly = g.Count(x => x.IsPunishedCheckOut),
                         TotalPunishmentAmount = punishmentAmounts.ContainsKey(g.Key.UserId.Value) ? punishmentAmounts[g.Key.UserId.Value] : 0
                     };

            int totalPunishmentAmount = punishmentAmounts.Values.Sum();
            
            var gridResult = await tk.GetGridResult(tk, input);
            
            return new TardinessResultDto
            {
                GridResult = gridResult,
                TotalPunishmentAmount = totalPunishmentAmount
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_View)]
        [HttpGet]
        public async Task<List<UserPunishmentDetailDto>> GetDetailTimekeeping(
          int year, int month, int? day,
          long? userId, long? branchId,
          bool? isPunished, bool? isComplain,
          UserPunishmentType? statusPunish = null,
          PunishmentGroupType? groupType = null)
        {
            var checkInOutTypes = new[] {
            UserPunishmentType.Late,
              UserPunishmentType.NoCheckIn,
              UserPunishmentType.NoCheckOut,
              UserPunishmentType.LateAndNoCheckOut,
              UserPunishmentType.NoCheckInAndNoCheckOut
          };
            var trackerTypes = new[] {
            UserPunishmentType.Tracker_20k,
              UserPunishmentType.Tracker_50k,
              UserPunishmentType.Tracker_100k,
              UserPunishmentType.Tracker_200k
          };
            var pmReportTypes = new[] {
            UserPunishmentType.PMReport_20k,
              UserPunishmentType.PMReport_50k
          };

            var groupTypeMap = new Dictionary<PunishmentGroupType,
              UserPunishmentType?> {
              {
                PunishmentGroupType.NoPunish, UserPunishmentType.NoPunish
              },
              {
                PunishmentGroupType.Daily,
                UserPunishmentType.Daily
              },
              {
                PunishmentGroupType.Mention,
                UserPunishmentType.Mention
              },
              {
                PunishmentGroupType.ReviewIntern,
                UserPunishmentType.ReviewIntern
              },
              {
                PunishmentGroupType.Ant,
                UserPunishmentType.Ant
              },
              {
                PunishmentGroupType.UnlockTSGmail,
                UserPunishmentType.UnlockTSGmail
              },
              {
                PunishmentGroupType.UnlockTSIMS,
                UserPunishmentType.UnlockTSIMS
              },
              {
                PunishmentGroupType.UnlockTS_PM,
                UserPunishmentType.UnlockPM
              },
              {
                PunishmentGroupType.UnlockTS_Staff,
                UserPunishmentType.UnlockStaff
              },
              {
                PunishmentGroupType.PMOthers,
                UserPunishmentType.PMOthers
              }
            };

            var isRequestingNoPunish = (statusPunish == UserPunishmentType.NoPunish) ||
              (groupType == PunishmentGroupType.NoPunish) ||
              (statusPunish == null && isPunished == false && groupType == null);

            if (isRequestingNoPunish)
            {
                if ((statusPunish.HasValue && statusPunish != UserPunishmentType.NoPunish) ||
                  (groupType.HasValue && groupType != PunishmentGroupType.NoPunish))
                {
                    return new List<UserPunishmentDetailDto>();
                }

                var query = from t in WorkScope.GetAll<Timekeeping>()
                  .AsNoTracking()
                  .Where(t =>
                    t.DateAt.Year == year &&
                    t.DateAt.Month == month &&
                    t.StatusPunish == CheckInCheckOutPunishmentType.NoPunish &&
                    t.CountPunishDaily == 0 &&
                    t.CountPunishMention == 0 &&
                    (!userId.HasValue || t.UserId == userId.Value) &&
                    (!day.HasValue || day < 0 || t.DateAt.Day == day.Value) &&
                    (!isPunished.HasValue || t.IsPunishedCheckIn == isPunished.Value) &&
                    (!isComplain.HasValue || (string.IsNullOrEmpty(t.UserNote) != isComplain.Value))
                  )
                            join u in WorkScope.GetAll<User>().AsNoTracking() on t.UserId equals u.Id
                            where (!branchId.HasValue || u.BranchId == branchId.Value)
                            join tu in WorkScope.GetAll<User>().AsNoTracking() on t.LastModifierUserId equals tu.Id into tuGroup
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

                var noPunishResult = await query
                  .OrderByDescending(d => d.Date)
                  .ThenByDescending(d => d.IsPunished)
                  .ThenByDescending(d => d.ResultCheckIn)
                  .ToListAsync();

                return noPunishResult;
            }

            var upQuery = WorkScope.GetAll<UserPunishment>()
              .AsNoTracking()
              .Include(up => up.User).ThenInclude(u => u.Branch)
              .Where(up =>
                up.DateAt.Year == year &&
                up.DateAt.Month == month &&
                (!userId.HasValue || up.UserId == userId.Value) &&
                (!day.HasValue || day < 0 || up.DateAt.Day == day.Value) &&
                (!branchId.HasValue || up.User.BranchId == branchId.Value) &&
                (!groupType.HasValue ||
                  (groupType == PunishmentGroupType.CheckInOut && checkInOutTypes.Contains(up.Type)) ||
                  (groupType == PunishmentGroupType.Tracker && trackerTypes.Contains(up.Type)) ||
                  (groupType == PunishmentGroupType.PmReport && pmReportTypes.Contains(up.Type)) ||
                  (groupTypeMap.ContainsKey(groupType.Value) && up.Type == groupTypeMap[groupType.Value])) &&
                (!statusPunish.HasValue || up.Type == statusPunish.Value) &&
                (!isPunished.HasValue || (up.Type != UserPunishmentType.NoPunish) == isPunished.Value) &&
                (!isComplain.HasValue || (string.IsNullOrEmpty(up.UserNote) != isComplain.Value))
              );

            var upIdsTask = upQuery.Select(up => new {
                up.UserId,
                up.DateAt
            }).ToListAsync();
            var upIds = await upIdsTask;

            var userDates = upIds.Select(x => new {
                UserId = x.UserId,
                Date = x.DateAt.Date
            }).ToList();

            var tkQuery = WorkScope.GetAll<Timekeeping>()
              .AsNoTracking()
              .Where(t =>
                t.DateAt.Year == year &&
                t.DateAt.Month == month &&
                userDates.Any(ud => ud.UserId == t.UserId && ud.Date == t.DateAt.Date)
              );

            var userIdsTask = upQuery.Select(up => up.LastModifierUserId)
              .Union(tkQuery.Select(t => t.LastModifierUserId))
              .Where(id => id.HasValue)
              .Select(id => id.Value)
              .Distinct()
              .ToListAsync();

            var upTask = upQuery.ToListAsync();
            var tkTask = tkQuery.ToListAsync();
            var userIdsResult = await userIdsTask;

            var userTask = WorkScope.GetAll<User>()
              .AsNoTracking()
              .Where(u => userIdsResult.Contains(u.Id))
              .Select(u => new {
                  u.Id,
                  u.UserName
              })
              .ToListAsync();

            await Task.WhenAll(upTask, tkTask, userTask);

            var upList = upTask.Result;
            var tkList = tkTask.Result;
            var userList = userTask.Result;

            var tkDict = tkList.ToDictionary(
              t => new {
                  UserId = (long)t.UserId,
                  Date = t.DateAt.Date
              },
              t => t
            );

            var userDict = userList.ToDictionary(u => u.Id, u => u);

            var finalResult = new List<UserPunishmentDetailDto>();

            foreach (var up in upList)
            {
                var key = new
                {
                    UserId = (long)up.UserId,
                    Date = up.DateAt.Date
                };
                tkDict.TryGetValue(key, out
                  var t);

                var lastModId = t != null ? (long?)t.LastModifierUserId : null;
                userDict.TryGetValue(lastModId ?? 0, out
                  var tu);

                finalResult.Add(new UserPunishmentDetailDto
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
                });
            }

            if (statusPunish == null && (isPunished == null || isPunished == false) && (groupType == null || groupType == PunishmentGroupType.NoPunish))
            {
                var noPunishQuery = from t in WorkScope.GetAll<Timekeeping>()
                  .AsNoTracking()
                  .Where(t =>
                    t.DateAt.Year == year &&
                    t.DateAt.Month == month &&
                    t.StatusPunish == CheckInCheckOutPunishmentType.NoPunish &&
                    t.CountPunishDaily == 0 &&
                    t.CountPunishMention == 0 &&
                    (!userId.HasValue || t.UserId == userId.Value) &&
                    (!day.HasValue || day < 0 || t.DateAt.Day == day.Value) &&
                    (!isPunished.HasValue || t.IsPunishedCheckIn == isPunished.Value) &&
                    (!isComplain.HasValue || (string.IsNullOrEmpty(t.UserNote) != isComplain.Value))
                  )
                                    join u in WorkScope.GetAll<User>().AsNoTracking() on t.UserId equals u.Id
                                    where (!branchId.HasValue || u.BranchId == branchId.Value)
                                    join tu in WorkScope.GetAll<User>().AsNoTracking() on t.LastModifierUserId equals tu.Id into tuGroup
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
              finalResult
              .OrderByDescending(d => d.IsPunished)
              .ThenByDescending(d => d.ResultCheckIn)
              .ThenByDescending(d => d.Date)
              .ToList() :
              finalResult
              .OrderByDescending(d => d.Date)
              .ThenByDescending(d => d.IsPunished)
              .ThenByDescending(d => d.ResultCheckIn)
              .ToList();
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimeSheet_ViewMyTardinessDetail)]
        public async Task<List<GetTimekeepingUserDto>> GetMyDetails(int year, int month)
        {

            var targetMonth = new DateTime(year, month, 1);
            var totalPaidPunishment = await WorkScope.GetAll<UserPunishmentPaid> ()
              .Where(up => up.UserId == AbpSession.UserId &&
                up.TargetMonth.Year == year &&
                up.TargetMonth.Month == month)
              .SumAsync(up => (int ? ) up.Amount) ?? 0;

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

            var upLookup = upList.ToLookup(up => new {
                Date = up.DateAt.Date,
                UserId = (long)up.UserId
            });
            foreach (var tk in tkList)
            {
                if (!tk.UserId.HasValue)
                    continue;

                var matchingUps = upLookup[new
                {
                    Date = tk.DateAt.Date,
                    UserId = tk.UserId.Value
                }].ToList();

                var (totalDayPunishment, totalMonthPunishmentTotal) = CalculatePunishmentTotals(upList, tk.DateAt, tk.UserId.Value, tkList);

                if (matchingUps.Any())
                {
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

                            UserPunishmentType = up.Type,
                            MoneyPunish = up.TotalMoney,
                            TrackerTime = tk.TrackerTime,
                            DailyPunish = tk.CountPunishDaily,
                            MentionPunish = tk.CountPunishMention,
                            BranchColor = tk.User.Branch?.Color,
                            BranchDisplayName = tk.User.Branch?.DisplayName,
                            BranchId = tk.User.BranchId,
                            StatusPunish = tk.StatusPunish,
                            TotalDayPunishment = totalDayPunishment,
                            TotalMonthPunishmentTotal = totalMonthPunishmentTotal,
                            TotalPaidPunishment = totalPaidPunishment
                        });
                    }
                }
                else
                {

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

                        MoneyPunish = tk.MoneyPunish,
                        TrackerTime = tk.TrackerTime,
                        DailyPunish = tk.CountPunishDaily,
                        MentionPunish = tk.CountPunishMention,

                        BranchColor = tk.User.Branch?.Color,
                        BranchDisplayName = tk.User.Branch?.DisplayName,
                        BranchId = tk.User.BranchId,

                        StatusPunish = tk.StatusPunish,

                        TotalDayPunishment = totalDayPunishment,
                        TotalMonthPunishmentTotal = totalMonthPunishmentTotal,
                        TotalPaidPunishment = totalPaidPunishment
                    });
                }
            }

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

                    var (totalDayPunishment, totalMonthPunishmentTotal) = CalculatePunishmentTotals(upList, up.DateAt, up.UserId, tkList);

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
                        UserPunishmentType = up.Type,
                        MoneyPunish = up.TotalMoney,
                        BranchColor = up.User.Branch?.Color,
                        BranchDisplayName = up.User.Branch?.DisplayName,
                        BranchId = up.User.BranchId,

                        StatusPunish = CheckInCheckOutPunishmentType.NoPunish,
                        TotalDayPunishment = totalDayPunishment,
                        TotalMonthPunishmentTotal = totalMonthPunishmentTotal,
                        TotalPaidPunishment = totalPaidPunishment
                    });
                }
            }

            return result.OrderByDescending(t => t.Date).ToList();
        }

        private (int totalDayPunishment, decimal totalMonthPunishmentTotal) CalculatePunishmentTotals(
            List<UserPunishment> userPunishments,
            DateTime date,
            long userId,
            List<Timekeeping> timekeepings = null)
        {
            var dayPunishments = userPunishments
                .Where(up => up.DateAt.Date == date.Date && up.UserId == userId)
                .ToList();

            var monthPunishments = userPunishments
                .Where(up => up.DateAt.Year == date.Year &&
                            up.DateAt.Month == date.Month &&
                            up.UserId == userId)
                .ToList();

            int totalDayPunishment = dayPunishments.Sum(up => up.TotalMoney);
            decimal totalMonthPunishmentTotal = monthPunishments.Sum(up => up.TotalMoney);

            if (timekeepings != null && timekeepings.Any())
            {
                var daysWithUserPunishment = userPunishments
                    .Where(up => up.DateAt.Year == date.Year &&
                                up.DateAt.Month == date.Month &&
                                up.UserId == userId)
                    .Select(up => up.DateAt.Date)
                    .ToHashSet();

                var timekeepingPunishments = timekeepings
                    .Where(t => t.UserId == userId &&
                               t.StatusPunish > 0 &&
                               t.DateAt.Year == date.Year &&
                               t.DateAt.Month == date.Month)
                    .GroupBy(t => new { t.DateAt.Date, t.StatusPunish })
                    .Select(g => new
                    {
                        g.Key.Date,
                        g.Key.StatusPunish,
                        MoneyPunish = g.Sum(x => x.MoneyPunish)
                    })
                    .ToList();

                var currentDayPunishment = timekeepingPunishments
                    .Where(t => t.Date == date.Date && 
                              !userPunishments.Any(up => up.DateAt.Date == t.Date && 
                                                       (int)up.Type == (int)t.StatusPunish))
                    .Sum(t => t.MoneyPunish);

                totalDayPunishment += currentDayPunishment;

                var currentMonthPunishment = timekeepingPunishments
                    .Where(t => !userPunishments.Any(up => up.DateAt.Date == t.Date && 
                                                         (int)up.Type == (int)t.StatusPunish))
                    .Sum(t => t.MoneyPunish);

                totalMonthPunishmentTotal += currentMonthPunishment;
            }

            return (totalDayPunishment, (int)totalMonthPunishmentTotal);
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
            var user = await WorkScope.GetAsync<User>(t.UserId ?? 0);
            if (user.Type != Usertype.Vendor)
            {
                await timekeepingServices.CheckIsPunished(t);
                await timekeepingServices.CheckIsPunishedByRule(t, LimitedMinute, DateTimeUtils.ConvertHHmmssToMinutes(input.TrackerTime));
            }
            await WorkScope.GetRepo<Timekeeping>().UpdateAsync(t);
            return t;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_UserNote)]
        [HttpPost]
        public async Task<UserComplaintResultDto> UserKhieuLai(SubmitUserComplaintDto input)
        {
            try
            {

                var userPunishment = await WorkScope.GetAll<UserPunishment>()
                  .Where(x => x.Id == input.UserPunishmentId)
                  .FirstOrDefaultAsync();

                if (userPunishment == null)
                {
                    throw new UserFriendlyException("Punishment record not found!");
                }

                if (userPunishment.UserId != AbpSession.UserId.Value)
                {
                    throw new UserFriendlyException("You can only submit complaints for your own records");
                }

                string userNote = !string.IsNullOrWhiteSpace(input.UserNote) ? input.UserNote : null;
                
                userPunishment.UserNote = userNote;
                await WorkScope.UpdateAsync(userPunishment);

                Timekeeping timekeeping = null;

                if ((int)userPunishment.Type >= (int)UserPunishmentType.NoPunish &&
                    (int)userPunishment.Type <= (int)UserPunishmentType.NoCheckInAndNoCheckOut)
                {
                    timekeeping = await WorkScope.GetAll<Timekeeping>()
                      .Where(t => t.UserId == userPunishment.UserId &&
                        t.DateAt.Date == userPunishment.DateAt.Date)
                      .FirstOrDefaultAsync();
                    if (timekeeping != null)
                    {
                        timekeeping.UserNote = userNote;
                        await WorkScope.UpdateAsync(timekeeping);
                    }
                }

                var punishmentTypeName = GetPunishmentTypeName(userPunishment.Type);

                await CurrentUnitOfWork.SaveChangesAsync();
                return new UserComplaintResultDto
                {
                    UserPunishmentId = userPunishment.Id,
                    TimekeepingId = timekeeping?.Id,
                    UserNote = input.UserNote,
                    PunishmentType = userPunishment.Type,
                    PunishmentTypeName = punishmentTypeName,
                    Success = true,
                    Message = "Complaint updated successfully"
                };
            }
            catch (UserFriendlyException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in UserKhieuLai: " + ex.Message, ex);
                throw new UserFriendlyException("An error occurred while processing the complaint!");
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

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_UserNote)]
        [HttpPost]
        [UnitOfWork(IsolationLevel.ReadCommitted)]
        public async Task<RespondToComplaintResultDto> RespondToComplaint(RespondToComplaintDto input)
        {
            try
            {
                var userPunishment = await this.WorkScope.GetAll<UserPunishment>()
                    .Where(x => x.Id == input.UserpunishmentId)
                    .FirstOrDefaultAsync() ??
                    throw new UserFriendlyException("No matching record found to update.");

                var oldPunishmentType = userPunishment.Type;
                var oldPunishmentMoney = userPunishment.TotalMoney; 
                var newPunishmentType = input.StatusPunish;

                var timekeeping = await this.WorkScope.GetAll<Timekeeping>()
                    .Where(t => t.UserId == userPunishment.UserId &&
                             t.DateAt.Date == userPunishment.DateAt.Date)
                    .FirstOrDefaultAsync();

                if (oldPunishmentType == UserPunishmentType.Daily || oldPunishmentType == UserPunishmentType.Mention)
                {
                    if (input.ChangeCount.HasValue)
                    {
                        return await HandleChangePunishmentCount(userPunishment, input, timekeeping);
                    }
                }

                ValidatePunishmentTypeChange(oldPunishmentType, newPunishmentType);
                PunishmentSystem punishmentSystem = null;
                if (newPunishmentType != UserPunishmentType.NoPunish)
                {
                    punishmentSystem = await WorkScope.GetAll<PunishmentSystem>()
                        .Where(x => x.Type == newPunishmentType && x.IsActive)
                        .OrderByDescending(x => x.CreationTime)
                        .FirstOrDefaultAsync() ??
                        throw new UserFriendlyException("No matching punishment configuration found.");
                }

                await HandleRefundForPaidPunishment(userPunishment, newPunishmentType, punishmentSystem);

                var result = await HandlePunishmentTypeChange(
                    userPunishment,
                    oldPunishmentType,
                    newPunishmentType,
                    punishmentSystem,
                    timekeeping,
                    input.NoteReply);

                await CurrentUnitOfWork.SaveChangesAsync();
                return result;
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Error while updating punishment: " + ex.Message, ex);
                throw new UserFriendlyException("Sorry, something went wrong while updating the userpunishments.");
            }
        }

        private async Task<RespondToComplaintResultDto> HandlePunishmentTypeChange(
            UserPunishment userPunishment,
            UserPunishmentType oldType,
            UserPunishmentType newType,
            PunishmentSystem punishmentSystem,
            Timekeeping timekeeping,
            string noteReply)
        {
            await HandleOldPunishmentType(timekeeping, oldType, newType, userPunishment.Count);

            userPunishment.Type = newType;
            userPunishment.NoteReply = noteReply;

            if (punishmentSystem != null)
            {
                userPunishment.PunishmentSystemId = punishmentSystem.Id;
                userPunishment.TotalMoney = punishmentSystem.Money * (userPunishment.Count > 0 ? userPunishment.Count : 1);
            }
            else if (newType == UserPunishmentType.NoPunish)
            {
                await WorkScope.GetRepo<UserPunishment>().DeleteAsync(userPunishment.Id);

                if (timekeeping != null)
                {
                    timekeeping.NoteReply = noteReply;
                    await WorkScope.GetRepo<Timekeeping>().UpdateAsync(timekeeping);
                }

                return new RespondToComplaintResultDto
                {
                    Success = true,
                    PunishmentId = 0,
                    PunishmentType = UserPunishmentType.NoPunish.ToString(),
                    PunishmentMoney = 0,
                    RemainingCount = 0
                };
            }

            await HandleNewPunishmentType(timekeeping, newType, punishmentSystem);

            await WorkScope.GetRepo<UserPunishment>().UpdateAsync(userPunishment);

            if (timekeeping != null)
            {
                timekeeping.NoteReply = noteReply;
                await WorkScope.GetRepo<Timekeeping>().UpdateAsync(timekeeping);
            }

            return new RespondToComplaintResultDto
            {
                Success = true,
                PunishmentId = userPunishment.Id,
                PunishmentType = userPunishment.Type.ToString(),
                PunishmentMoney = userPunishment.TotalMoney,
                RemainingCount = userPunishment.Count
            };
        }

        private async Task HandleOldPunishmentType(
            Timekeeping timekeeping,
            UserPunishmentType oldType,
            UserPunishmentType newType,
            int count)
        {
            if (timekeeping == null) return;

            if (IsCheckInOutPunishment(oldType) &&
                (!IsCheckInOutPunishment(newType) || oldType != newType))
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                timekeeping.MoneyPunish = 0;
                timekeeping.IsPunishedCheckIn = false;
                timekeeping.IsPunishedCheckOut = false;
            }
            if (newType == UserPunishmentType.NoPunish)
            {
                if (oldType == UserPunishmentType.Daily)
                    timekeeping.CountPunishDaily = 0;

                if (oldType == UserPunishmentType.Mention)
                    timekeeping.CountPunishMention = 0;
            }
        }

        private bool IsCheckInOutPunishment(UserPunishmentType type)
        {
            return type >= UserPunishmentType.Late &&
                   type <= UserPunishmentType.NoCheckInAndNoCheckOut;
        }

        private async Task HandleNewPunishmentType(
            Timekeeping timekeeping,
            UserPunishmentType newType,
            PunishmentSystem punishmentSystem)
        {
            if (timekeeping == null || punishmentSystem == null) return;

            if (IsCheckInOutPunishment(newType))
            {
                timekeeping.StatusPunish = (CheckInCheckOutPunishmentType)newType;
                timekeeping.MoneyPunish = punishmentSystem.Money;

                timekeeping.IsPunishedCheckIn = newType == UserPunishmentType.NoCheckIn ||
                                              newType == UserPunishmentType.NoCheckInAndNoCheckOut;
                timekeeping.IsPunishedCheckOut = newType == UserPunishmentType.NoCheckOut ||
                                               newType == UserPunishmentType.LateAndNoCheckOut ||
                                               newType == UserPunishmentType.NoCheckInAndNoCheckOut;
            }
        }

        private async Task<RespondToComplaintResultDto> HandleChangePunishmentCount(
          UserPunishment userPunishment,
          RespondToComplaintDto input,
          Timekeeping timekeeping)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                try
                {
                    var newCount = input.ChangeCount.Value;

                    if (newCount <= 0)
                    {
                        var punishmentType = userPunishment.Type;

                        await WorkScope.GetRepo<UserPunishment>().DeleteAsync(userPunishment.Id);

                        if (timekeeping != null)
                        {
                            if (punishmentType == UserPunishmentType.Mention)
                            {
                                timekeeping.CountPunishMention = 0;
                            }
                            else if (punishmentType == UserPunishmentType.Daily)
                            {
                                timekeeping.CountPunishDaily = 0;
                            }

                            timekeeping.NoteReply = input.NoteReply;
                            await WorkScope.GetRepo<Timekeeping>().UpdateAsync(timekeeping);
                        }

                        await uow.CompleteAsync();

                        return new RespondToComplaintResultDto
                        {
                            Success = true,
                            PunishmentId = 0,
                            PunishmentType = UserPunishmentType.NoPunish.ToString(),
                            PunishmentMoney = 0,
                            RemainingCount = 0
                        };
                    }

                    userPunishment.Count = newCount;

                    if (timekeeping != null)
                    {
                        if (userPunishment.Type == UserPunishmentType.Mention)
                        {
                            timekeeping.CountPunishMention = newCount;
                        }
                        else if (userPunishment.Type == UserPunishmentType.Daily)
                        {
                            timekeeping.CountPunishDaily = newCount;
                        }

                        timekeeping.NoteReply = input.NoteReply;
                        await WorkScope.GetRepo<Timekeeping>().UpdateAsync(timekeeping);
                    }

                    var punishmentSystem = await WorkScope.GetAll<PunishmentSystem>()
                      .Where(x => x.Type == userPunishment.Type && x.IsActive)
                      .OrderByDescending(x => x.CreationTime)
                      .FirstOrDefaultAsync();

                    if (punishmentSystem != null)
                    {
                        userPunishment.TotalMoney = userPunishment.Count * punishmentSystem.Money;
                    }

                    userPunishment.NoteReply = input.NoteReply;
                    await WorkScope.GetRepo<UserPunishment>().UpdateAsync(userPunishment);

                    await uow.CompleteAsync();

                    return new RespondToComplaintResultDto
                    {
                        Success = true,
                        PunishmentId = userPunishment.Id,
                        PunishmentType = userPunishment.Type.ToString(),
                        PunishmentMoney = userPunishment.TotalMoney,
                        RemainingCount = userPunishment.Count
                    };
                }
                catch (Exception)
                {
                    uow.Dispose();
                    throw;
                }
            }
        }

        private void ValidatePunishmentTypeChange(UserPunishmentType oldType, UserPunishmentType newType)
        {
            if (newType == UserPunishmentType.NoPunish)
                return;

            var specialTypes = new Dictionary<UserPunishmentType, string>
            {
                { UserPunishmentType.Daily, "Daily" },
                { UserPunishmentType.Mention, "Mention" },
                { UserPunishmentType.ReviewIntern, "ReviewIntern" },
                { UserPunishmentType.Ant, "Ant" },
                { UserPunishmentType.UnlockTSGmail, "UnlockTS Gmail" },
                { UserPunishmentType.UnlockTSIMS, "UnlockTS IMS" }
            };

            if (specialTypes.ContainsKey(oldType) && oldType != newType)
            {
                throw new UserFriendlyException($"Can't change type {specialTypes[oldType]} to another type");
            }

            var oldGroup = GetPunishmentGroup(oldType);
            var newGroup = GetPunishmentGroup(newType);
            
            if (oldGroup != newGroup)
            {
                var groupErrorMessages = new Dictionary<string, string>
                {
                    { "Tracker", "You can only switch between Tracker type" },
                    { "CheckInOut", "You can only switch between checkin/checkout type" },
                    { "PMReport", "You can only switch between PM Report type" },
                };
                
                var errorMessage = groupErrorMessages.ContainsKey(oldGroup) 
                    ? groupErrorMessages[oldGroup] 
                    : "You can't switch between punishment types from different groups";
                    
                throw new UserFriendlyException(errorMessage);
            }
        }

        private string GetPunishmentGroup(UserPunishmentType type)
        {
            if (type >= UserPunishmentType.Late && type <= UserPunishmentType.NoCheckInAndNoCheckOut)
                return "CheckInOut";

            if (type >= UserPunishmentType.Tracker_20k && type <= UserPunishmentType.Tracker_200k)
                return "Tracker";

            if (type == UserPunishmentType.PMReport_20k || type == UserPunishmentType.PMReport_50k)
                return "PMReport";

            return "Other";
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
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timekeeping_UserNote)]
        [HttpPost]
        public async Task DeleteComplaint(DeleteComplaintDto input)
        {
            var userPunishment = await WorkScope.GetAsync<UserPunishment>(input.UserPunishmentId);
            if (userPunishment == null)
            {
                throw new UserFriendlyException("Punishment record not found!");
            }
            if (userPunishment.UserId != AbpSession.UserId.Value)
            {
                throw new UserFriendlyException("You can only delete complaints for your own records");
            }
            userPunishment.UserNote = null;
            await WorkScope.GetRepo<UserPunishment>().UpdateAsync(userPunishment);
            var timekeeping = await WorkScope.GetAll<Timekeeping>()
                .Where(t => t.UserId == userPunishment.UserId &&
                         t.DateAt.Date == userPunishment.DateAt.Date)
                .FirstOrDefaultAsync();
            
            if (timekeeping != null)
            {
                timekeeping.UserNote = null;
                await WorkScope.GetRepo<Timekeeping>().UpdateAsync(timekeeping);
            }
        }

        private async Task HandleRefundForPaidPunishment(
            UserPunishment userPunishment, 
            UserPunishmentType newPunishmentType, 
            PunishmentSystem punishmentSystem)
        {
            var oldPunishmentMoney = userPunishment.TotalMoney; 
            int amountReduced = 0;

            if (newPunishmentType == UserPunishmentType.NoPunish)
            {
                amountReduced = oldPunishmentMoney;
            }
            else if (punishmentSystem != null)
            {
                var newPunishmentMoney = punishmentSystem.Money * (userPunishment.Count > 0 ? userPunishment.Count : 1);
                amountReduced = Math.Max(0, oldPunishmentMoney - newPunishmentMoney);
            }

            if (amountReduced > 0)
            {
                await UpdateUserPunishmentBalance(userPunishment.UserId, amountReduced, userPunishment.IsPaid);
                Logger.Info($"Updated UserPunishmentBalance for user {userPunishment.UserId}: reduced {amountReduced}, isPaid: {userPunishment.IsPaid}");
            }

            if (userPunishment.IsPaid && amountReduced > 0)
            {
                var refund = new UserPunishmentRefund
                {
                    UserId = userPunishment.UserId,
                    UserPunishmentId = userPunishment.Id,
                    Points = amountReduced,
                    Type = PointType.IsClaim
                };
                await WorkScope.InsertAsync(refund);
                Logger.Info($"Created refund {amountReduced} points for user {userPunishment.UserId}, punishment {userPunishment.Id}");
            }
        }

        private async Task UpdateUserPunishmentBalance(long userId, int punishmentAmountReduced = 0, bool isPaid = false)
        {
            var balance = await WorkScope.GetAll<UserPunishmentBalance>()
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (balance == null)
            {
                balance = new UserPunishmentBalance
                {
                    UserId = userId,
                    TotalPunishmentMoney = 0,
                    RemainPoints = isPaid ? punishmentAmountReduced : 0 
                };
                await WorkScope.InsertAsync(balance);
                Logger.Info($"Created new UserPunishmentBalance for user {userId} with RemainPoints = {(isPaid ? punishmentAmountReduced : 0)} (isPaid: {isPaid})");
            }
            else
            {
                if (isPaid)
                {
                    balance.RemainPoints += punishmentAmountReduced;
                }
                else
                {
                    balance.TotalPunishmentMoney = Math.Max(0, balance.TotalPunishmentMoney - punishmentAmountReduced);
                }
                
                await WorkScope.UpdateAsync(balance);
                Logger.Info($"Updated UserPunishmentBalance for user {userId}: {(isPaid ? $"added {punishmentAmountReduced} to RemainPoints" : $"reduced TotalPunishmentMoney by {punishmentAmountReduced}")}. TotalPunishmentMoney = {balance.TotalPunishmentMoney}, RemainPoints = {balance.RemainPoints}");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_TardinessLeaveEarly_RetrieveData)]
        [HttpPost]
        public async Task<bool> SnapshotTimekeepingDay(DateTime date)
        {
            return await timekeepingServices.SnapshotUserPunishmentsForDay(date);
        }
    }
}