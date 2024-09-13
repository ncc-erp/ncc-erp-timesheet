using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
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

        public TimekeepingServices(KomuService komuService, TrackerService trackerService, IWorkScope workScope, FaceIdService faceIdService) : base(workScope)
        {
            _komuService = komuService;
            _trackerService = trackerService;
            _faceIdService = faceIdService;
        }

        [UnitOfWork]
        public async Task<List<Timekeeping>> AddTimekeepingByDay(DateTime selectedDate)
        {
            // check ngày nghỉ thì ko cần tạo Timekeeping
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

            var mapAbsenceUsers = WorkScope.GetAll<AbsenceDayDetail>().Include(s => s.Request)
                .Where(s => s.DateAt.Date == selectedDate.Date
                && s.Request.Status == RequestStatus.Approved)
                .GroupBy(s => s.Request.UserId)
                .ToDictionary(s => s.Key, s => s.Select(x => new MapAbsenceUserDto
                {
                    UserId = x.Request.UserId,
                    DateType = x.DateType,//morning, afternoon, fullday, custom
                    AbsenceTime = x.AbsenceTime,//dau. giua, cuoi
                    Hour = x.Hour,
                    Type = x.Request.Type,
                })
                                .OrderBy(x => x.AbsenceTime)
                                .ThenBy(x => x.Type)
                                .ToList());

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

            foreach (var user in users)
            {
                var t = new Timekeeping { };

                // Tính toán register check in out với leave
                //var RegisterCheckInOut = CaculateCheckInOutTime(mapAbsenceUsers, user);
                var registerCheckInOut = CaculateCheckInOutTimeNew(mapAbsenceUsers, user);
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
                    var dailyUser = mapDailyUsers[user.UserName];

                    t.CountPunishDaily = dailyUser;
                }

                if (mapMentionUsers.ContainsKey(user.UserName))
                {
                    var mentionUser = mapMentionUsers[user.UserName];

                    t.CountPunishMention = mentionUser;
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
                await CheckIsPunishedByRule(t, LimitedMinute, trackerTime, registerCheckInOut);
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
                }
                catch (Exception e)
                {
                    Logger.Error($"INSERT DATA ISSUE email: {t.User.EmailAddress} Error: {e.Message}");
                }
            }

            // co trong checkIn nhung ko co trong user
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
                    Logger.Error($"INSERT DATA ISSUE email: {t.User.EmailAddress} Error: {e.Message}");
                }
            }

            return rs;
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
        public async System.Threading.Tasks.Task CheckIsPunishedByRule(Timekeeping timekeeping, int limitedMinute, float trackerTime, CheckInOutTimeDto checkInOutTimeDto)
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
            }
            else if (!timekeeping.NoteReply.IsNullOrEmpty() && timekeeping.NoteReply.Contains("Onsite"))
            {
                if ((CheckIn && CheckOut) || (CheckIn && NoCheckOut) || (CheckInLate && NoCheckOut))
                {
                    timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                }
                else if (NoCheckIn && CheckOut)
                {
                    timekeeping.StatusPunish = CheckInCheckOutPunishmentType.Late;
                }
            }
            else if (NoCheckInAndNoCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckInAndNoCheckOut;
            }
            else if (NoCheckInAndNoCheckOut && trackerTime >= trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckIn;
            }
            else if (CheckInLate && NoCheckOut && trackerTime >= trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.Late;
            }
            else if (CheckInLate && timekeeping.CheckOut.HasValue())
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.Late;
            }
            else if (CheckInLate && NoCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.LateAndNoCheckOut;
            }
            else if (CheckIn && NoCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckOut;
            }
            else if (CheckIn && CheckOut)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
            }
            else if (CheckIn && !timekeeping.CheckOut.HasValue() && trackerTime >= trackerTimeByRegisterWorkingHours)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
            }
            else if (NoCheckIn)
            {
                timekeeping.StatusPunish = CheckInCheckOutPunishmentType.NoCheckIn;
            }
            await SetMoneyPunishByType(timekeeping, trackerTime, checkInOutTimeDto);

        }
        private async Task SetMoneyPunishByType(Timekeeping timekeeping, float trackerTime, CheckInOutTimeDto checkInOutTimeDto)
        {
            var checkInCheckOutPunishmentSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInCheckOutPunishmentSetting);
            var rs = JsonConvert.DeserializeObject<List<CheckInCheckOutPunishmentSettingDto>>(checkInCheckOutPunishmentSetting);
            var MoneyPunish = rs.Where(x => x.Id == timekeeping.StatusPunish).Select(x => x.Money).FirstOrDefault();
            if (timekeeping.CountPunishDaily > 0) MoneyPunish += rs.Where(x => x.Id == CheckInCheckOutPunishmentType.NoDaily).Select(x => x.Money).FirstOrDefault() * timekeeping.CountPunishDaily;
            if (timekeeping.CountPunishMention > 0) MoneyPunish += rs.Where(x => x.Id == CheckInCheckOutPunishmentType.NoReplyMention).Select(x => x.Money).FirstOrDefault() * timekeeping.CountPunishMention;

            if (checkInOutTimeDto != null && checkInOutTimeDto.isRemote)
            {
                if (trackerTime <= checkInOutTimeDto.TrackerTimeLevel[0])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime0).Select(x => x.Money).FirstOrDefault();
                }
                else if (trackerTime <= checkInOutTimeDto.TrackerTimeLevel[1])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime1).Select(x => x.Money).FirstOrDefault();
                }
                else if (trackerTime <= checkInOutTimeDto.TrackerTimeLevel[2])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime2).Select(x => x.Money).FirstOrDefault();
                }
                else if (trackerTime <= checkInOutTimeDto.TrackerTimeLevel[3])
                {
                    timekeeping.MoneyPunish = rs.Where(x => x.Id == CheckInCheckOutPunishmentType.TrackerTime3).Select(x => x.Money).FirstOrDefault();
                }
                else timekeeping.MoneyPunish = 0;
            }
            else timekeeping.MoneyPunish = 0;

            timekeeping.MoneyPunish += MoneyPunish;
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
            t.isRemote = false;
            // Mốc phạt KomuTracker: 0-2h, 2-4h, 4-6h, 6-7h
            t.TrackerTimeLevel = new List<float>() { 2 * 60, 4 * 60, 6 * 60, 7 * 60 };
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
                            // Mốc phạt KomuTracker: 0-2h, 2-3h, 3-4h, 4 -> (7 - Thời gian DM/VS)h
                            t.TrackerTimeLevel = new List<float>(){2 * 60, 3 * 60, 4 * 60, (7 - (float)absenceUser.Hour) * 60};
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
                    } else if (absenceUser.Type == RequestType.Remote)
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
