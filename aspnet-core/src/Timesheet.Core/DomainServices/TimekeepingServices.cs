using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Word;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities.Enum;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Services.FaceIdService;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Services.Project.Dto;
using Timesheet.Services.Tracker;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using static Sieve.Extensions.MethodInfoExtended;

namespace Timesheet.DomainServices
{
    public class TimekeepingServices : BaseDomainService, ITimekeepingServices, ITransientDependency
    {
        private readonly KomuService _komuService;
        private readonly TrackerService _trackerService;
        private readonly FaceIdService _faceIdService;
        private readonly ProjectService _projectService;

        public TimekeepingServices(KomuService komuService, TrackerService trackerService, IWorkScope workScope, FaceIdService faceIdService, ProjectService projectService) : base(workScope)
        {
            _komuService = komuService;
            _trackerService = trackerService;
            _faceIdService = faceIdService;
            _projectService = projectService;
        }

        [UnitOfWork]
        public async Task<List<Timekeeping>> AddTimekeepingByDay(DateTime selectedDate)
        {
            var isOffDate = WorkScope.GetAll<DayOffSetting>()
                .Where(s => s.DayOff.Date == selectedDate.Date)
                .Any();

            if (isOffDate)
            {
                throw new UserFriendlyException($"{selectedDate.ToString("MM/dd/yyyy HH:mm:ss")} is Off Date => stop");
            }

            var users = WorkScope.GetAll<User>()
                .Where(u => u.IsActive)
                .Where(u => u.StartDateAt <= selectedDate)
                .Select(u => new TimesheetUserDto
                {
                    UserId = u.Id,
                    IsStopWork = u.IsStopWork,
                    StopWorkingDate = u.EndDateAt,
                    EmailAddress = u.EmailAddress,
                    MorningStartAt = u.MorningStartAt,
                    MorningEndAt = u.MorningEndAt,
                    MorningWorking = u.MorningWorking,
                    AfternoonStartAt = u.AfternoonStartAt,
                    AfternoonEndAt = u.AfternoonEndAt,
                    AfternoonWorking = u.AfternoonWorking,
                }).ToList();

            if (users.Count < 1)
            {
                throw new UserFriendlyException("The day " + selectedDate + " is not a working day for any branch.");
            }

            var listTimekeepingOldBySelectedDate = WorkScope.GetAll<Timekeeping>()
                .Where(s => s.DateAt.Date == selectedDate.Date)
                .ToList();

            var listDicUserIdToNote = listTimekeepingOldBySelectedDate
                .GroupBy(s => s.UserId != null ? s.UserId : 0)
                .ToDictionary(s => s.Key,
                s => s.Select(x => new
                {
                    x.NoteReply,
                    x.UserNote
                })).ToList();

            listTimekeepingOldBySelectedDate.ForEach(s =>
            {
                s.IsDeleted = true;
                s.DeletionTime = DateTimeUtils.GetNow();
            });
            CurrentUnitOfWork.SaveChanges();

            var oldPunishments = await WorkScope.GetAll<UserPunishment>()
               .Where(p => p.DateAt.Date == selectedDate.Date)
               .ToListAsync();
            if (oldPunishments.Any())
            {
                oldPunishments.ForEach(p =>
                {
                    p.IsDeleted = true;
                    p.DeletionTime = DateTimeUtils.GetNow();
                });
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var allApprovedRequests = WorkScope.GetAll<AbsenceDayDetail>().Include(s => s.Request).Where(s => s.DateAt.Date == selectedDate.Date && s.Request.Status == RequestStatus.Approved).ToList();

            var mapAbsenceUsers = allApprovedRequests.Where(s => s.Request.Type != RequestType.Remote).GroupBy(s => s.Request.UserId).ToDictionary(
            s => s.Key, s => s.Select(x => new MapAbsenceUserDto
            {
                UserId = x.Request.UserId,
                DateType = x.DateType,
                AbsenceTime = x.AbsenceTime,
                Hour = x.Hour,
                Type = x.Request.Type,
            }).OrderBy(x => x.AbsenceTime).ToList());

            var mapRemoteUsers = allApprovedRequests.Where(s => s.Request.Type == RequestType.Remote).GroupBy(s => s.Request.UserId).ToDictionary(
            s => s.Key, s => s.Select(x => new MapAbsenceUserDto
            {
                UserId = x.Request.UserId,
                DateType = x.DateType,
                AbsenceTime = x.AbsenceTime,
                Hour = x.Hour,
                Type = x.Request.Type,
            }).OrderBy(x => x.AbsenceTime).ToList());

            var rs = new List<Timekeeping>();
            var LimitedMinute = Int32.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));

            var checkInUsers = _faceIdService.GetEmployeeCheckInOutMini(selectedDate);
            if (checkInUsers == null)
            {
                return default;
            }

            var dailyAndMentionPunishs = _komuService.GetDailyReport(selectedDate);
            if (dailyAndMentionPunishs == null)
            {
                return default;
            }
            var listDaily = dailyAndMentionPunishs.daily.ToList();
            var listMention = dailyAndMentionPunishs.mention.ToList();

            var mapCheckInUsers = checkInUsers.ToDictionary(s => s.Email, s => s);
            var mapDailyUsers = listDaily.ToDictionary(s => s.email, s => s.count);
            var mapMentionUsers = listMention.ToDictionary(s => s.email, s => s.count);

            var listUserName = users.Select(x => x.UserName).Distinct().ToList();
            var userTrackerTimes = _trackerService.GetTimeTrackerToDay(selectedDate, listUserName);
            var dicUserNameToTrackerTime = userTrackerTimes.ToDictionary(s => s.email, s => new { s.ActiveMinute, s.active_time });

            var punishmentTypes = Enum.GetValues(typeof(UserPunishmentType))
                .Cast<UserPunishmentType>()
                .ToArray();
            var punishmentSystems = await WorkScope.GetAll<PunishmentSystem>()
                .Where(x => punishmentTypes.Contains(x.Type))
                .ToDictionaryAsync(x => x.Type, x => x);

            var userPunishmentsToInsert = new List<UserPunishment>();

            foreach (var user in users)
            {
                var t = new Timekeeping { };
                bool isRemoteWork = mapRemoteUsers.ContainsKey(user.UserId);

                var registerCheckInOut = isRemoteWork ?
                  CaculateCheckInOutTimeNew(mapRemoteUsers, user)
                  :
                  CaculateCheckInOutTimeNew(mapAbsenceUsers, user);

                float trackerTime = dicUserNameToTrackerTime.ContainsKey(user.UserName) ? dicUserNameToTrackerTime[user.UserName].ActiveMinute : 0;

                t.RegisterCheckIn = registerCheckInOut.CheckIn;
                t.RegisterCheckOut = registerCheckInOut.CheckOut;
                t.NoteReply = registerCheckInOut.Note;

                if (mapCheckInUsers.ContainsKey(user.EmailAddress))
                {
                    var checkInUser = mapCheckInUsers[user.EmailAddress];
                    t.CheckIn = checkInUser?.VerifyStartTimeStr;
                    t.CheckOut = checkInUser?.VerifyEndTimeStr;
                    if (registerCheckInOut.AbsenceDayType == DayType.Afternoon)
                    {
                        ChangeCheckInCheckOutTimeIfCheckOutIsEmptyCaseOffAfternoon(t);
                    }
                    else
                    {
                        ChangeCheckInCheckOutTimeIfCheckOutIsEmpty(t);
                    }
                }

                if (mapDailyUsers.ContainsKey(user.UserName))
                {
                    t.CountPunishDaily = mapDailyUsers[user.UserName];
                }

                if (mapMentionUsers.ContainsKey(user.UserName))
                {
                    t.CountPunishMention = mapMentionUsers[user.UserName];
                }

                listDicUserIdToNote.ForEach(item =>
                {
                    if (user.UserId == item.Key)
                    {
                        t.NoteReply = item.Value.Select(s => s.NoteReply).FirstOrDefault();
                        t.UserNote = item.Value.Select(s => s.UserNote).FirstOrDefault();
                    }
                });

                t.UserEmail = user.EmailAddress;
                t.DateAt = selectedDate;
                t.UserId = user.UserId;

                await CheckIsPunished(t, LimitedMinute);
                await CheckIsPunishedByRule(t, LimitedMinute, trackerTime);
                if (user.IsStopWork || (user.StopWorkingDate.HasValue && user.StopWorkingDate.Value.Date < selectedDate))
                {
                    t.IsPunishedCheckIn = false;
                    t.IsPunishedCheckOut = false;
                    t.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                    t.MoneyPunish = 0;
                }
                if (selectedDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    t.RegisterCheckIn = "10:00";
                    t.RegisterCheckOut = "12:00";
                    t.NoteReply += " Saturday";
                    t.IsPunishedCheckIn = false;
                    t.IsPunishedCheckOut = false;
                    t.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                    t.MoneyPunish = 0;
                }
                else if (selectedDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    t.RegisterCheckIn = "";
                    t.RegisterCheckOut = "";
                    t.NoteReply = "Sunday";
                    t.IsPunishedCheckIn = false;
                    t.IsPunishedCheckOut = false;
                    t.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                    t.MoneyPunish = 0;
                }

                t.TrackerTime = dicUserNameToTrackerTime.ContainsKey(user.UserName) ? dicUserNameToTrackerTime[user.UserName].active_time : "0";
                try
                {
                    t.Id = WorkScope.InsertAndGetId<Timekeeping>(t);
                    rs.Add(t);

                    if (t.StatusPunish != CheckInCheckOutPunishmentType.NoPunish)
                    {
                        var punishmentSystem = punishmentSystems.Values.FirstOrDefault(x =>
                        (StatusEnum.CheckInCheckOutPunishmentType)x.Type == t.StatusPunish);
                        if (punishmentSystem != null)
                        {
                            userPunishmentsToInsert.Add(new UserPunishment
                            {
                                DateAt = selectedDate,
                                UserId = user.UserId,
                                PunishmentSystemId = punishmentSystem.Id,
                                Type = punishmentSystem.Type,
                                Count = 1,
                                TotalMoney = punishmentSystem.Money,
                                UserNote = t.UserNote,
                                NoteReply = t.NoteReply
                            });
                        }
                    }

                    if (trackerTime > 0 && isRemoteWork)
                    {
                        var registerWorkingMinutes = CommonUtils.GetEmployeeWorkingHours(t.RegisterCheckOut, t.RegisterCheckIn);
                        var dayOffType = registerCheckInOut.AbsenceDayType;
                        var trackerPunishment = await CreateTrackerTimePunishment(selectedDate, user.UserId, trackerTime, registerWorkingMinutes, t.UserNote, t.NoteReply, dayOffType, punishmentSystems);
                        if (trackerPunishment != null)
                        {
                            userPunishmentsToInsert.Add(trackerPunishment);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"INSERT DATA ISSUE (Timekeeping) email: {user?.EmailAddress ?? "Unknown"}, UserId: {user?.UserId}, Error: {e}");
                }
            }

            if (userPunishmentsToInsert.Any())
            {
                await WorkScope.InsertRangeAsync(userPunishmentsToInsert);
            }

            var userEmails = users.Select(s => s.EmailAddress).ToHashSet();
            var checkInUsersOnly = checkInUsers.Where(s => !userEmails.Contains(s.Email));

            foreach (var checkIn in checkInUsersOnly)
            {
                var t = new Timekeeping
                {
                    UserEmail = checkIn.Email,
                    CheckIn = checkIn?.VerifyStartTimeStr,
                    CheckOut = checkIn?.VerifyEndTimeStr,
                    DateAt = selectedDate,
                    NoteReply = "Email not match",
                    TrackerTime = dicUserNameToTrackerTime.ContainsKey(checkIn.Email.Split("@")[0]) ? dicUserNameToTrackerTime[checkIn.Email.Split("@")[0]].active_time : "0",
                };
                ChangeCheckInCheckOutTimeIfCheckOutIsEmpty(t);

                try
                {
                    t.Id = WorkScope.InsertAndGetId<Timekeeping>(t);
                    rs.Add(t);
                }
                catch (Exception e)
                {
                    Logger.Error($"INSERT DATA ISSUE email: {t.User?.EmailAddress} Error: {e.Message}");
                }
            }

            await SaveDailyAndMentionPunishments(selectedDate, users, mapDailyUsers, mapMentionUsers, punishmentSystems);
            return rs;
        }

        public async System.Threading.Tasks.Task SaveDailyAndMentionPunishments(DateTime selectedDate, List<TimesheetUserDto> users,
          Dictionary<string, int> mapDailyUsers, Dictionary<string, int> mapMentionUsers, Dictionary<UserPunishmentType, PunishmentSystem> punishmentSystems)
        {
            var dailyMentionPunishments = new List<UserPunishment>();

            foreach (var user in users)
            {
                if (mapDailyUsers.ContainsKey(user.UserName) && mapDailyUsers[user.UserName] > 0)
                {
                    var dailyPunishment = punishmentSystems[UserPunishmentType.Daily];
                    dailyMentionPunishments.Add(new UserPunishment
                    {
                        DateAt = selectedDate,
                        UserId = user.UserId,
                        PunishmentSystemId = dailyPunishment.Id,
                        Type = dailyPunishment.Type,
                        Count = mapDailyUsers[user.UserName],
                        TotalMoney = mapDailyUsers[user.UserName] * dailyPunishment.Money,
                    });
                }

                if (mapMentionUsers.ContainsKey(user.UserName) && mapMentionUsers[user.UserName] > 0)
                {
                    var mentionPunishment = punishmentSystems[UserPunishmentType.Mention];
                    dailyMentionPunishments.Add(new UserPunishment
                    {
                        DateAt = selectedDate,
                        UserId = user.UserId,
                        PunishmentSystemId = mentionPunishment.Id,
                        Type = mentionPunishment.Type,
                        Count = mapMentionUsers[user.UserName],
                        TotalMoney = mapMentionUsers[user.UserName] * mentionPunishment.Money,
                    });
                }
            }

            if (dailyMentionPunishments.Any())
            {
                await WorkScope.InsertRangeAsync(dailyMentionPunishments);
            }
        }

        public async Task<UserPunishment> CreateTrackerTimePunishment(DateTime selectedDate, long userId, float trackerTime, double registerWorkingMinutes, string userNote, string noteReply, DayType? dayOffType, Dictionary<UserPunishmentType, PunishmentSystem> punishmentSystems)
        {
            var punishmentLevels = new List<(UserPunishmentType Type, double MinPercentage, double? MaxPercentage)> {
            (UserPunishmentType.Tracker_200k, 0, 25),
            (UserPunishmentType.Tracker_100k, 25, 50),
            (UserPunishmentType.Tracker_50k, 50, 75),
            (UserPunishmentType.Tracker_20k, 75, 85)
          };

            double standardWorkingHours = dayOffType == DayType.Morning || dayOffType == DayType.Afternoon ? 4 : registerWorkingMinutes / 60;
            double percentage = standardWorkingHours > 0 ? (trackerTime / 60.0 / standardWorkingHours) * 100 : 0;

            var punishmentLevel = punishmentLevels.FirstOrDefault(x => percentage >= x.MinPercentage && (x.MaxPercentage == null || percentage < x.MaxPercentage));

            if (punishmentLevel !=
              default && punishmentSystems.TryGetValue(punishmentLevel.Type, out
                var punishmentSystem))
            {
                return new UserPunishment
                {
                    DateAt = selectedDate,
                    UserId = userId,
                    PunishmentSystemId = punishmentSystem.Id,
                    Type = punishmentSystem.Type,
                    Count = 1,
                    TotalMoney = punishmentSystem.Money,
                    UserNote = userNote,
                    NoteReply = noteReply
                };
            }
            return null;
        }

        public void ChangeCheckInCheckOutTimeIfCheckOutIsEmpty(Timekeeping t)
        {
            try
            {
                string enableChangeTime = SettingManager.GetSettingValueForApplication(AppSettingNames.TimeStartChangingCheckinToCheckoutEnable);
                if (enableChangeTime != "true")
                {
                    return;
                }
                if (!string.IsNullOrEmpty(t.CheckIn))
                {
                    int checkInMinutes = DateTimeUtils.ConvertHHmmssToMinutes(t.CheckIn);
                    int timeStartChangingCheckInToCheckOutMinutes = DateTimeUtils.ConvertHHmmssToMinutes(SettingManager.GetSettingValue(AppSettingNames.TimeStartChangingCheckinToCheckout));

                    if (checkInMinutes >= timeStartChangingCheckInToCheckOutMinutes)
                    {
                        if (!string.IsNullOrEmpty(t.RegisterCheckIn))
                        {
                            int registerCheckInTime = DateTimeUtils.ConvertHHmmssToMinutes(t.RegisterCheckIn);
                            if (registerCheckInTime > 0 && checkInMinutes > registerCheckInTime)
                            {
                                if (t.CheckOut == default || t.CheckOut.IsNullOrEmpty())
                                    t.CheckOut = t.CheckIn;

                                t.CheckIn = "";
                            }
                        }
                        else if (t.CheckOut == default || t.CheckOut.IsNullOrEmpty())
                        {
                            t.CheckOut = t.CheckIn;
                            t.CheckIn = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Excute function ChangeCheckInCheckOutTimeIfCheckOutIsEmpty error: ", ex.Message));
            }
        }

        public void ChangeCheckInCheckOutTimeIfCheckOutIsEmptyCaseOffAfternoon(Timekeeping t)
        {
            try
            {
                string enableChangeTimeCaseOffAfternoon = SettingManager.GetSettingValueForApplication(AppSettingNames.TimeStartChangingCheckinToCheckoutEnable);
                if (enableChangeTimeCaseOffAfternoon != "true")
                {
                    return;
                }
                if (!string.IsNullOrEmpty(t.CheckIn))
                {
                    int checkInMinutes = DateTimeUtils.ConvertHHmmssToMinutes(t.CheckIn);
                    int timeStartChangingCheckInToCheckOutCaseOffAfternoonMinutes = DateTimeUtils.ConvertHHmmssToMinutes(SettingManager.GetSettingValue(AppSettingNames.TimeStartChangingCheckinToCheckoutCaseOffAfternoon));
                    if (checkInMinutes >= timeStartChangingCheckInToCheckOutCaseOffAfternoonMinutes)
                    {
                        if (!string.IsNullOrEmpty(t.RegisterCheckIn))
                        {
                            int registerCheckInTime = DateTimeUtils.ConvertHHmmssToMinutes(t.RegisterCheckIn);
                            int registerCheckOutTime = DateTimeUtils.ConvertHHmmssToMinutes(t.RegisterCheckOut);
                            int regiterCheckInTimeOffAfternoon = DateTimeUtils.ConvertHHmmssToMinutes("12:00");
                            if (registerCheckInTime > 0
                                && checkInMinutes > registerCheckInTime
                                && registerCheckOutTime <= regiterCheckInTimeOffAfternoon)
                            {
                                if (t.CheckOut == default || t.CheckOut.IsNullOrEmpty())
                                    t.CheckOut = t.CheckIn;

                                t.CheckIn = "";
                            }
                        }
                        else if (t.CheckOut == default || t.CheckOut.IsNullOrEmpty())
                        {
                            t.CheckOut = t.CheckIn;
                            t.CheckIn = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Excute function ChangeCheckInCheckOutTimeIfCheckOutIsEmptyCaseOffAfternoon error: ", ex.Message));
            }
        }
        public async System.Threading.Tasks.Task CheckIsPunished(Timekeeping timekeeping, int LimitedMinute)
        {
            timekeeping.IsPunishedCheckIn = (!String.IsNullOrEmpty(timekeeping.RegisterCheckIn) && String.IsNullOrEmpty(timekeeping.CheckIn)) || CommonUtils.SubtractHHmm(timekeeping.CheckIn, timekeeping.RegisterCheckIn) > LimitedMinute;
            timekeeping.IsPunishedCheckOut = (!String.IsNullOrEmpty(timekeeping.RegisterCheckOut) && String.IsNullOrEmpty(timekeeping.CheckOut)) || CommonUtils.SubtractHHmm(timekeeping.RegisterCheckOut, timekeeping.CheckOut) > LimitedMinute;
        }

        public async System.Threading.Tasks.Task CheckIsPunished(Timekeeping timekeeping)
        {
            var LimitedMinute = Int32.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));
            await CheckIsPunished(timekeeping, LimitedMinute);
        }
        public async System.Threading.Tasks.Task CheckIsPunishedByRule(Timekeeping timekeeping, int limitedMinute, float trackerTime)
        {
            var registerWorkingHours = CommonUtils.GetEmployeeWorkingHours(timekeeping.RegisterCheckOut, timekeeping.RegisterCheckIn);
            var NoCheckInAndNoCheckOut = (String.IsNullOrEmpty(timekeeping.CheckOut) && String.IsNullOrEmpty(timekeeping.CheckIn));
            var NoCheckOut = (String.IsNullOrEmpty(timekeeping.CheckOut));
            var CheckInLate = (!String.IsNullOrEmpty(timekeeping.CheckIn)) && CommonUtils.SubtractHHmm(timekeeping.CheckIn, timekeeping.RegisterCheckIn) > limitedMinute;
            var CheckIn = (!String.IsNullOrEmpty(timekeeping.CheckIn)) && CommonUtils.SubtractHHmm(timekeeping.CheckIn, timekeeping.RegisterCheckIn) <= limitedMinute;
            var CheckOut = (!String.IsNullOrEmpty(timekeeping.CheckOut));
            var NoCheckIn = (String.IsNullOrEmpty(timekeeping.CheckIn));

            float percentageConfig = getPercentageConfig();
            var trackerTimeByRegisterWorkingHours = percentageConfig * registerWorkingHours;
            if (!timekeeping.NoteReply.IsNullOrEmpty() && timekeeping.NoteReply.Contains("Off fullday"))
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (!timekeeping.NoteReply.IsNullOrEmpty() && timekeeping.NoteReply.Contains("Onsite"))
            {
                if ((CheckIn && CheckOut) || (CheckIn && NoCheckOut) || (CheckInLate && NoCheckOut))
                {
                    timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                    timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
                }
                else if (NoCheckIn && CheckOut)
                {
                    timekeeping.StatusPunish = CheckInCheckOutPunishmentType.Late;
                    timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
                }
            }
            else if (NoCheckInAndNoCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckInAndNoCheckOut;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (NoCheckInAndNoCheckOut && trackerTime >= trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckIn;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (CheckInLate && NoCheckOut && trackerTime >= trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.Late;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (CheckInLate && timekeeping.CheckOut.HasValue())
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.Late;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (CheckInLate && NoCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.LateAndNoCheckOut;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (CheckIn && NoCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckOut;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (CheckIn && CheckOut)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (CheckIn && !timekeeping.CheckOut.HasValue() && trackerTime >= trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
            else if (NoCheckIn)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckIn;
                timekeeping.MoneyPunish = await GetMoneyPunishByType(timekeeping.StatusPunish);
            }
        }

        private async Task<int> GetMoneyPunishByType(CheckInCheckOutPunishmentType StatusPunish)
        {
            var checkInCheckOutPunishmentSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInCheckOutPunishmentSetting);
            var rs = JsonConvert.DeserializeObject<List<CheckInCheckOutPunishmentSettingDto>>(checkInCheckOutPunishmentSetting);
            return rs.Where(x => x.Id == StatusPunish).Select(x => x.Money).FirstOrDefault();
        }

        public CheckInOutTimeDto CaculateCheckInOutTime(Dictionary<long, MapAbsenceUserDto> mapAbsenceUsers, TimesheetUserDto user)
        {
            var t = new CheckInOutTimeDto { };
            t.CheckIn = user.MorningStartAt;
            t.CheckOut = user.AfternoonEndAt;
            if (user.IsStopWork)
            {
                t.CheckIn = "";
                t.CheckOut = "";
                t.Note = "Stoped working";
                return t;
            }
            if (mapAbsenceUsers.ContainsKey(user.UserId))
            {//leave request
                var absenceUser = mapAbsenceUsers[user.UserId];
                if (absenceUser.DateType == DayType.Fullday)
                {
                    t.CheckIn = "";
                    t.CheckOut = "";
                    t.Note = "Off fullday";
                }
                else if (absenceUser.DateType == DayType.Morning)
                {
                    t.CheckIn = user.AfternoonStartAt;
                    t.Note = "Off morning";
                }
                else if (absenceUser.DateType == DayType.Afternoon)
                {
                    t.CheckOut = user.MorningEndAt;
                    t.Note = "Off afternoon";
                }
                else if (absenceUser.DateType == DayType.Custom)
                {
                    if (absenceUser.AbsenceTime == OnDayType.DiMuon)
                    {
                        if (absenceUser.Hour < user.MorningWorking)
                        {
                            t.CheckIn = CommonUtils.AddMoreHourToHHmm(user.MorningStartAt, absenceUser.Hour);
                        }
                        else
                        {
                            t.CheckIn = CommonUtils.AddMoreHourToHHmm(user.AfternoonStartAt, absenceUser.Hour - user.MorningWorking.Value);
                        }
                        t.Note = "Xin đến muộn " + absenceUser.Hour + " h";
                    }
                    else if (absenceUser.AbsenceTime == OnDayType.VeSom)
                    {
                        if (absenceUser.Hour < user.AfternoonWorking)
                        {
                            t.CheckOut = CommonUtils.AddMoreHourToHHmm(user.AfternoonEndAt, -absenceUser.Hour);
                        }
                        else
                        {
                            t.CheckOut = CommonUtils.AddMoreHourToHHmm(user.MorningEndAt, user.AfternoonWorking.Value - absenceUser.Hour);
                        }
                        t.Note = "Xin về sớm " + absenceUser.Hour + " h";
                    }
                }
            }
            return t;
        }
        public CheckInOutTimeDto CaculateCheckInOutTimeNew(Dictionary<long, List<MapAbsenceUserDto>> mapAbsenceUsers, TimesheetUserDto user)
        {
            var t = new CheckInOutTimeDto { };
            t.CheckIn = user.MorningStartAt;
            t.CheckOut = user.AfternoonEndAt;
            if (user.IsStopWork)
            {
                t.CheckIn = "";
                t.CheckOut = "";
                t.Note = "Stoped working";
                return t;
            }
            if (mapAbsenceUsers.ContainsKey(user.UserId))
            {//leave request
                foreach (var absenceUser in mapAbsenceUsers[user.UserId])
                {
                    var note = string.IsNullOrEmpty(t.Note) ? "" : t.Note;
                    t.Note = note == "" ? "" : t.Note + "\n";
                    if (absenceUser.Type == RequestType.Off)
                    {
                        if (absenceUser.DateType == DayType.Fullday)
                        {
                            t.CheckIn = "";
                            t.CheckOut = "";
                            t.Note += "Off fullday";
                            t.AbsenceDayType = DayType.Fullday;
                        }
                        else if (absenceUser.DateType == DayType.Morning)
                        {
                            t.CheckIn = user.AfternoonStartAt;
                            t.Note += "Off morning";
                            t.AbsenceDayType = DayType.Morning;
                        }
                        else if (absenceUser.DateType == DayType.Afternoon)
                        {
                            t.CheckOut = user.MorningEndAt;
                            t.Note += "Off afternoon";
                            t.AbsenceDayType = DayType.Afternoon;
                        }
                        else if (absenceUser.DateType == DayType.Custom)
                        {
                            if (absenceUser.AbsenceTime == OnDayType.DiMuon)
                            {
                                t.CheckIn = CommonUtils.AddMoreHourToHHmm(t.CheckIn, absenceUser.Hour);
                                t.Note += "Xin đến muộn " + absenceUser.Hour + " h";
                                t.AbsenceDayType = DayType.Custom;
                            }
                            else if (absenceUser.AbsenceTime == OnDayType.VeSom)
                            {
                                t.CheckOut = CommonUtils.AddMoreHourToHHmm(t.CheckOut, -(absenceUser.Hour));
                                t.Note += "Xin về sớm " + absenceUser.Hour + " h";
                                t.AbsenceDayType = DayType.Custom;
                            }
                        }
                    }
                    else if (absenceUser.Type == RequestType.Onsite)
                    {
                        if (absenceUser.DateType == DayType.Fullday)
                        {
                            t.CheckIn = "";
                            t.CheckOut = "";
                            t.Note += "Onsite fullday";
                            t.AbsenceDayType = DayType.Fullday;
                        }
                        else if (absenceUser.DateType == DayType.Morning)
                        {
                            t.CheckIn = user.AfternoonStartAt;
                            t.Note += "Onsite morning";
                            t.AbsenceDayType = DayType.Morning;
                        }
                        else if (absenceUser.DateType == DayType.Afternoon)
                        {
                            t.CheckOut = user.MorningEndAt;
                            t.Note += "Onsite afternoon";
                            t.AbsenceDayType = DayType.Afternoon;
                        }
                    }
                }
            }
            return t;
        }

        public async Task<object> NoticePunishUserCheckInOut(DateTime date)
        {
            var isTheDateOff = WorkScope.GetAll<DayOffSetting>()
                .Any(s => s.DayOff.Date == date.Date);

            if (isTheDateOff)
            {
                Logger.Info($"NoticePunishUserCheckInOut() {DateTimeUtils.ToString(date)} is Off Date => stop");
                return null;
            }

            string channelId = SettingManager.GetSettingValueForApplication(AppSettingNames.NofityKomuCheckInOutPunishmentToChannelId);
            if (string.IsNullOrEmpty(channelId))
            {
                Logger.Info($"NoticePunishUserCheckInOut() channelId is empty => stop");
                return null;
            }

            var listUserPunish = GetUserCheckInOutInfo(date)
                .Where(s => s.IsNoCheckInAndNoCheckOut || s.IsNoCheckInOrNoCheckOut)
                .ToList();

            if (listUserPunish.IsEmpty())
            {
                _komuService.NotifyToChannel("**KHÔNG** có nhân viên nào bị phạt quên check in, check out", channelId);
                return "KHÔNG có nhân viên nào bị phạt quên check in, check out";
            }

            var punishResult = GetPunishCheckInOutResult(listUserPunish, date);

            await _komuService.NotifyToChannelAwait(punishResult.List100k.ToArray(), channelId);

            await _komuService.NotifyToChannelAwait(punishResult.List50k.ToArray(), channelId);

            return punishResult;
        }

        private string SAODO_KOMU_ID = "922312910397653003";

        private List<UserCheckInOutInfoDto> GetUserCheckInOutInfo(DateTime dateAt)
        {
            var offFullDayUserIds = WorkScope.GetAll<AbsenceDayDetail>()
                .Select(s => new { s.DateAt, s.Request.Status, s.Request.Type, s.DateType, s.Request.UserId })
                .Where(s => s.DateAt.Date == dateAt.Date)
                .Where(s => s.Status == RequestStatus.Approved)
                .Where(s => s.Type == RequestType.Off)
                .Where(s => s.DateType == DayType.Fullday)
                .Select(s => s.UserId)
                .ToList();

            var workingUserIds = WorkScope.GetAll<User>()
                .Where(s => !s.IsStopWork || (s.IsStopWork && s.EndDateAt > dateAt.Date))
                .Select(s => s.Id)
                .ToList();

            var listUserCheckInOut = WorkScope.GetAll<Timekeeping>()
                .Where(s => s.UserId.HasValue)
                .Select(s => new { UserId = s.UserId.Value, DateAt = s.DateAt.Date, s.CheckIn, s.CheckOut, s.UserEmail })
                .WhereIf(!offFullDayUserIds.IsNullOrEmpty(), s => !offFullDayUserIds.Contains(s.UserId))
                .Where(s => workingUserIds.Contains(s.UserId))
                .Where(s => s.DateAt == dateAt.Date)
                .Where(s => s.CheckIn == null || s.CheckOut == null || s.CheckIn == "" || s.CheckOut == "")
                .Select(s => new UserCheckInOutInfoDto
                {
                    CheckIn = s.CheckIn,
                    CheckOut = s.CheckOut,
                    EmailAddress = s.UserEmail,
                    UserId = s.UserId,
                })
                .ToList();

            return listUserCheckInOut;
        }

        private PunishCheckInOutResult GetPunishCheckInOutResult(List<UserCheckInOutInfoDto> listUserCheckInOutInfo, DateTime date)
        {
            var result = new PunishCheckInOutResult
            {
                List100k = new List<string>(),
                List50k = new List<string>(),
            };

            if (listUserCheckInOutInfo == null || listUserCheckInOutInfo.IsEmpty())
            {
                return result;
            }
            var listUserName = listUserCheckInOutInfo.Select(x => x.UserName).ToList();

            var userTrackerTimes = _trackerService.GetTimeTrackerToDay(date, listUserName);

            var dicUserNameToTrackerTime = userTrackerTimes.ToDictionary(s => s.email, s => new { s.ActiveMinute, s.active_time });

            var dicUserNameToRegisterWorkingMinute = GetDicUserNameToWorkingMinute(date, listUserName);

            float percentageConfig = getPercentageConfig();

            listUserCheckInOutInfo.ForEach(item =>
            {
                var registerMinute = dicUserNameToRegisterWorkingMinute.ContainsKey(item.UserName) ? dicUserNameToRegisterWorkingMinute[item.UserName] : 8 * 60;
                var trackerInfo = dicUserNameToTrackerTime[item.UserName];
                var trackerMinute = trackerInfo?.ActiveMinute ?? 0;
                var minMinute = percentageConfig * registerMinute;

                if (item.IsNoCheckInAndNoCheckOut)
                {
                    var message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                        $"- no check in and no check out - not enough tracker time (" +
                        $"tracker time: {trackerInfo.active_time} < " +
                        $"{percentageConfig * 100}% * {DateTimeUtils.ConvertMinuteToHour(registerMinute)}h)";

                    if (trackerMinute < minMinute)
                    {
                        if (trackerInfo.ActiveMinute <= 0)
                        {
                            message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                            $"- no check in and no check out - no tracker time";

                            result.List100k.Add(message);
                        }
                        else
                        {
                            result.List100k.Add(message);
                        }
                    }
                    else
                    {
                        message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                        $"- no check in and no check out - enough tracker time (" +
                        $"tracker time: {trackerInfo.active_time} >= " +
                        $"{percentageConfig * 100}% * {DateTimeUtils.ConvertMinuteToHour(registerMinute)}h)";

                        result.List50k.Add(message);
                    }
                }
                else if (item.IsNoCheckOut)
                {
                    if (trackerMinute < minMinute)
                    {
                        if (trackerInfo.ActiveMinute <= 0)
                        {
                            var message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                                $"- no check out - no tracker time";

                            result.List50k.Add(message);
                        }
                        else
                        {
                            var message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                                $"- no check out - not enough tracker time (" +
                                $"tracker time: {trackerInfo.active_time} < " +
                                $"{percentageConfig * 100}% * {DateTimeUtils.ConvertMinuteToHour(registerMinute)}h)";
                            result.List50k.Add(message);
                        }
                    }
                }
            });

            result.List100k.Insert(0, $"<@&{SAODO_KOMU_ID}>: **{result.List100k.Count}** nhân viên bị phạt **100K** lỗi check in check out " +
            $"ngày {date.ToString("dd/MM/yyyy")}: ");

            result.List100k.Add("\n---------------------------------");

            result.List50k.Insert(0, $"<@&{SAODO_KOMU_ID}>: **{result.List50k.Count}** nhân viên bị phạt **50K** lỗi check in check out " +
            $"ngày {date.ToString("dd/MM/yyyy")}: ");

            result.List50k.Add("\n---------------------------------");

            return result;
        }

        private float getPercentageConfig()
        {
            var strPercentOfTrackerOnWorking = SettingManager.GetSettingValueForApplication(AppSettingNames.PercentOfTrackerOnWorking);
            if (string.IsNullOrEmpty(strPercentOfTrackerOnWorking))
            {
                return 0.9f;
            }
            try
            {
                return float.Parse(strPercentOfTrackerOnWorking) / 100;
            }
            catch
            {
                return 0.9f;
            }
        }

        private Dictionary<string, double> GetDicUserNameToWorkingMinute(DateTime dateAt, List<string> userNames)
        {
            var UserOffHours = WorkScope.GetAll<AbsenceDayDetail>()
                .Select(s => new { s.Request.User.UserName, s.DateAt.Date, s.Request.Status, s.Request.Type, s.Hour })
                .Where(s => userNames.Contains(s.UserName))
                .Where(s => s.Date == dateAt.Date)
                .Where(s => s.Status == RequestStatus.Approved)
                .Where(s => s.Type == RequestType.Off)
                .Select(s => new
                {
                    s.UserName,
                    s.Hour
                })
                .AsNoTracking()
                .ToList();

            return UserOffHours
                .GroupBy(s => s.UserName)
                .ToDictionary(s => s.Key, s => 60 * (8 - s.Sum(x => x.Hour)));
        }
    }
}