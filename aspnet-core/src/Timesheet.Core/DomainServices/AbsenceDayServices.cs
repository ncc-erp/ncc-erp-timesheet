using Abp.Application.Services.Dto;
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

        private TimeSpan? ParseTimeSpan(string input)
        {
            return TimeSpan.TryParse(input, out var result) ? result : (TimeSpan?)null;
        }

        private double CalculateLateMinutes(TimeSpan? checkInTime, TimeSpan? morningStartAt, TimeSpan? afternoonStartAt, double minutes)
        {
            double lateMinutes = 0.0;
            if (checkInTime.HasValue)
            {
                if (morningStartAt.HasValue && checkInTime > morningStartAt)
                {
                    var delay = checkInTime.Value - morningStartAt.Value;
                    if (delay.TotalMinutes <= minutes)
                    {
                        lateMinutes = Math.Min(delay.TotalMinutes, minutes);
                    }
                }
                else if (afternoonStartAt.HasValue && checkInTime > afternoonStartAt)
                {
                    var delay = checkInTime.Value - afternoonStartAt.Value;
                    if (delay.TotalMinutes <= minutes)
                    {
                        lateMinutes = Math.Min(delay.TotalMinutes, minutes);
                    }
                }
            }
            return lateMinutes;
        }

        private void AddOrUpdateAnomaly(AnomalyInputDto input, List<YesterdayAnomalyDTO> yesterday, List<LastWeekAnomalyDTO> lastWeek)
        {
            if (input.IsYesterdayInFunction)
            {
                var yesterdayDto = new YesterdayAnomalyDTO
                {
                    UserId = input.UserId,
                    EmployeeName = input.EmployeeName,
                    UserName = input.UserName,
                    Date = input.Date,
                    ActualHours = input.TotalWorkingTime.HasValue ? $"{input.TotalWorkingTime.Value:F2}h" : "0h",
                    Notes = input.Notes,
                    Branch = input.Branch
                };
                yesterday.Add(yesterdayDto);
            }
            else
            {
                var lastWeekDto = lastWeek.FirstOrDefault(a => a.UserId == input.UserId) ?? 
                    new LastWeekAnomalyDTO { 
                        UserId = input.UserId,
                        EmployeeName = input.EmployeeName, 
                        UserName = input.UserName, 
                        Branch = input.Branch 
                    };
                if (input.ViolationType == ViolationStatus.DatesMissed)
                    lastWeekDto.DatesMissed.Add(input.Date);
                else if (input.ViolationType == ViolationStatus.DatesNoTrackerTime)
                    lastWeekDto.DatesNoTrackerTime.Add(input.Date);
                else
                    lastWeekDto.DatesBelowThreshold.Add(input.Date);
                lastWeekDto.Count++;
                lastWeekDto.Notes = input.Notes;
                if (!lastWeek.Any(a => a.UserId == input.UserId))
                    lastWeek.Add(lastWeekDto);
            }
        }

        private bool IsValidAbsenceForCase(List<AbsenceDetailDto> absenceDetailsAndDayApplied,
            bool isFullDayAbsence, bool isMorningAbsence, bool isAfternoonAbsence)
        {
            if (absenceDetailsAndDayApplied == null || !absenceDetailsAndDayApplied.Any())
                return false;

            bool hasMorningRequest = false, hasAfternoonRequest = false;
            bool hasMorningApproved = false, hasAfternoonApproved = false;
            bool hasMorningRemote = false, hasAfternoonRemote = false;
            bool hasMorningOffOnsite = false, hasAfternoonOffOnsite = false;

            foreach (var detail in absenceDetailsAndDayApplied)
            {
                if (detail.RequestStatus == RequestStatus.Approved)
                {
                    if (isFullDayAbsence && detail.DateType == DayType.Fullday)
                        return true;
                    if (isMorningAbsence && detail.DateType == DayType.Morning)
                        return true;
                    if (isAfternoonAbsence && detail.DateType == DayType.Afternoon)
                        return true;
                    if ((isMorningAbsence || isAfternoonAbsence) && detail.DateType == DayType.Fullday && detail.RequestType == RequestType.Remote)
                        return true;
                    if (isFullDayAbsence && detail.RequestType == RequestType.Remote)
                        return true;

                    if (detail.DateType == DayType.Morning)
                    {
                        hasMorningRequest = true;
                        if (detail.RequestType == RequestType.Off || detail.RequestType == RequestType.Onsite)
                            hasMorningOffOnsite = true;
                        if (detail.RequestType == RequestType.Remote)
                            hasMorningRemote = true;
                        if (detail.RequestType == RequestType.Off || detail.RequestType == RequestType.Onsite || detail.RequestType == RequestType.Remote)
                            hasMorningApproved = true;
                    }
                    else if (detail.DateType == DayType.Afternoon)
                    {
                        hasAfternoonRequest = true;
                        if (detail.RequestType == RequestType.Off || detail.RequestType == RequestType.Onsite)
                            hasAfternoonOffOnsite = true;
                        if (detail.RequestType == RequestType.Remote)
                            hasAfternoonRemote = true;
                        if (detail.RequestType == RequestType.Off || detail.RequestType == RequestType.Onsite || detail.RequestType == RequestType.Remote)
                            hasAfternoonApproved = true;
                    }
                }
            }

            if (hasMorningRequest && hasAfternoonRequest &&
                ((hasMorningOffOnsite && hasAfternoonRemote) || (hasAfternoonOffOnsite && hasMorningRemote) ||
                (hasMorningOffOnsite && hasAfternoonOffOnsite)) &&
                hasMorningApproved && hasAfternoonApproved)
            {
                return true;
            }

            return false;
        }

        private void CreateAndLogAnomaly(UserWithAnomalyDto user, double? workingTime, string notes, ViolationStatus violationType,
            List<YesterdayAnomalyDTO> yesterdayAnomalies,
            List<LastWeekAnomalyDTO> lastWeekAnomalies)
        {
            var anomalyInput = new AnomalyInputDto
            {
                UserId = user.UserId,
                EmployeeName = user.EmployeeName,
                UserName = user.UserName,
                Date = user.DateAt.ToString("dd/MM/yyyy"),
                TotalWorkingTime = workingTime,
                Notes = notes,
                IsYesterdayInFunction = user.IsYesterday,
                ViolationType = violationType,
                Branch = user.Branch
            };

            AddOrUpdateAnomaly(anomalyInput, yesterdayAnomalies, lastWeekAnomalies);
        }

        private void SortAnomalyDates(List<LastWeekAnomalyDTO> lastWeek)
        {
            foreach (var anomaly in lastWeek)
            {
                anomaly.DatesMissed = anomaly.DatesMissed
                    .OrderBy(date => DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
                anomaly.DatesNoTrackerTime = anomaly.DatesNoTrackerTime
                    .OrderBy(date => DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
                anomaly.DatesBelowThreshold = anomaly.DatesBelowThreshold
                    .OrderBy(date => DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
            }
        }

        private (List<YesterdayAnomalyDTO> yesterdayAnomalies, List<LastWeekAnomalyDTO> lastWeekAnomalies) ProcessAnomalies(ProcessAnomaliesInputDto input)
        {
            var branchUsers = input.AllUsers.ToList();
            if (!branchUsers.Any()) return (new List<YesterdayAnomalyDTO>(), new List<LastWeekAnomalyDTO>());
            var userDict = branchUsers.ToDictionary(u => u.Id);

            var timekeepings = input.AllTimekeepings.ToList();

            var absenceDetailDict = input.AllAbsenceDetails.GroupBy(d => ((long)d.UserId, d.DateAt)).ToDictionary(g => g.Key, g => g.ToList());

            var yesterdayAnomalies = new List<YesterdayAnomalyDTO>();
            var lastWeekAnomalies = new List<LastWeekAnomalyDTO>();
            double wfhThreshold = double.Parse(_settingManager.GetSettingValue(AppSettingNames.PercentOfTrackerOnWorking)) / 100;
            double allowedLateMinutes = double.Parse(_settingManager.GetSettingValue(AppSettingNames.LimitedMinutes));

            foreach (var tk in timekeepings)
            {
                if (!userDict.TryGetValue((long)tk.UserId, out var user)) continue;

                var userWithAnomaly = new UserWithAnomalyDto
                {
                    UserId = tk.UserId.Value,
                    EmployeeName = user.FullName,
                    UserName = user.UserName,
                    DateAt = tk.DateAt,
                    Branch = user.Branch,
                    IsYesterday = input.IsYesterday
                };

                double? officeActualHours = null;
                double? trackerActualHours = null;
                double? trackerTimeHours = null;

                TimeSpan? morningStartAt = ParseTimeSpan(user.MorningStartAt);
                TimeSpan? morningEndAt = ParseTimeSpan(user.MorningEndAt);
                TimeSpan? afternoonStartAt = ParseTimeSpan(user.AfternoonStartAt);
                TimeSpan? afternoonEndAt = ParseTimeSpan(user.AfternoonEndAt);
                TimeSpan? checkInTime = ParseTimeSpan(tk.CheckIn);
                TimeSpan? checkOutTime = tk.CheckOut != null ? ParseTimeSpan(tk.CheckOut) : null;

                double breakTime = (afternoonStartAt.HasValue && morningEndAt.HasValue)
                    ? (afternoonStartAt.Value.TotalMinutes - morningEndAt.Value.TotalMinutes) / 60.0
                    : 1.0;

                double lateMinutes = CalculateLateMinutes(checkInTime, morningStartAt, afternoonStartAt, allowedLateMinutes);

                var absenceKey = ((long)tk.UserId, tk.DateAt);
                var hasAbsenceData = absenceDetailDict.TryGetValue(absenceKey, out var absenceData);

                bool isWFH = hasAbsenceData && absenceData.Any(d => d.RequestType == RequestType.Remote && d.RequestStatus == RequestStatus.Approved);

                bool isWFHFullday = isWFH && absenceData.Any(d => d.DateType == DayType.Fullday);
                bool isWFHMorning = isWFH && absenceData.Any(d => d.DateType == DayType.Morning);
                bool isWFHAfternoon = isWFH && absenceData.Any(d => d.DateType == DayType.Afternoon);
                bool isWFHAny = isWFHFullday || isWFHMorning || isWFHAfternoon;

                bool isFullDayAbsence = tk.CheckIn == null && tk.CheckOut == null;
                bool isMorningAbsence = checkInTime.HasValue && morningEndAt.HasValue && checkInTime > morningEndAt;
                bool isAfternoonAbsence = checkInTime.HasValue && !isWFHFullday &&
                    (!checkOutTime.HasValue || (afternoonStartAt.HasValue && checkOutTime.HasValue && checkOutTime < afternoonStartAt));

                double? parsedTrackerHours = null;
                if (!string.IsNullOrEmpty(tk.TrackerTime) && TimeSpan.TryParse(tk.TrackerTime, out var trackerTimeSpan))
                {
                    parsedTrackerHours = Math.Round((trackerTimeSpan.TotalMinutes + lateMinutes) / 60.0, 2);
                }

                double? calculatedWorkingHours = null;
                if (checkInTime.HasValue && checkOutTime.HasValue)
                {
                    var workingTime = (checkOutTime.Value.TotalMinutes - checkInTime.Value.TotalMinutes + lateMinutes) / 60.0;
                    if (!(isMorningAbsence || isAfternoonAbsence))
                    {
                        workingTime -= breakTime;
                    }
                    calculatedWorkingHours = Math.Round(workingTime, 2);
                }

                if (isWFHAny)
                {
                    trackerTimeHours = (parsedTrackerHours > 0) ? parsedTrackerHours : calculatedWorkingHours;
                    trackerActualHours = trackerTimeHours > 0 ? trackerTimeHours : 0;
                }

                if (calculatedWorkingHours.HasValue)
                {
                    officeActualHours = calculatedWorkingHours;
                }
                else if (!isWFHAny && checkInTime.HasValue && !checkOutTime.HasValue)
                {
                    officeActualHours = parsedTrackerHours;
                }

                double tardinessHour = 0, leaveEarlyHour = 0;
                if (hasAbsenceData)
                {
                    foreach (var d in absenceData)
                    {
                        if (d.AbsenceTime == OnDayType.DiMuon && d.Hour > 0)
                            tardinessHour += d.Hour;
                        if (d.AbsenceTime == OnDayType.VeSom && d.Hour > 0)
                            leaveEarlyHour += d.Hour;
                    }
                }

                if ((checkInTime.HasValue && checkOutTime.HasValue && !isFullDayAbsence) ||
                    (checkInTime == null && checkOutTime == null && isWFHAny) ||
                    (checkInTime.HasValue && checkOutTime == null && isWFHAny))
                {
                    double? totalWorkingTime = (officeActualHours ?? 0) + (trackerActualHours ?? 0);
                    double standardWorkHours = 8.0;
                    if (isAfternoonAbsence)
                        standardWorkHours = user.MorningWorking;
                    else if (isMorningAbsence)
                        standardWorkHours = user.AfternoonWorking;

                    double requiredHours = 0.0;
                    bool useWfhThreshold = isWFHAny || (!isWFHAny && officeActualHours.HasValue && !checkOutTime.HasValue);

                    if (isWFHFullday)
                        requiredHours = standardWorkHours * wfhThreshold;
                    else if (isWFHMorning)
                        requiredHours = user.MorningWorking * wfhThreshold;
                    else if (isWFHAfternoon)
                        requiredHours = user.AfternoonWorking * wfhThreshold;
                    else
                        requiredHours = useWfhThreshold ? standardWorkHours * wfhThreshold : standardWorkHours;
                    if (tardinessHour > 0 || leaveEarlyHour > 0)
                        requiredHours -= (tardinessHour + leaveEarlyHour);

                    bool isTimeViolation = (isWFHAny && trackerActualHours.HasValue && trackerActualHours.Value < requiredHours) ||
                        (!isWFHAny && officeActualHours.HasValue && officeActualHours.Value < requiredHours);

                    bool isZeroTimeViolation = (isWFHAny && trackerActualHours == 0.0) || (!isWFHAny && officeActualHours == 0.0);

                    string notes = isZeroTimeViolation ? "No tracker time for approved WFH" : "No early leave/late arrival approval";
                    ViolationStatus violationType = isZeroTimeViolation ? ViolationStatus.DatesNoTrackerTime : ViolationStatus.DatesBelowThreshold;

                    if (isTimeViolation || isZeroTimeViolation)
                    {
                        CreateAndLogAnomaly(userWithAnomaly, totalWorkingTime, notes, violationType, yesterdayAnomalies, lastWeekAnomalies);
                    }

                    if (isWFHMorning && isMorningAbsence)
                    {
                        double afternoonRequiredHours = user.AfternoonWorking - tardinessHour - leaveEarlyHour;
                        if ((officeActualHours ?? 0) < afternoonRequiredHours)
                        {
                            CreateAndLogAnomaly(userWithAnomaly, officeActualHours, notes, violationType, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }

                    if (isWFHAfternoon && isAfternoonAbsence)
                    {
                        double morningRequiredHours = user.MorningWorking - tardinessHour - leaveEarlyHour;
                        if ((officeActualHours ?? 0) < morningRequiredHours)
                        {
                            CreateAndLogAnomaly(userWithAnomaly, officeActualHours, notes, violationType, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }
                }

                if (isFullDayAbsence || isMorningAbsence || isAfternoonAbsence)
                {
                    bool isValidAbsence = IsValidAbsenceForCase(absenceData, isFullDayAbsence, isMorningAbsence, isAfternoonAbsence);

                    if (!isValidAbsence)
                    {
                        CreateAndLogAnomaly(userWithAnomaly, officeActualHours, "No leave/WFH record", ViolationStatus.DatesMissed, yesterdayAnomalies, lastWeekAnomalies);
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
            var allUsers = await _workScope.GetAll<User>()
                .Include(u => u.Branch)
                .WhereIf(branchIds != null && branchIds.Any(), u => u.BranchId.HasValue && branchIds.Contains(u.BranchId.Value))
                .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    UserName = u.UserName,
                    MorningStartAt = u.MorningStartAt,
                    MorningEndAt = u.MorningEndAt,
                    AfternoonStartAt = u.AfternoonStartAt,
                    AfternoonEndAt = u.AfternoonEndAt,
                    MorningWorking = u.MorningWorking.Value,
                    AfternoonWorking = u.AfternoonWorking.Value,
                    Branch = new BranchToDisplayDto
                    {
                        Name = u.Branch.Name,
                        Code = u.Branch.Code,
                        Color = u.Branch.Color
                    }
                })
                .ToListAsync();

            var userIds = allUsers.Select(u => u.Id).ToList();

            var allTimekeepings = await _workScope.GetAll<Timekeeping>()
                .Where(t => t.DateAt >= startDate && t.DateAt <= endDate)
                .Where(t => t.UserId.HasValue && userIds.Contains(t.UserId.Value))
                .Select(t => new TimekeepingDto
                {
                    UserId = t.UserId,
                    DateAt = t.DateAt,
                    CheckIn = t.CheckIn,
                    CheckOut = t.CheckOut,
                    TrackerTime = t.TrackerTime
                })
                .ToListAsync();

            var allAbsenceDetails = await _workScope.GetAll<AbsenceDayDetail>()
                .Where(d => !d.Request.IsDeleted && d.DateAt >= startDate && d.DateAt <= endDate)
                .Where(d => userIds.Contains(d.Request.UserId))
                .Select(d => new AbsenceDetailDto
                {
                    RequestId = d.RequestId,
                    DateAt = d.DateAt.Date,
                    DateType = d.DateType,
                    AbsenceTime = d.AbsenceTime,
                    Hour = d.Hour,
                    RequestStatus = d.Request.Status,
                    RequestType = d.Request.Type,
                    UserId = d.Request.UserId
                })
                .ToListAsync();

            return new AnomalyData
            {
                Users = allUsers,
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
                var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);

                var data = await LoadAnomalyDataAsync(input.BranchIds, lastWeekStart, yesterday);

                // Nếu gộp truy vấn trong hàm LoadAnomalyDataAsync thì sẽ chỉ cần một lần filter các bản ghi trong ngày hôm qua
                var yesterdayTimekeepings = data.Timekeepings.Where(t => t.DateAt == yesterday).ToList();
                var yesterdayAbsences = data.AbsenceDetails.Where(d => d.DateAt == yesterday).ToList();

                var yesterdayProcessAnomaliesInput = new ProcessAnomaliesInputDto
                {
                    AllUsers = data.Users,
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
                    AllUsers = data.Users,
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
            var lastWeekEnd = firstDayOfWeek.AddDays(-1).Date;
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
                AllUsers = data.Users,
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
                        await SendSmartDailyReport(input.botUri, branchName, now.AddDays(-1).Date);
                    }
                    else
                    {
                        var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);
                        await SendSmartWeeklyReport(input.botUri, branchName, lastWeekStart, lastWeekEnd);
                    }
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException($"Error processing branch {branchName}: {ex.Message}");
                }
            }

            return true;
        }

        private async Task SendSmartDailyReport(string botUri, string branchName, DateTime reportDate)
        {
            const int BATCH_SIZE = 5;

            var allAnomalies = await GetYesterdayAnomalies(branchName, reportDate);

            var unplannedAbsences = allAnomalies.Where(a => a.Notes == "No leave/WFH record").ToList();
            var unapprovedShortHours = allAnomalies.Where(a => a.Notes == "No early leave/late arrival approval").ToList();
            var noTrackerTime = allAnomalies.Where(a => a.Notes == "No tracker time for approved WFH").ToList();

            SendDailyHeaderMessage(botUri, branchName, reportDate);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Yesterday – Unplanned Absences",
                unplannedAbsences, "No leave/WFH record", true, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Yesterday – Unapproved Short Working Hours",
                unapprovedShortHours, "No early leave/late arrival approval", true, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Yesterday – Missing Tracker Time For Approved WFH",
                noTrackerTime, "No tracker time for approved WFH", true, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);
        }

        private async Task SendSmartWeeklyReport(string botUri, string branchName, DateTime startDate, DateTime endDate)
        {
            const int BATCH_SIZE = 5;

            var allAnomalies = await GetLastWeekAnomalies(branchName, startDate, endDate);

            var unplannedAbsences = allAnomalies.Where(a => a.DatesMissed.Any()).ToList();
            var unapprovedShortHours = allAnomalies.Where(a => a.DatesBelowThreshold.Any()).ToList();
            var noTrackerTime = allAnomalies.Where(a => a.DatesNoTrackerTime.Any()).ToList();

            SendWeeklyHeaderMessage(botUri, branchName, startDate, endDate);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Last Week – Unplanned Absences",
                unplannedAbsences, "No leave/WFH record", false, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Last Week – Unapproved Short Working Hours",
                unapprovedShortHours, "No early leave/late arrival approval", false, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, "Last Week – Missing Tracker Time For Approved WFH",
                noTrackerTime, "No tracker time for approved WFH", false, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);
        }

        private async Task SendAnomalyMessages<T>(string botUri, string sectionTitle,
            List<T> anomalies, string notes, bool isYesterday, int batchSize)
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
                    notes, isYesterday, isFirstChunk, isLastChunk, ref globalIndex);
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
            List<T> chunkAnomalies, string notes, bool isYesterday,
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
                    AppendYesterdayAnomaly(sb, mkList, ref pos, globalIndex++, yesterdayAnomaly, notes);
                }
                else
                {
                    var weekAnomaly = (LastWeekAnomalyDTO)(object)anomaly;
                    AppendWeekAnomaly(sb, mkList, ref pos, globalIndex++, weekAnomaly, notes);
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
            int index, YesterdayAnomalyDTO anomaly, string notes)
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

            if (notes == "No early leave/late arrival approval")
            {
                string hoursLine = $"Actual Hours: {anomaly.ActualHours}";
                int hoursPos = pos;
                sb.AppendLine(hoursLine);
                pos += hoursLine.Length + Environment.NewLine.Length;
                mkList.Add(new { type = "b", s = hoursPos, e = hoursPos + "Actual Hours:".Length });
            }

            string notesLine = $"Notes: {notes}";
            int notesPos = pos;
            sb.AppendLine(notesLine);
            pos += notesLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });
        }

        private void AppendWeekAnomaly(StringBuilder sb, List<object> mkList, ref int pos,
            int index, LastWeekAnomalyDTO anomaly, string notes)
        {
            string empLine = $"{index}. Employee Name: {anomaly.EmployeeName}";
            int empPos = pos;
            sb.AppendLine(empLine);
            pos += empLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = empPos, e = empPos + $"{index}. Employee Name:".Length });

            List<string> dates;
            string datesKey;
            switch (notes)
            {
                case "No leave/WFH record":
                    dates = anomaly.DatesMissed;
                    datesKey = "Dates Missed";
                    break;
                case "No early leave/late arrival approval":
                    dates = anomaly.DatesBelowThreshold;
                    datesKey = "Dates Below Threshold";
                    break;
                default:
                    dates = anomaly.DatesNoTrackerTime;
                    datesKey = "Dates With No Tracker Time";
                    break;
            }

            string datesLine = $"{datesKey}: {string.Join(", ", dates)}";
            int datesPos = pos;
            sb.AppendLine(datesLine);
            pos += datesLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = datesPos, e = datesPos + $"{datesKey}:".Length });

            string countLine = $"Count: {dates.Count}";
            int countPos = pos;
            sb.AppendLine(countLine);
            pos += countLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = countPos, e = countPos + "Count:".Length });

            string notesLine = $"Notes: {notes}";
            int notesPos = pos;
            sb.AppendLine(notesLine);
            pos += notesLine.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });
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
