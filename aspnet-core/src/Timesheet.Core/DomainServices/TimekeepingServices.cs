using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Services.FaceIdService;
using Timesheet.Services.Komu;
using Timesheet.Services.Komu.Dto;
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
        private readonly FaceIdService _faceIdService;
        private readonly ProjectService _projectService;
        private readonly ISettingManager _settingManager;

        public TimekeepingServices(
            KomuService komuService,
            IWorkScope workScope,
            FaceIdService faceIdService,
            ProjectService projectService,
            ISettingManager settingManager)
            : base(workScope)
        {
            _komuService = komuService;
            _faceIdService = faceIdService;
            _projectService = projectService;
            _settingManager = settingManager;
        }

        private async System.Threading.Tasks.Task EnsureNotOffDate(DateTime selectedDate)
        {
            var isOffDate = await WorkScope.GetAll<DayOffSetting>()
                .AnyAsync(s => s.DayOff.Date == selectedDate.Date);

            if (isOffDate)
                throw new UserFriendlyException($"{selectedDate:MM/dd/yyyy HH:mm:ss} is Off Date => stop");
        }

        private async Task<List<TimesheetUserDto>> GetWorkingUsers(DateTime selectedDate)
        {
            var users = await WorkScope.GetAll<User>()
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
                    Type = u.Type,
                }).ToListAsync();

            if (!users.Any())
                throw new UserFriendlyException($"No working users found on {selectedDate:yyyy-MM-dd}");

            return users;
        }

        private async Task<Dictionary<long, List<(string NoteReply, string UserNote)>>> SoftDeleteOldTimekeeping(DateTime selectedDate)
        {
            var now = DateTimeUtils.GetNow();
            var olds = await WorkScope.GetAll<Timekeeping>()
                .Where(s => s.DateAt.Date == selectedDate.Date)
                .ToListAsync();

            var notes = olds
                .GroupBy(s => s.UserId ?? 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (x.NoteReply, x.UserNote)).ToList()
                );

            olds.ForEach(s => { s.IsDeleted = true; s.DeletionTime = now; });

            await CurrentUnitOfWork.SaveChangesAsync();

            return notes;
        }

        private async System.Threading.Tasks.Task SoftDeleteOldPunishments(DateTime selectedDate)
        {
            var now = DateTimeUtils.GetNow();
            var typesToDelete = new List<UserPunishmentType>
            {
                UserPunishmentType.Late,
                UserPunishmentType.NoCheckIn,
                UserPunishmentType.NoCheckOut,
                UserPunishmentType.LateAndNoCheckOut,
                UserPunishmentType.NoCheckInAndNoCheckOut,
                UserPunishmentType.Daily,
                UserPunishmentType.Mention,
                UserPunishmentType.Tracker_20k,
                UserPunishmentType.Tracker_50k,
                UserPunishmentType.Tracker_100k,
                UserPunishmentType.Tracker_200k,
            };

            var olds = await WorkScope.GetAll<UserPunishment>()
                .Where(p => p.DateAt.Date == selectedDate.Date && typesToDelete.Contains(p.Type))
                .ToListAsync();

            if (olds.Count > 0)
            {
                olds.ForEach(p => { p.IsDeleted = true; p.DeletionTime = now; });
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        private async Task<(Dictionary<long, List<MapAbsenceUserDto>> AbsenceUsers, Dictionary<long, List<MapAbsenceUserDto>> RemoteUsers)> GetAbsenceAndRemoteUsers(DateTime selectedDate)
        {
            var mapAbsenceUsers = new Dictionary<long, List<MapAbsenceUserDto>>();
            var mapRemoteUsers = new Dictionary<long, List<MapAbsenceUserDto>>();

            var absenceDayDetails = await WorkScope.GetAll<AbsenceDayDetail>()
                .Include(s => s.Request)
                .Where(s => s.DateAt.Date == selectedDate.Date
                    && s.Request.Status == RequestStatus.Approved)
                .Select(s => new MapAbsenceUserDto
                {
                    UserId = s.Request.UserId,
                    DateType = s.DateType,
                    AbsenceTime = s.AbsenceTime,
                    Hour = s.Hour,
                    Type = s.Request.Type
                })
                .ToListAsync();


            var nonRemoteRecords = absenceDayDetails
                .Where(r => r.Type != RequestType.Remote)
                .GroupBy(r => r.UserId);

            var remoteRecords = absenceDayDetails
                .Where(r => r.Type == RequestType.Remote)
                .GroupBy(r => r.UserId);

            foreach (var group in nonRemoteRecords)
            {
                mapAbsenceUsers[group.Key] = group.OrderBy(x => x.AbsenceTime).ToList();
            }

            foreach (var group in remoteRecords)
            {
                mapRemoteUsers[group.Key] = group.OrderBy(x => x.AbsenceTime).ToList();
            }

            return (mapAbsenceUsers, mapRemoteUsers);
        }

        private async Task<(Dictionary<string, UserCheckInDto> mapCheckInUsers,
            Dictionary<string, int> mapDailyUsers,
            Dictionary<string, int> mapMentionUsers,
            Dictionary<string, int> mapWFHUsers,
            Dictionary<string, (float SpentMinute, string spent_time)> dicUserNameToTracker,
            Dictionary<UserPunishmentType, PunishmentSystem> punishmentSystems)> LoadExternalData(DateTime selectedDate, List<TimesheetUserDto> users)
        {
            var checkInUsers = _faceIdService.GetEmployeeCheckInOutMini(selectedDate)
                ?? throw new UserFriendlyException("Missing checkIn data");

            var dailyAndMentionAndTracker = _komuService.GetDailyReport(selectedDate)
                ?? throw new UserFriendlyException("Missing komu data");

            var userNames = users.Select(x => x.UserName).Distinct().ToList();

            var punishmentTypes = Enum.GetValues(typeof(UserPunishmentType))
                .Cast<UserPunishmentType>()
                .ToArray();

            var punishmentSystems = await WorkScope.GetAll<PunishmentSystem>()
                .Where(x => punishmentTypes.Contains(x.Type))
                .ToDictionaryAsync(x => x.Type, x => x);

            var mapCheckInUsers = checkInUsers.ToDictionary(s => s.Email);
            var mapDailyUsers = dailyAndMentionAndTracker.daily.ToDictionary(s => s.email, s => s.count);
            var mapMentionUsers = dailyAndMentionAndTracker.mention.ToDictionary(s => s.name, s => s.count);
            var mapWFHUsers = dailyAndMentionAndTracker.wfh.ToDictionary(s => s.name, s => s.total);

            var processedTracker = dailyAndMentionAndTracker.tracker
                .GroupBy(t => t.email)
                .Select(g =>
                {
                    var best = g
                        .OrderByDescending(t => t.SpentMinute).First();
                    return new TrackerDto
                    {
                        email = g.Key,
                        spent_time = best.spent_time
                    };
                })
                .ToList();
            var dicUserNameToTracker = processedTracker.ToDictionary(s => s.email, s => (s.SpentMinute, s.spent_time));

            var PunishmentSystems = punishmentSystems;
            return
            (
                mapCheckInUsers,
                mapDailyUsers,
                mapMentionUsers,
                mapWFHUsers,
                dicUserNameToTracker,
                punishmentSystems
            );
        }

        private async Task<(List<Timekeeping> rs, List<UserPunishment> userPunishmentsToInsert)> GenerateTimekeepingRecords(
            DateTime selectedDate,
            TimesheetUserDto user,
            Dictionary<string, UserCheckInDto> mapCheckInUsers,
            Dictionary<string, int> mapDailyUsers,
            Dictionary<string, int> mapMentionUsers,
            Dictionary<string, int> mapWFHUsers,
            Dictionary<string, (float SpentMinute, string spent_time)> dicUserNameToTracker,
            Dictionary<UserPunishmentType, PunishmentSystem> punishmentSystems,
            Dictionary<long, List<(string NoteReply, string UserNote)>> oldTimekeepingNotes,
            Dictionary<long, List<MapAbsenceUserDto>> mapAbsenceUsers,
            Dictionary<long, List<MapAbsenceUserDto>> mapRemoteUsers)
        {
            var LimitedMinute = Int32.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));
            var rs = new List<Timekeeping>();
            var userPunishmentsToInsert = new List<UserPunishment>();

            var t = new Timekeeping { };
            var combinedAbsenceList = new List<MapAbsenceUserDto>();
            var combinedMapAbsenceUsers = new Dictionary<long, List<MapAbsenceUserDto>>();

            // request off
            if (mapAbsenceUsers.ContainsKey(user.UserId))
            {
                combinedAbsenceList.AddRange(mapAbsenceUsers[user.UserId]);
            }

            // request remote
            if (mapRemoteUsers.ContainsKey(user.UserId))
            {
                combinedAbsenceList.AddRange(mapRemoteUsers[user.UserId]);
            }

            // combined
            if (combinedAbsenceList.Any())
            {
                combinedMapAbsenceUsers[user.UserId] = combinedAbsenceList.OrderBy(x => x.DateType).ToList();
            }

            var registerCheckInOut = CaculateCheckInOutTimeNew(combinedMapAbsenceUsers, user);
            bool isRemoteWork = mapRemoteUsers.ContainsKey(user.UserId);

            float trackerTime = dicUserNameToTracker.ContainsKey(user.UserName) ? dicUserNameToTracker[user.UserName].SpentMinute : 0;

            t.RegisterCheckIn = registerCheckInOut.CheckIn;
            t.RegisterCheckOut = registerCheckInOut.CheckOut;
            t.NoteReply = registerCheckInOut.Note;

            if (registerCheckInOut.Note != "Off fullday")
            {

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

                if (user.Type != Usertype.Vendor)
                {
                    if (mapDailyUsers.ContainsKey(user.UserName))
                    {
                        t.CountPunishDaily = mapDailyUsers[user.UserName];
                        var dailyPunishment = punishmentSystems[UserPunishmentType.Daily];
                        userPunishmentsToInsert.Add(new UserPunishment
                        {
                            DateAt = selectedDate,
                            UserId = user.UserId,
                            PunishmentSystemId = dailyPunishment.Id,
                            Type = dailyPunishment.Type,
                            Count = mapDailyUsers[user.UserName],
                            TotalMoney = mapDailyUsers[user.UserName] * dailyPunishment.Money
                        });
                    }

                    if (mapMentionUsers.ContainsKey(user.UserName))
                    {
                        t.CountPunishMention = mapMentionUsers[user.UserName];
                        var mentionPunishment = punishmentSystems[UserPunishmentType.Mention];
                        userPunishmentsToInsert.Add(new UserPunishment
                        {
                            DateAt = selectedDate,
                            UserId = user.UserId,
                            PunishmentSystemId = mentionPunishment.Id,
                            Type = mentionPunishment.Type,
                            Count = mapMentionUsers[user.UserName],
                            TotalMoney = mapMentionUsers[user.UserName] * mentionPunishment.Money,
                        });
                    }

                    if (mapWFHUsers.ContainsKey(user.UserName))
                    {
                        t.CountPunishMention += mapWFHUsers[user.UserName];
                        var mentionPunishment = punishmentSystems[UserPunishmentType.Mention];
                        var existingMention = userPunishmentsToInsert
                            .FirstOrDefault(p => p.UserId == user.UserId && p.Type == UserPunishmentType.Mention);

                        if (existingMention != null)
                        {
                            existingMention.Count += mapWFHUsers[user.UserName];
                            existingMention.TotalMoney = existingMention.Count * mentionPunishment.Money;
                        }
                        else
                        {
                            userPunishmentsToInsert.Add(new UserPunishment
                            {
                                DateAt = selectedDate,
                                UserId = user.UserId,
                                PunishmentSystemId = mentionPunishment.Id,
                                Type = mentionPunishment.Type,
                                Count = mapWFHUsers[user.UserName],
                                TotalMoney = mapWFHUsers[user.UserName] * mentionPunishment.Money,
                            });
                        }
                    }
                }
            }


            if (oldTimekeepingNotes.ContainsKey(user.UserId))
            {
                oldTimekeepingNotes[user.UserId].ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(item.NoteReply))
                    {
                        t.NoteReply = item.NoteReply;
                    }

                    if (!string.IsNullOrEmpty(item.UserNote))
                    {
                        t.UserNote = item.UserNote;
                    }
                });

            }


            t.UserEmail = user.EmailAddress;
            t.DateAt = selectedDate;
            t.UserId = user.UserId;

            if (user.Type != Usertype.Vendor)
            {
                await CheckIsPunished(t, LimitedMinute);
                await CheckIsPunishedByRule(t, LimitedMinute, trackerTime);
            }
            else
            {
                t.IsPunishedCheckIn = false;
                t.IsPunishedCheckOut = false;
                t.StatusPunish = CheckInCheckOutPunishmentType.NoPunish;
                t.MoneyPunish = 0;
            }

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

            t.TrackerTime = dicUserNameToTracker.ContainsKey(user.UserName) ? dicUserNameToTracker[user.UserName].spent_time : "0:00:00";
            try
            {
                rs.Add(t);

                if (t.StatusPunish != CheckInCheckOutPunishmentType.NoPunish && user.Type != Usertype.Vendor)
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

                if (isRemoteWork && user.Type != Usertype.Vendor)
                {
                    var registerWorkingMinutes = CommonUtils.GetEmployeeWorkingHours(t.RegisterCheckOut, t.RegisterCheckIn);
                    var dayOffType = registerCheckInOut.AbsenceDayType;
                    var trackerPunishment = await CreateTrackerTimePunishment(
                        selectedDate,
                        user.UserId,
                        trackerTime,
                        registerWorkingMinutes,
                        t.UserNote,
                        t.NoteReply,
                        dayOffType,
                        punishmentSystems);
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

            return (rs, userPunishmentsToInsert);
        }

        [UnitOfWork(TransactionScopeOption.RequiresNew)]
        public async Task<List<Timekeeping>> AddTimekeepingByDay(DateTime selectedDate)
        {
            var allTimekeepings = new List<Timekeeping>();
            var allPunishments = new List<UserPunishment>();

            var start = DateTime.Now;
            var now = DateTimeUtils.GetNow();
            var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew);
            try
            {
                await EnsureNotOffDate(selectedDate);

                var users = await GetWorkingUsers(selectedDate);

                var oldTimekeepingNotes = await SoftDeleteOldTimekeeping(selectedDate);
                await SoftDeleteOldPunishments(selectedDate);

                var (mapAbsenceUsers, mapRemoteUsers) = await GetAbsenceAndRemoteUsers(selectedDate);
                var (mapCheckInUsers, mapDailyUsers, mapMentionUsers, mapWFHUsers, dicUserNameToTracker, punishmentSystems) = await LoadExternalData(selectedDate, users);


                int batchSize = 100;
                for (int i = 0; i < users.Count; i += batchSize)
                {
                    var batch = users.Skip(i).Take(batchSize).ToList();

                    try
                    {
                        Logger.Info($"Processing batch {i / batchSize + 1} with {batch.Count} users...");

                        var tasks = batch.Select(user =>
                            GenerateTimekeepingRecords(
                                selectedDate,
                                user,
                                mapCheckInUsers,
                                mapDailyUsers,
                                mapMentionUsers,
                                mapWFHUsers,
                                dicUserNameToTracker,
                                punishmentSystems,
                                oldTimekeepingNotes,
                                mapAbsenceUsers,
                                mapRemoteUsers
                            )
                        );

                        var result = await System.Threading.Tasks.Task.WhenAll(tasks);

                        allTimekeepings.AddRange(result.SelectMany(r => r.rs));
                        allPunishments.AddRange(result.SelectMany(r => r.userPunishmentsToInsert));


                        Logger.Info($"Finished batch {i / batchSize + 1}. " +
                                    $"Accumulated {allTimekeepings.Count} records so far.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"❌ Error in batch {i / batchSize + 1}, " +
                                     $"users {batch.First().UserId} → {batch.Last().UserId}. " +
                                     $"Error: {ex}");
                    }
                }

                // TODO: check again
                var userEmail = users.Select(u => u.EmailAddress).ToHashSet();
                var checkInUsersOnly = mapCheckInUsers.Values
                    .Where(u => !userEmail.Contains(u.Email))
                    .ToList();

                foreach (var checkIn in checkInUsersOnly)
                {
                    var t = new Timekeeping
                    {
                        UserEmail = checkIn.Email,
                        CheckIn = checkIn?.VerifyStartTimeStr,
                        CheckOut = checkIn?.VerifyEndTimeStr,
                        DateAt = selectedDate,
                        NoteReply = "Email not match",
                        TrackerTime = dicUserNameToTracker.ContainsKey(checkIn.Email.Split("@")[0]) ? dicUserNameToTracker[checkIn.Email.Split("@")[0]].spent_time : "0",
                    };
                    ChangeCheckInCheckOutTimeIfCheckOutIsEmpty(t);

                    try
                    {
                        allTimekeepings.Add(t);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"INSERT DATA ISSUE email: {t.User?.EmailAddress} Error: {e.Message}");
                    }
                }

                foreach (var punishment in allPunishments)
                {
                    punishment.IsPaid = false;
                }

                await WorkScope.InsertRangeAsync(allTimekeepings);
                await WorkScope.InsertRangeAsync(allPunishments);

                await CurrentUnitOfWork.SaveChangesAsync();

                var affectedUserIds = allPunishments
                    .Where(p => p.UserId > 0)
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToList();

                if (affectedUserIds.Any())
                {
                    var totalByUsers = await WorkScope.GetAll<UserPunishment>()
                        .Where(p => !p.IsDeleted && (p.IsPaid == false || p.IsPaid == null) && affectedUserIds.Contains(p.UserId))
                        .GroupBy(p => p.UserId)
                        .Select(g => new
                        {
                            UserId = g.Key,
                            Total = g.Sum(p => (long)p.TotalMoney)
                        })
                        .ToDictionaryAsync(x => x.UserId, x => x.Total);

                    var balanceRepo = WorkScope.GetRepo<UserPunishmentBalance>();
                    var existingBalances = await balanceRepo.GetAll()
                        .Where(b => affectedUserIds.Contains(b.UserId))
                        .ToListAsync();

                    var existingBalanceMap = existingBalances.ToDictionary(b => b.UserId);

                    foreach (var userId in affectedUserIds)
                    {
                        totalByUsers.TryGetValue(userId, out var totalUnpaidPunishment);

                        if (!existingBalanceMap.TryGetValue(userId, out var balance))
                        {
                            balance = new UserPunishmentBalance
                            {
                                UserId = userId,
                                TotalPunishmentMoney = (int)totalUnpaidPunishment,
                                RemainPoints = 0
                            };

                            await balanceRepo.InsertAsync(balance);
                        }
                        else
                        {
                            balance.TotalPunishmentMoney = (int)totalUnpaidPunishment;
                            await balanceRepo.UpdateAsync(balance);
                        }
                    }
                }

                await uow.CompleteAsync();
            }
            finally
            {
                uow.Dispose();
            }

            var elapsed = DateTime.Now - start;
            Console.WriteLine($"Thời gian chạy: {elapsed.TotalMilliseconds} ms");
            Logger.Info($"✅ Successfully processed timekeeping for date {selectedDate:yyyy-MM-dd}. " +
            $"Total {allTimekeepings.Count} records, {allPunishments.Count} punishments.");
            return allTimekeepings;
        }

        public async System.Threading.Tasks.Task SaveDailyAndMentionPunishments(DateTime selectedDate, List<TimesheetUserDto> users,
          Dictionary<string, int> mapDailyUsers, Dictionary<string, int> mapMentionUsers, Dictionary<string, int> mapWFHUsers, Dictionary<UserPunishmentType, PunishmentSystem> punishmentSystems)
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
                        TotalMoney = mapDailyUsers[user.UserName] * dailyPunishment.Money
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
                        TotalMoney = mapMentionUsers[user.UserName] * mentionPunishment.Money
                    });
                }

                if (mapWFHUsers.ContainsKey(user.UserName) && mapWFHUsers[user.UserName] > 0)
                {
                    var mentionPunishment = punishmentSystems[UserPunishmentType.Mention];
                    var existingMention = dailyMentionPunishments
                        .FirstOrDefault(p => p.UserId == user.UserId && p.Type == UserPunishmentType.Mention);

                    if (existingMention != null)
                    {
                        existingMention.Count += mapWFHUsers[user.UserName];
                        existingMention.TotalMoney = existingMention.Count * mentionPunishment.Money;
                    }
                    else
                    {
                        dailyMentionPunishments.Add(new UserPunishment
                        {
                            DateAt = selectedDate,
                            UserId = user.UserId,
                            PunishmentSystemId = mentionPunishment.Id,
                            Type = mentionPunishment.Type,
                            Count = mapWFHUsers[user.UserName],
                            TotalMoney = mapWFHUsers[user.UserName] * mentionPunishment.Money,
                        });
                    }
                }
            }

            if (dailyMentionPunishments.Any())
            {
                await WorkScope.InsertRangeAsync(dailyMentionPunishments);
            }
        }

        public async Task<UserPunishment> CreateTrackerTimePunishment(
            DateTime selectedDate,
            long userId,
            float trackerTime,
            double registerWorkingMinutes,
            string userNote,
            string noteReply,
            DayType? dayOffType,
            Dictionary<UserPunishmentType, PunishmentSystem> punishmentSystems)
        {
            var punishmentLevels = new List<(UserPunishmentType Type, double MinPercentage, double? MaxPercentage)> {
                (UserPunishmentType.Tracker_200k,
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker200kPunishment).Split('-')[0]),
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker200kPunishment).Split('-')[1])),
                (UserPunishmentType.Tracker_100k,
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker100kPunishment).Split('-')[0]),
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker100kPunishment).Split('-')[1])),
                (UserPunishmentType.Tracker_50k,
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker50kPunishment).Split('-')[0]),
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker50kPunishment).Split('-')[1])),
                (UserPunishmentType.Tracker_20k,
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker20kPunishment).Split('-')[0]),
                double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker20kPunishment).Split('-')[1]))
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
            var absenceInfo = ParseAbsenceInfo(timekeeping.NoteReply);

            if (absenceInfo.HasFullDayAbsence || absenceInfo.HasCombinedAbsence)
            {
                await SetPunishmentResult(timekeeping, CheckInCheckOutPunishmentType.NoPunish);
                return;
            }

            if (absenceInfo.HasOnsiteOnly && absenceInfo.HasOffCombination)
            {
                await SetPunishmentResult(timekeeping, CheckInCheckOutPunishmentType.NoPunish);
                return;
            }

            var punishmentType = DetermineStandardPunishment(timekeeping, limitedMinute, trackerTime);
            await SetPunishmentResult(timekeeping, punishmentType);
        }

        private AbsenceInfo ParseAbsenceInfo(string noteReply)
        {
            var info = new AbsenceInfo();
            if (string.IsNullOrEmpty(noteReply)) return info;

            info.HasOffMorning = noteReply.Contains("Off morning");
            info.HasOffAfternoon = noteReply.Contains("Off afternoon");
            info.HasOffFullDay = noteReply.Contains("Off fullday");
            info.HasOnsiteMorning = noteReply.Contains("Onsite morning");
            info.HasOnsiteAfternoon = noteReply.Contains("Onsite afternoon");
            info.HasOnsiteFullDay = noteReply.Contains("Onsite fullday");

            info.HasFullDayAbsence = info.HasOffFullDay || info.HasOnsiteFullDay;
            info.HasCombinedAbsence = (info.HasOffMorning && info.HasOnsiteAfternoon) ||
                                    (info.HasOnsiteMorning && info.HasOffAfternoon) ||
                                    (info.HasOffMorning && info.HasOffAfternoon) ||
                                    (info.HasOnsiteMorning && info.HasOnsiteAfternoon);
            info.HasOnsiteOnly = (info.HasOnsiteMorning || info.HasOnsiteAfternoon) && !info.HasOnsiteFullDay;
            info.HasOffCombination = (info.HasOnsiteMorning && info.HasOffAfternoon) || (info.HasOnsiteAfternoon && info.HasOffMorning);

            return info;
        }

        private CheckInCheckOutPunishmentType DetermineStandardPunishment(Timekeeping timekeeping, int limitedMinute, float trackerTime)
        {
            var registerWorkingHours = CommonUtils.GetEmployeeWorkingHours(timekeeping.RegisterCheckOut, timekeeping.RegisterCheckIn);
            float percentageConfig = getPercentageConfig();
            var trackerTimeByRegisterWorkingHours = percentageConfig * registerWorkingHours;

            var noCheckInAndNoCheckOut = string.IsNullOrEmpty(timekeeping.CheckOut) && string.IsNullOrEmpty(timekeeping.CheckIn);
            var noCheckOut = string.IsNullOrEmpty(timekeeping.CheckOut);
            var checkInLate = !string.IsNullOrEmpty(timekeeping.CheckIn) && CommonUtils.SubtractHHmm(timekeeping.CheckIn, timekeeping.RegisterCheckIn) > limitedMinute;
            var checkIn = !string.IsNullOrEmpty(timekeeping.CheckIn) && CommonUtils.SubtractHHmm(timekeeping.CheckIn, timekeeping.RegisterCheckIn) <= limitedMinute;
            var checkOut = !string.IsNullOrEmpty(timekeeping.CheckOut);
            var noCheckIn = string.IsNullOrEmpty(timekeeping.CheckIn);

            if (noCheckInAndNoCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
                return CheckInCheckOutPunishmentType.NoCheckInAndNoCheckOut;
            if (noCheckInAndNoCheckOut && trackerTime >= trackerTimeByRegisterWorkingHours)
                return CheckInCheckOutPunishmentType.NoCheckIn;
            if (checkInLate && noCheckOut && trackerTime >= trackerTimeByRegisterWorkingHours)
                return CheckInCheckOutPunishmentType.Late;
            if (checkInLate && timekeeping.CheckOut.HasValue())
                return CheckInCheckOutPunishmentType.Late;
            if (checkInLate && noCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
                return CheckInCheckOutPunishmentType.LateAndNoCheckOut;
            if (checkIn && noCheckOut && trackerTime < trackerTimeByRegisterWorkingHours)
                return CheckInCheckOutPunishmentType.NoCheckOut;
            if (checkIn && checkOut)
                return CheckInCheckOutPunishmentType.NoPunish;
            if (checkIn && !timekeeping.CheckOut.HasValue() && trackerTime >= trackerTimeByRegisterWorkingHours)
                return CheckInCheckOutPunishmentType.NoPunish;
            if (noCheckIn)
                return CheckInCheckOutPunishmentType.NoCheckIn;

            return CheckInCheckOutPunishmentType.NoPunish;
        }

        private async System.Threading.Tasks.Task SetPunishmentResult(Timekeeping timekeeping, CheckInCheckOutPunishmentType punishmentType)
        {
            timekeeping.StatusPunish = punishmentType;
            timekeeping.MoneyPunish = await GetMoneyPunishByType(punishmentType);
        }

        private class AbsenceInfo
        {
            public bool HasOffMorning { get; set; }
            public bool HasOffAfternoon { get; set; }
            public bool HasOffFullDay { get; set; }
            public bool HasOnsiteMorning { get; set; }
            public bool HasOnsiteAfternoon { get; set; }
            public bool HasOnsiteFullDay { get; set; }
            public bool HasFullDayAbsence { get; set; }
            public bool HasCombinedAbsence { get; set; }
            public bool HasOnsiteOnly { get; set; }
            public bool HasOffCombination { get; set; }
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
            {
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
            {
                var absenceList = mapAbsenceUsers[user.UserId];
                var notes = new List<string>();

                bool morningCovered = false;
                bool afternoonCovered = false;
                bool isFullDayAbsence = false;
                bool isLateRequest = false;
                double lateHours = 0;

                foreach (var absenceUser in absenceList)
                {
                    if (absenceUser.Type == RequestType.Off)
                    {
                        if (absenceUser.DateType == DayType.Fullday)
                        {
                            isFullDayAbsence = true;
                            morningCovered = true;
                            afternoonCovered = true;
                            notes.Add("Off fullday");
                            t.AbsenceDayType = DayType.Fullday;
                        }
                        else if (absenceUser.DateType == DayType.Morning)
                        {
                            morningCovered = true;
                            notes.Add("Off morning");
                            t.AbsenceDayType = DayType.Morning;
                        }
                        else if (absenceUser.DateType == DayType.Afternoon)
                        {
                            afternoonCovered = true;
                            notes.Add("Off afternoon");
                            t.AbsenceDayType = DayType.Afternoon;
                        }
                        else if (absenceUser.DateType == DayType.Custom)
                        {
                            if (absenceUser.AbsenceTime == OnDayType.DiMuon)
                            {
                                isLateRequest = true;
                                lateHours = absenceUser.Hour;
                                notes.Add("Xin đến muộn " + absenceUser.Hour + " h");
                                t.AbsenceDayType = DayType.Custom;
                            }
                            else if (absenceUser.AbsenceTime == OnDayType.VeSom)
                            {
                                t.CheckOut = CommonUtils.AddMoreHourToHHmm(t.CheckOut, -(absenceUser.Hour));
                                notes.Add("Xin về sớm " + absenceUser.Hour + " h");
                                t.AbsenceDayType = DayType.Custom;
                            }
                        }
                    }
                    else if (absenceUser.Type == RequestType.Onsite)
                    {
                        if (absenceUser.DateType == DayType.Fullday)
                        {
                            isFullDayAbsence = true;
                            morningCovered = true;
                            afternoonCovered = true;
                            notes.Add("Onsite fullday");
                            t.AbsenceDayType = DayType.Fullday;
                        }
                        else if (absenceUser.DateType == DayType.Morning)
                        {
                            morningCovered = true;
                            notes.Add("Onsite morning");
                            t.AbsenceDayType = DayType.Morning;
                        }
                        else if (absenceUser.DateType == DayType.Afternoon)
                        {
                            afternoonCovered = true;
                            notes.Add("Onsite afternoon");
                            t.AbsenceDayType = DayType.Afternoon;
                        }
                    }
                    else if (absenceUser.Type == RequestType.Remote)
                    {
                        if (absenceUser.DateType == DayType.Fullday)
                        {
                            notes.Add("Remote fullday");
                            t.AbsenceDayType = DayType.Fullday;
                        }
                        else if (absenceUser.DateType == DayType.Morning)
                        {
                            notes.Add("Remote morning");
                            t.AbsenceDayType = DayType.Morning;
                        }
                        else if (absenceUser.DateType == DayType.Afternoon)
                        {
                            notes.Add("Remote afternoon");
                            t.AbsenceDayType = DayType.Afternoon;
                        }
                    }
                }

                if (isFullDayAbsence)
                {
                    t.CheckIn = "";
                    t.CheckOut = "";
                }
                else if (morningCovered && afternoonCovered)
                {
                    t.CheckIn = "";
                    t.CheckOut = "";
                }
                else if (morningCovered && !afternoonCovered)
                {
                    t.CheckIn = user.AfternoonStartAt;
                    if (isLateRequest)
                    {
                        t.CheckIn = CommonUtils.AddMoreHourToHHmm(user.AfternoonStartAt, lateHours);
                    }
                }
                else if (!morningCovered && afternoonCovered)
                {
                    t.CheckOut = user.MorningEndAt;
                }
                else if (isLateRequest && !morningCovered)
                {
                    t.CheckIn = CommonUtils.AddMoreHourToHHmm(user.MorningStartAt, lateHours);
                }

                t.Note = string.Join("/", notes);
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
                .Where(s => s.Type != Usertype.Vendor && (!s.IsStopWork || s.EndDateAt > dateAt.Date))
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

            var userTrackerTimes = _komuService.GetDailyReport(date).tracker;

            var dicUserNameToTrackerTime = userTrackerTimes.ToDictionary(s => s.email, s => new { s.SpentMinute, s.spent_time });

            var dicUserNameToRegisterWorkingMinute = GetDicUserNameToWorkingMinute(date, listUserName);

            float percentageConfig = getPercentageConfig();

            listUserCheckInOutInfo.ForEach(item =>
            {
                var registerMinute = dicUserNameToRegisterWorkingMinute.ContainsKey(item.UserName) ? dicUserNameToRegisterWorkingMinute[item.UserName] : 8 * 60;
                var trackerInfo = dicUserNameToTrackerTime[item.UserName];
                var trackerMinute = trackerInfo?.SpentMinute ?? 0;
                var minMinute = percentageConfig * registerMinute;

                if (item.IsNoCheckInAndNoCheckOut)
                {
                    var message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                        $"- no check in and no check out - not enough tracker time (" +
                        $"tracker time: {trackerInfo.spent_time} < " +
                        $"{percentageConfig * 100}% * {DateTimeUtils.ConvertMinuteToHour(registerMinute)}h)";

                    if (trackerMinute < minMinute)
                    {
                        if (trackerInfo.SpentMinute <= 0)
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
                        $"tracker time: {trackerInfo.spent_time} >= " +
                        $"{percentageConfig * 100}% * {DateTimeUtils.ConvertMinuteToHour(registerMinute)}h)";

                        result.List50k.Add(message);
                    }
                }
                else if (item.IsNoCheckOut)
                {
                    if (trackerMinute < minMinute)
                    {
                        if (trackerInfo.SpentMinute <= 0)
                        {
                            var message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                                $"- no check out - no tracker time";

                            result.List50k.Add(message);
                        }
                        else
                        {
                            var message = $"{CommonUtils.GetDiscordTagUser(item.EmailAddress)} " +
                                $"- no check out - not enough tracker time (" +
                                $"tracker time: {trackerInfo.spent_time} < " +
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