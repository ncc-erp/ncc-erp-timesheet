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
using System.Transactions;
using Abp.Domain.Uow;
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

            if (oldType == UserPunishmentType.Daily || oldType == UserPunishmentType.Mention)
            {
                throw new UserFriendlyException("Can't change type Daily/Mention");
            }
            
            if (oldType == UserPunishmentType.ReviewIntern)
            {
                throw new UserFriendlyException("Can't change type ReviewIntern");
            }

            var oldGroup = GetPunishmentGroup(oldType);

            var newGroup = GetPunishmentGroup(newType);
            if (oldGroup != newGroup)
            {
                string errorMessage;
                switch (oldGroup)
                {
                    case "Tracker":
                        errorMessage = "You can only switch between Tracker type";
                        break;
                    case "CheckInOut":
                        errorMessage = "You can only switch between checkin/checkout type";
                        break;
                    case "PMReport":
                        errorMessage = "You can only switch between PM Report type";
                        break;
                    default:
                        errorMessage = "You can't switch between punishment types from different groups";
                        break;
                }
                throw new UserFriendlyException(errorMessage);
            }
        }
        
        private string GetPunishmentGroup(UserPunishmentType type)
        {
            if (type == UserPunishmentType.ReviewIntern) 
                return "ReviewIntern";
                
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
    }
}
