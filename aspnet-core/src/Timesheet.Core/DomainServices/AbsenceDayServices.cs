using Abp.Linq.Extensions;
using Abp.Dependency;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Services.Mezon;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.Configuration;
using Ncc.Configuration;

namespace Timesheet.DomainServices
{
    public class AbsenceDayServices : IAbsenceDayServices, ITransientDependency
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private readonly ISettingManager _settingManager;
        private const int MESSAGE_DELAY_MS = 1000;
        public AbsenceDayServices(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager)
        {
            _workScope = workScope;
            _mezonService = mezonService;
            _settingManager = settingManager;
        }

        private static readonly Dictionary<AnomalyType, string> AnomalyNotes = new Dictionary<AnomalyType, string>()
        {
            { AnomalyType.NoCheckInOut, "No check in & No check out & No off/onsite/remote request" },
            { AnomalyType.NoMorningCheckIn, "No morning check in & No off/onsite request for the morning" },
            { AnomalyType.EarlyCheckOut, "Early check out & No off/onsite request for the afternoon" },
            { AnomalyType.OfficeShortFullday, "Office Fullday: Working hours < Standard hours" },
            { AnomalyType.OfficeShortMorning, "Office Morning: Working hours < Registered morning hours" },
            { AnomalyType.OfficeShortAfternoon, "Office Afternoon: Working hours < Registered afternoon hours" },
            { AnomalyType.RemoteShortFullday, "Remote Fullday: No check out & Tracker time < {0}%" },
            { AnomalyType.RemoteShortMorning, "Remote Morning: No check out & Tracker time < {0}%" },
            { AnomalyType.RemoteShortAfternoon, "Remote Afternoon: No check out & Tracker time < {0}%" }
        };

        private TimeSpan? ParseTimeSpan(string input)
        {
            return TimeSpan.TryParse(input, out var result) ? result : (TimeSpan?)null;
        }

        private double CalculateLateMinutes(TimeSpan? checkInTime, TimeSpan? morningStartAt, TimeSpan? afternoonStartAt)
        {
            double allowedLateMinutes = double.Parse(_settingManager.GetSettingValue(AppSettingNames.LimitedMinutes));
            double lateMinutes = 0.0;
            if (checkInTime.HasValue)
            {
                if (afternoonStartAt.HasValue && checkInTime >= afternoonStartAt)
                {
                    var delay = checkInTime.Value - afternoonStartAt.Value;
                    if (delay.TotalMinutes <= allowedLateMinutes)
                    {
                        lateMinutes = delay.TotalMinutes;
                    }
                }
                else if (morningStartAt.HasValue && checkInTime >= morningStartAt)
                {
                    var delay = checkInTime.Value - morningStartAt.Value;
                    if (delay.TotalMinutes <= allowedLateMinutes)
                    {
                        lateMinutes = delay.TotalMinutes;
                    }
                }
            }
            return lateMinutes;
        }

        private void AddOrUpdateAnomaly(UserWithAnomalyDto user, double? totalWorkingTime, string note, bool isUnplannedAbsence,
            List<YesterdayAnomalyDTO> yesterday, List<LastWeekAnomalyDTO> lastWeek)
        {
            if (user.IsYesterday)
            {
                var yesterdayDto = new YesterdayAnomalyDTO
                {
                    UserId = user.UserId,
                    EmployeeName = user.EmployeeName,
                    UserName = user.UserName,
                    Date = user.DateAt.ToString("dd/MM/yyyy"),
                    ActualHours = Math.Round(totalWorkingTime ?? 0, 2),
                    Notes = note,
                    Branch = user.Branch,
                    IsUnplannedAbsence = isUnplannedAbsence
                };
                yesterday.Add(yesterdayDto);
            }
            else
            {
                var lastWeekDto = lastWeek.FirstOrDefault(a => a.UserId == user.UserId);
                bool isNewUser = false;

                if (lastWeekDto == null)
                {
                    lastWeekDto = new LastWeekAnomalyDTO
                    {
                        UserId = user.UserId,
                        EmployeeName = user.EmployeeName,
                        UserName = user.UserName,
                        Branch = user.Branch
                    };
                    isNewUser = true;
                }

                var dateNote = new DateWithNoteDto { Date = user.DateAt.ToString("dd/MM/yyyy"), Note = note };

                if (isUnplannedAbsence)
                {
                    lastWeekDto.DatesMissed.Add(dateNote);
                }
                else
                {
                    lastWeekDto.DatesBelowThreshold.Add(dateNote);
                }

                lastWeekDto.Count++;

                if (isNewUser)
                {
                    lastWeek.Add(lastWeekDto);
                }
            }
        }

        private AnomalyType? GetInvalidAbsenceNote(List<AbsenceDetailDto> absenceDetails,
            bool isNoCheckInAndNoCheckOut,
            bool isMissingMorningRecord,
            bool isMissingAfternoonRecord)
        {
            if (isNoCheckInAndNoCheckOut)
            {
                bool hasFullDayRequest = absenceDetails.Any(d => d.DateType == DayType.Fullday) ||
                                        (absenceDetails.Any(d => d.DateType == DayType.Morning) && absenceDetails.Any(d => d.DateType == DayType.Afternoon));
                if (!hasFullDayRequest)
                    return AnomalyType.NoCheckInOut;
            }

            if (isMissingMorningRecord)
            {
                bool hasMorningRequest = absenceDetails.Any(d => d.DateType == DayType.Morning
                                        && (d.RequestType == RequestType.Off || d.RequestType == RequestType.Onsite));
                if (!hasMorningRequest)
                    return AnomalyType.NoMorningCheckIn;
            }

            if (isMissingAfternoonRecord)
            {
                bool hasAfternoonRequest = absenceDetails.Any(d => d.DateType == DayType.Afternoon
                                        && (d.RequestType == RequestType.Off || d.RequestType == RequestType.Onsite));
                if (!hasAfternoonRequest)
                    return AnomalyType.EarlyCheckOut;
            }

            return null;
        }

        private void SortAnomalyDates(List<LastWeekAnomalyDTO> lastWeek)
        {
            foreach (var anomaly in lastWeek)
            {
                anomaly.DatesMissed = anomaly.DatesMissed
                    .OrderBy(date => DateTime.ParseExact(date.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
                anomaly.DatesBelowThreshold = anomaly.DatesBelowThreshold
                    .OrderBy(date => DateTime.ParseExact(date.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
            }
        }

        private (double max200kPercentage, double max100kPercentage, double max50kPercentage, double max20kPercentage) GetCommonCofig()
        {
            // Penalty thresholds for tracker time percentage (e.g. < 85% = 20k, < 75% = 50k, < 50% = 100k, < 25% = 200k)
            double max200kPercentage = double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker200kPunishment).Split('-')[1]);
            double max100kPercentage = double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker100kPunishment).Split('-')[1]);
            double max50kPercentage = double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker50kPunishment).Split('-')[1]);
            double max20kPercentage = double.Parse(_settingManager.GetSettingValueForApplication(AppSettingNames.Tracker20kPunishment).Split('-')[1]);

            return (max200kPercentage, max100kPercentage, max50kPercentage, max20kPercentage);
        }

        private (TimeSpan? morningStartAt, TimeSpan? morningEndAt, TimeSpan? afternoonStartAt, TimeSpan? afternoonEndAt, TimeSpan? checkInTime, TimeSpan? checkOutTime)
            ParseTimeSpanString(string morningStart, string morningEnd, string afternoonStart, string afternoonEnd, string checkIn, string checkOut)
        {
            TimeSpan? morningStartAt = ParseTimeSpan(morningStart);
            TimeSpan? morningEndAt = ParseTimeSpan(morningEnd);
            TimeSpan? afternoonStartAt = ParseTimeSpan(afternoonStart);
            TimeSpan? afternoonEndAt = ParseTimeSpan(afternoonEnd);
            TimeSpan? checkInTime = ParseTimeSpan(checkIn);
            TimeSpan? checkOutTime = checkOut != null ? ParseTimeSpan(checkOut) : null;

            return (morningStartAt, morningEndAt, afternoonStartAt, afternoonEndAt, checkInTime, checkOutTime);
        }

        private (double? faceIdHours, double trackerHours) CalculateWorkingHours(
            TimeSpan? checkInTime, 
            TimeSpan? checkOutTime, 
            TimeSpan? morningStartAt, 
            TimeSpan? afternoonStartAt, 
            AbsenceDetailDto offOrOnsiteRequest,
            string trackerTimeString)
        {
            double? faceIdHours = null;
            double breakTime = 1.0;

            if (checkInTime.HasValue && checkOutTime.HasValue)
            {
                double lateMinutes = CalculateLateMinutes(checkInTime, morningStartAt, afternoonStartAt);
                var officeWorkingTime = (checkOutTime.Value.TotalMinutes - checkInTime.Value.TotalMinutes + lateMinutes) / 60.0;
                if (offOrOnsiteRequest == null)
                {
                    officeWorkingTime -= breakTime;
                }
                faceIdHours = Math.Round(officeWorkingTime, 2);
            }

            double trackerHours = TimeSpan.TryParse(trackerTimeString, out var ts)
                ? Math.Round(ts.TotalMinutes / 60.0, 2)
                : 0;

            return (faceIdHours, trackerHours);
        }

        private (List<YesterdayAnomalyDTO> yesterdayAnomalies, List<LastWeekAnomalyDTO> lastWeekAnomalies) ProcessAnomalies(ProcessAnomaliesInputDto input)
        {
            var yesterdayAnomalies = new List<YesterdayAnomalyDTO>();
            var lastWeekAnomalies = new List<LastWeekAnomalyDTO>();

            var timekeepings = input.AllTimekeepings;
            var absenceDetailDict = input.AllAbsenceDetails.GroupBy(d => ((long)d.UserId, d.DateAt)).ToDictionary(g => g.Key, g => g.ToList());

            var ( max200kPercentage, max100kPercentage, max50kPercentage, max20kPercentage) = GetCommonCofig();

            foreach (var tk in timekeepings)
            {
                var userWithAnomaly = new UserWithAnomalyDto
                {
                    UserId = tk.UserId.Value,
                    EmployeeName = tk.User.FullName,
                    UserName = tk.User.UserName,
                    DateAt = tk.DateAt,
                    Branch = tk.User.Branch,
                    IsYesterday = input.IsYesterday
                };

                var (morningStartAt, morningEndAt, afternoonStartAt, afternoonEndAt, checkInTime, checkOutTime) = ParseTimeSpanString(
                    tk.User.MorningStartAt, tk.User.MorningEndAt, tk.User.AfternoonStartAt, tk.User.AfternoonEndAt, tk.CheckIn, tk.CheckOut);

                var absenceData = absenceDetailDict.GetValueOrDefault(((long)tk.UserId, tk.DateAt));
                var wfhRequest = absenceData?.FirstOrDefault(d => d.RequestType == RequestType.Remote);
                var offOrOnsiteRequest = absenceData?.FirstOrDefault(d => d.RequestType == RequestType.Off || d.RequestType == RequestType.Onsite);

                bool isNoCheckInAndNoCheckOut = checkInTime == null && checkOutTime == null && offOrOnsiteRequest?.DateType != DayType.Fullday;
                bool isMissingMorningRecord = checkInTime > morningEndAt && offOrOnsiteRequest?.DateType != DayType.Morning;
                bool isMissingAfternoonRecord = checkInTime < morningEndAt && checkOutTime < afternoonStartAt && offOrOnsiteRequest?.DateType != DayType.Afternoon;

                var (faceIdHours, parsedTrackerHours) = CalculateWorkingHours(
                        checkInTime, checkOutTime, morningStartAt, afternoonStartAt,
                        offOrOnsiteRequest, tk.TrackerTime);

                double registeredMorningHours = offOrOnsiteRequest?.DateType == DayType.Morning ? 0 : tk.User.MorningWorking;
                double registeredAfternoonHours = offOrOnsiteRequest?.DateType == DayType.Afternoon ? 0 : tk.User.AfternoonWorking;
                double totalRegisteredHours = registeredMorningHours + registeredAfternoonHours;

                if ((checkInTime.HasValue && checkOutTime.HasValue) || (checkInTime.HasValue && checkOutTime == null))
                {
                    double requiredTrackerHours = 0.0;
                    double requiredOfficeHours = 0.0;

                    if (wfhRequest?.DateType == DayType.Fullday)
                    {
                        requiredTrackerHours = totalRegisteredHours;
                    }
                    else if (wfhRequest?.DateType == DayType.Morning)
                    {
                        requiredTrackerHours = registeredMorningHours;
                        requiredOfficeHours = registeredAfternoonHours;
                    }
                    else if (wfhRequest?.DateType == DayType.Afternoon)
                    {
                        requiredOfficeHours = registeredMorningHours;
                        requiredTrackerHours = registeredAfternoonHours;
                    }
                    else
                    {
                        requiredOfficeHours = totalRegisteredHours;
                    }

                    double officeHours = faceIdHours ?? 0;
                    double trackerHours = (parsedTrackerHours > 0) ? parsedTrackerHours : officeHours;

                    if (faceIdHours.HasValue && faceIdHours.Value < requiredOfficeHours)
                    {
                        var anomalyType = offOrOnsiteRequest?.DateType == DayType.Morning ? AnomalyType.OfficeShortAfternoon :
                                          offOrOnsiteRequest?.DateType == DayType.Afternoon ? AnomalyType.OfficeShortMorning :
                                            AnomalyType.OfficeShortFullday;
                        string note = AnomalyNotes[anomalyType];
                        AddOrUpdateAnomaly(userWithAnomaly, officeHours, note, false, yesterdayAnomalies, lastWeekAnomalies);
                    }

                    if (requiredTrackerHours > 0)
                    {
                        double trackerPercentage = trackerHours / requiredTrackerHours * 100;
                        double penaltyThreshold = new double[] { max200kPercentage, max100kPercentage, max50kPercentage, max20kPercentage }
                                                    .FirstOrDefault(max => trackerPercentage < max);
                        if (penaltyThreshold > 0)
                        {
                            var remoteAnomalyType = wfhRequest.DateType == DayType.Fullday ? AnomalyType.RemoteShortFullday :
                                                    wfhRequest.DateType == DayType.Morning ? AnomalyType.RemoteShortMorning :
                                                    AnomalyType.RemoteShortAfternoon;

                            string note = string.Format(AnomalyNotes[remoteAnomalyType], penaltyThreshold);
                            AddOrUpdateAnomaly(userWithAnomaly, trackerHours, note, false, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }
                }

                if (isNoCheckInAndNoCheckOut || isMissingMorningRecord || isMissingAfternoonRecord)
                {
                    AnomalyType? absenceType = GetInvalidAbsenceNote(
                        absenceData ?? new List<AbsenceDetailDto>(),
                        isNoCheckInAndNoCheckOut,
                        isMissingMorningRecord,
                        isMissingAfternoonRecord);
                    if (absenceType.HasValue)
                    {
                        string absenceNote = AnomalyNotes[absenceType.Value];
                        AddOrUpdateAnomaly(userWithAnomaly, faceIdHours, absenceNote, true, yesterdayAnomalies, lastWeekAnomalies);
                    }
                }
            }

            SortAnomalyDates(lastWeekAnomalies);
            yesterdayAnomalies = yesterdayAnomalies.OrderBy(a => a.EmployeeName).ToList();
            lastWeekAnomalies = lastWeekAnomalies.OrderBy(a => a.EmployeeName).ToList();
            return (yesterdayAnomalies, lastWeekAnomalies);
        }

        private async Task<AnomalyData> LoadAnomalyDataAsync(
            List<long> branchIds,
            DateTime startDate,
            DateTime endDate)
        {
            var allTimekeepings = await _workScope.GetAll<Timekeeping>()
                .WhereIf(branchIds != null && branchIds.Any(), t => t.User.BranchId.HasValue && branchIds.Contains(t.User.BranchId.Value))
                .Where(t => t.DateAt >= startDate && t.DateAt <= endDate && t.DateAt.DayOfWeek != DayOfWeek.Saturday)
                .Where(t => t.UserId.HasValue)
                .Where(t => t.User.IsActive && !t.User.IsDeleted && !t.User.IsStopWork)
                .Select(t => new TimekeepingDto
                {
                    UserId = t.UserId,
                    DateAt = t.DateAt,
                    CheckIn = t.CheckIn,
                    CheckOut = t.CheckOut,
                    TrackerTime = t.TrackerTime,
                    User = new UserDto
                    {
                        FullName = t.User.FullName,
                        UserName = t.User.UserName,
                        MorningStartAt = t.User.MorningStartAt,
                        MorningEndAt = t.User.MorningEndAt,
                        AfternoonStartAt = t.User.AfternoonStartAt,
                        AfternoonEndAt = t.User.AfternoonEndAt,
                        MorningWorking = t.User.MorningWorking.Value,
                        AfternoonWorking = t.User.AfternoonWorking.Value,
                        Branch = new BranchToDisplayDto
                        {
                            Name = t.User.Branch.Name,
                            Code = t.User.Branch.Code,
                            Color = t.User.Branch.Color
                        }
                    }
                })
                .ToListAsync();

            var allAbsenceDetails = await _workScope.GetAll<AbsenceDayDetail>()
                .Where(d => !d.Request.IsDeleted && d.DateAt >= startDate && d.DateAt <= endDate)
                .Where(d => allTimekeepings.Select(t => t.UserId.Value).Distinct().Contains(d.Request.UserId))
                .Where(d => d.Request.Status == RequestStatus.Approved)
                .Select(d => new AbsenceDetailDto
                {
                    DateAt = d.DateAt.Date,
                    DateType = d.DateType,
                    RequestType = d.Request.Type,
                    UserId = d.Request.UserId
                })
                .ToListAsync();

            return new AnomalyData
            {
                Timekeepings = allTimekeepings,
                AbsenceDetails = allAbsenceDetails
            };
        }
        public async Task<AnomaliesTimelogReportDto> GetAnomaliesTimelogReport(GetAnomaliesTimelogReportInput input)
        {
            try
            {
                var now = DateTimeUtils.GetNow().Date;
                var yesterday = now.AddDays(-1);

                if (yesterday.DayOfWeek == DayOfWeek.Sunday)
                    yesterday = yesterday.AddDays(-2);
                else if (yesterday.DayOfWeek == DayOfWeek.Saturday)
                    yesterday = yesterday.AddDays(-1);

                var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);

                var data = await LoadAnomalyDataAsync(input.BranchIds, lastWeekStart, yesterday);

                var yesterdayTimekeepings = data.Timekeepings.Where(t => t.DateAt == yesterday).ToList();
                var yesterdayAbsences = data.AbsenceDetails.Where(d => d.DateAt == yesterday).ToList();

                var yesterdayProcessAnomaliesInput = new ProcessAnomaliesInputDto
                {
                    AllTimekeepings = yesterdayTimekeepings,
                    AllAbsenceDetails = yesterdayAbsences,
                    StartDate = yesterday,
                    EndDate = yesterday,
                    IsYesterday = true
                };

                var (yesterdayAnomalies, _) = ProcessAnomalies(yesterdayProcessAnomaliesInput);

                var lastWeekTimekeepings = data.Timekeepings.Where(t => t.DateAt >= lastWeekStart && t.DateAt <= lastWeekEnd).ToList();
                var lastWeekAbsences = data.AbsenceDetails.Where(d => d.DateAt >= lastWeekStart && d.DateAt <= lastWeekEnd).ToList();

                var lastWeekProcessAnomaliesInput = new ProcessAnomaliesInputDto
                {
                    AllTimekeepings = lastWeekTimekeepings,
                    AllAbsenceDetails = lastWeekAbsences,
                    StartDate = lastWeekStart,
                    EndDate = lastWeekEnd,
                    IsYesterday = false
                };

                var (_, lastWeekAnomalies) = ProcessAnomalies(lastWeekProcessAnomaliesInput);

                return new AnomaliesTimelogReportDto
                {
                    YesterdayAnomalies = yesterdayAnomalies,
                    LastWeekAnomalies = lastWeekAnomalies
                };
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while generating the anomalies timelog report: ", ex);
            }
        }

        private static (DateTime start, DateTime end) GetLastWeekRange(DateTime now)
        {
            var firstDayOfWeek = DateTimeUtils.FirstDayOfWeek(now);
            var lastWeekStart = firstDayOfWeek.AddDays(-7).Date;
            var lastWeekEnd = firstDayOfWeek.AddDays(-3).Date;
            return (lastWeekStart, lastWeekEnd);
        }

        private async Task<(List<YesterdayAnomalyDTO> yesterdayAnomalies, List<LastWeekAnomalyDTO> lastWeekAnomalies)> GetBranchDataAsync(
            string branchName, DateTime startDate, DateTime endDate, bool isYesterday)
        {
            var branchId = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Where(b => b.Name == branchName)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();

            var data = await LoadAnomalyDataAsync(new List<long> { branchId }, startDate, endDate);

            var processAnomaliesInput = new ProcessAnomaliesInputDto
            {
                AllTimekeepings = data.Timekeepings,
                AllAbsenceDetails = data.AbsenceDetails,
                StartDate = startDate,
                EndDate = endDate,
                IsYesterday = isYesterday
            };

            return ProcessAnomalies(processAnomaliesInput);
        }

        public async Task<List<YesterdayAnomalyDTO>> GetYesterdayAnomalies(string branchName, DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate;
                var (yesterdayAnomalies, _) = await GetBranchDataAsync(branchName, startDate, endDate, isYesterday: true);
                return yesterdayAnomalies;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"An error occurred while retrieving yesterday anomalies for branch '{branchName}': ", ex);
            }
        }

        public async Task<List<LastWeekAnomalyDTO>> GetLastWeekAnomalies(string branchName, DateTime startDate, DateTime endDate)
        {
            try
            {
                var (_, lastWeekAnomalies) = await GetBranchDataAsync(branchName, startDate, endDate, isYesterday: false);
                return lastWeekAnomalies;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"An error occurred while retrieving last week anomalies for branch '{branchName}': ", ex);
            }
        }

        private void AddBoldMarkup(StringBuilder messageBuilder, List<object> mkList, ref int currentPos, string title)
        {
            int titlePos = currentPos;
            messageBuilder.AppendLine(title);
            currentPos += title.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = titlePos, e = titlePos + title.Length });
        }

        private void SendMezonMessage(string botUri, StringBuilder messageBuilder, List<object> mkList)
        {
            var messageText = messageBuilder.ToString();
            if (!string.IsNullOrEmpty(messageText))
            {
                _mezonService.Post(botUri, new
                {
                    type = "hook",
                    botType = "Anomaly",
                    message = new
                    {
                        t = messageText,
                        mk = mkList
                    }
                });
            }
        }

        public async Task<bool> SendDailyAnomaliesToMezon(AnomaliesReportSettingDto input, bool isWeekly)
        {
            DateTime now = DateTime.Now;

            if (!input.enable)
            {
                return true;
            }

            foreach (var branchName in input.branchCodes)
            {
                try
                {
                    if (!isWeekly)
                    {
                        await SendDailyReport(input.botUri, branchName, now.AddDays(-1).Date);
                    }
                    else
                    {
                        var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);
                        await SendWeeklyReport(input.botUri, branchName, lastWeekStart, lastWeekEnd);
                    }
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException($"Error processing branch {branchName}: {ex.Message}");
                }
            }

            return true;
        }

        private async Task SendDailyReport(string botUri, string branchName, DateTime reportDate)
        {
            const int BATCH_SIZE = 5;

            var allAnomalies = await GetYesterdayAnomalies(branchName, reportDate);

            var unplannedAbsences = allAnomalies.Where(a => a.IsUnplannedAbsence).ToList();
            var unapprovedShortHours = allAnomalies.Where(a => !a.IsUnplannedAbsence).ToList();

            SendDailyHeaderMessage(botUri, branchName, reportDate);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Yesterday – Unplanned Absences",
                unplannedAbsences, true, true, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Yesterday – Unapproved Short Working Hours",
                unapprovedShortHours, false, true, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);
        }

        private async Task SendWeeklyReport(string botUri, string branchName, DateTime startDate, DateTime endDate)
        {
            const int BATCH_SIZE = 5;

            var allAnomalies = await GetLastWeekAnomalies(branchName, startDate, endDate);

            var unplannedAbsences = allAnomalies.Where(a => a.DatesMissed.Any()).ToList();
            var unapprovedShortHours = allAnomalies.Where(a => a.DatesBelowThreshold.Any()).ToList();

            SendWeeklyHeaderMessage(botUri, branchName, startDate, endDate);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Last Week – Unplanned Absences",
                unplannedAbsences, true, false, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Last Week – Unapproved Short Working Hours",
                unapprovedShortHours, false, false, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);
        }

        private async Task SendAnomalyMessages<T>(string botUri, string sectionTitle,
            List<T> anomalies, bool isUnplannedAbsence, bool isYesterday, int batchSize)
        {
            if (!anomalies.Any())
            {
                SendEmptySectionMessage(botUri, sectionTitle);
                return;
            }

            var dynamicAnomalies = anomalies.Cast<dynamic>().ToList();
            var chunks = CommonUtils.SplitIntoChunks(dynamicAnomalies, batchSize);
            int globalIndex = 1;

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i].Cast<T>().ToList();
                bool isFirstChunk = i == 0;
                bool isLastChunk = i == chunks.Count - 1;

                string chunkSectionTitle = isFirstChunk ? sectionTitle : null;
                SendAnomalyChunkMessage(botUri, chunkSectionTitle, chunk,
                    isUnplannedAbsence, isYesterday, isFirstChunk, isLastChunk, ref globalIndex);
                await Task.Delay(MESSAGE_DELAY_MS);
            }
        }

        private void SendDailyHeaderMessage(string botUri, string branchName, DateTime reportDate)
        {
            var sb = new StringBuilder();
            var mkList = new List<object>();
            int pos = 0;

            AddBoldMarkup(sb, mkList, ref pos, "════════════ Daily Anomalies Report ════════════");
            AddBoldMarkup(sb, mkList, ref pos, $"Report Period: {reportDate:dd/MM/yyyy}");
            AddBoldMarkup(sb, mkList, ref pos, $"Office: {branchName}");
            sb.AppendLine();

            SendMezonMessage(botUri, sb, mkList);
        }

        private void SendWeeklyHeaderMessage(string botUri, string branchName, DateTime startDate, DateTime endDate)
        {
            var sb = new StringBuilder();
            var mkList = new List<object>();
            int pos = 0;

            AddBoldMarkup(sb, mkList, ref pos, "════════════ Weekly Anomalies Report ════════════");
            AddBoldMarkup(sb, mkList, ref pos, $"Report Period: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}");
            AddBoldMarkup(sb, mkList, ref pos, $"Office: {branchName}");
            sb.AppendLine();

            SendMezonMessage(botUri, sb, mkList);
        }

        private void SendAnomalyChunkMessage<T>(string botUri, string sectionTitle,
            List<T> chunkAnomalies, bool isUnplannedAbsence, bool isYesterday,
            bool isFirstChunk, bool isLastChunk, ref int globalIndex)
        {
            var sb = new StringBuilder();
            var mkList = new List<object>();
            int pos = 0;

            if (isFirstChunk)
            {
                string chunkHeader = $"═══ {sectionTitle} ═══";
                AddBoldMarkup(sb, mkList, ref pos, chunkHeader);
                pos += Environment.NewLine.Length - 2;
            }

            foreach (var anomaly in chunkAnomalies)
            {
                if (isYesterday)
                {
                    var yesterdayAnomaly = (YesterdayAnomalyDTO)(object)anomaly;
                    AppendYesterdayAnomaly(sb, mkList, ref pos, globalIndex++, yesterdayAnomaly);
                }
                else
                {
                    var weekAnomaly = (LastWeekAnomalyDTO)(object)anomaly;
                    AppendWeekAnomaly(sb, mkList, ref pos, globalIndex++, weekAnomaly, isUnplannedAbsence);
                }
                pos += Environment.NewLine.Length;
                sb.AppendLine();
            }

            if (isLastChunk)
            {
                sb.AppendLine("═══════════════════════════════════════");
                pos += "═══════════════════════════════════════".Length + Environment.NewLine.Length;
            }

            SendMezonMessage(botUri, sb, mkList);
        }

        private void AppendYesterdayAnomaly(StringBuilder sb, List<object> mkList, ref int pos,
            int index, YesterdayAnomalyDTO anomaly)
        {
            string empLine = $"{index}. Employee Name: {anomaly.EmployeeName}";
            int empPos = pos;
            sb.AppendLine(empLine);
            pos += empLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = empPos, e = empPos + $"{index}. Employee Name:".Length });

            string dateLine = $"Date: {anomaly.Date}";
            int datePos = pos;
            sb.AppendLine(dateLine);
            pos += dateLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = datePos, e = datePos + "Date:".Length });

            if (!anomaly.IsUnplannedAbsence)
            {
                string hoursLine = $"Actual Hours: {anomaly.ActualHours}";
                int hoursPos = pos;
                sb.AppendLine(hoursLine);
                pos += hoursLine.Length + Environment.NewLine.Length;
                mkList.Add(new { type = "b", s = hoursPos, e = hoursPos + "Actual Hours:".Length });
            }

            string notesLine = $"Notes: {anomaly.Notes}";
            int notesPos = pos;
            sb.AppendLine(notesLine);
            pos += notesLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });
        }

        private void AppendWeekAnomaly(StringBuilder sb, List<object> mkList, ref int pos,
            int index, LastWeekAnomalyDTO anomaly, bool isUnplannedAbsence)
        {
            string empLine = $"{index}. Employee Name: {anomaly.EmployeeName}";
            int empPos = pos;
            sb.AppendLine(empLine);
            pos += empLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = empPos, e = empPos + $"{index}. Employee Name:".Length });

            List<string> dates;
            string datesKey;

            if (isUnplannedAbsence)
            {
                dates = anomaly.DatesMissed.Select(d => $"{d.Date} [{d.Note}]").ToList();
                datesKey = "Dates Missed";
            }
            else
            {
                dates = anomaly.DatesBelowThreshold.Select(d => $"{d.Date} [{d.Note}]").ToList();
                datesKey = "Dates Below Threshold";
            }

            string countLine = $"Count: {dates.Count}";
            int countPos = pos;
            sb.AppendLine(countLine);
            pos += countLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = countPos, e = countPos + "Count:".Length });

            string datesHeader = $"{datesKey}:";
            int datesPos = pos;
            sb.AppendLine(datesHeader);
            pos += datesHeader.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = datesPos, e = datesPos + datesHeader.Length });

            foreach (var dateItem in dates)
            {
                string line = $"  - {dateItem}";
                sb.AppendLine(line);
                pos += line.Length + Environment.NewLine.Length;
            }
        }

        private void SendEmptySectionMessage(string botUri, string sectionTitle)
        {
            var sb = new StringBuilder();
            var mkList = new List<object>();
            int pos = 0;

            string header = $"═══ {sectionTitle} ═══";
            AddBoldMarkup(sb, mkList, ref pos, header);
            pos += Environment.NewLine.Length - 2;
            sb.AppendLine("No anomalies found in this category");
            sb.AppendLine("═══════════════════════════════════════");

            SendMezonMessage(botUri, sb, mkList);
        }
    }
}
