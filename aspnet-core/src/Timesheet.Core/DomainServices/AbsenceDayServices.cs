using Abp.Dependency;
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
using Timesheet.Services.Mezon;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class AbsenceDayServices : IAbsenceDayServices, ITransientDependency
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private const int MESSAGE_DELAY_MS = 1000;
        public AbsenceDayServices(IWorkScope workScope, MezonService mezonService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
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
                var lastWeekDto = lastWeek.FirstOrDefault(a => a.UserId == input.UserId) ?? new LastWeekAnomalyDTO { UserId = input.UserId, EmployeeName = input.EmployeeName, UserName = input.UserName, Branch = input.Branch };
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

        private bool IsValidAbsenceForCase(IEnumerable<AbsenceRequestDto> userAbsenceRequests, DateTime dateAt,
            bool isFullDayAbsence, bool isMorningAbsence, bool isAfternoonAbsence,
            Dictionary<(long, DateTime), List<AbsenceDetailDto>> absenceDetailDict)
        {
            var relevantRequests = userAbsenceRequests
                .Where(r => absenceDetailDict.ContainsKey((r.Id, dateAt)))
                .ToList();

            bool hasMorningRequest = false, hasAfternoonRequest = false;
            bool hasMorningApproved = false, hasAfternoonApproved = false;
            bool hasMorningRemote = false, hasAfternoonRemote = false;
            bool hasMorningOffOnsite = false, hasAfternoonOffOnsite = false;

            foreach (var request in relevantRequests)
            {
                if (!absenceDetailDict.TryGetValue((request.Id, dateAt), out var details))
                    continue;

                foreach (var detail in details)
                {
                    if (request.Status == RequestStatus.Approved)
                    {
                        if (isFullDayAbsence && detail.DateType == DayType.Fullday)
                            return true;
                        if (isMorningAbsence && detail.DateType == DayType.Morning)
                            return true;
                        if (isAfternoonAbsence && detail.DateType == DayType.Afternoon)
                            return true;
                        if ((isMorningAbsence || isAfternoonAbsence) && detail.DateType == DayType.Fullday && request.Type == RequestType.Remote)
                            return true;
                        if (isFullDayAbsence && request.Type == RequestType.Remote)
                            return true;

                        if (detail.DateType == DayType.Morning)
                        {
                            hasMorningRequest = true;
                            if (request.Type == RequestType.Off || request.Type == RequestType.Onsite)
                                hasMorningOffOnsite = true;
                            if (request.Type == RequestType.Remote)
                                hasMorningRemote = true;
                            if (request.Type == RequestType.Off || request.Type == RequestType.Onsite || request.Type == RequestType.Remote)
                                hasMorningApproved = true;
                        }
                        else if (detail.DateType == DayType.Afternoon)
                        {
                            hasAfternoonRequest = true;
                            if (request.Type == RequestType.Off || request.Type == RequestType.Onsite)
                                hasAfternoonOffOnsite = true;
                            if (request.Type == RequestType.Remote)
                                hasAfternoonRemote = true;
                            if (request.Type == RequestType.Off || request.Type == RequestType.Onsite || request.Type == RequestType.Remote)
                                hasAfternoonApproved = true;
                        }
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

        private async Task<(List<YesterdayAnomalyDTO> yesterdayAnomalies, List<LastWeekAnomalyDTO> lastWeekAnomalies)> ProcessAnomalies(ProcessAnomaliesInputDto input)
        {
            var branchUsers = input.AllUsers.Where(u => u.BranchId == input.BranchId).ToList();
            if (!branchUsers.Any()) return (new List<YesterdayAnomalyDTO>(), new List<LastWeekAnomalyDTO>());

            var userDict = branchUsers.ToDictionary(u => u.Id);

            var userIds = branchUsers.Select(u => u.Id).ToList();

            var timekeepings = input.AllTimekeepings
                .Where(t => userIds.Contains((long) t.UserId) && t.DateAt >= input.StartDate && t.DateAt <= input.EndDate)
                .ToList();

            var timekeepingDict = timekeepings
                .GroupBy(t => t.UserId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var absenceRequests = input.AllAbsenceRequests.Where(r => userIds.Contains(r.UserId)).ToList();
            var absenceRequestDict = absenceRequests.GroupBy(r => r.UserId).ToDictionary(g => g.Key, g => g.ToList());

            var absenceRequestIds = absenceRequests.Select(r => r.Id).ToList();
            var absenceDetails = input.AllAbsenceDetails.Where(d => absenceRequestIds.Contains(d.RequestId)).ToList();
            var absenceDetailDict = absenceDetails.GroupBy(d => ((long)d.RequestId, d.DateAt)).ToDictionary(g => g.Key, g => g.ToList());

            var yesterdayAnomalies = new List<YesterdayAnomalyDTO>();
            var lastWeekAnomalies = new List<LastWeekAnomalyDTO>();
            const double breakTime = 1.0;
            const double wfhThreshold = 0.85;
            const double gracePeriodMinutes = 15.0;

            foreach (var tk in timekeepings)
            {
                if (!userDict.TryGetValue((long)tk.UserId, out var user)) continue;

                bool isValidAbsence = false;
                double? officeActualHours = null;
                double? trackerActualHours = null;
                double? trackerTimeHours = null;

                TimeSpan? morningStartAt = ParseTimeSpan(user.MorningStartAt);
                TimeSpan? morningEndAt = ParseTimeSpan(user.MorningEndAt);
                TimeSpan? afternoonStartAt = ParseTimeSpan(user.AfternoonStartAt);
                TimeSpan? afternoonEndAt = ParseTimeSpan(user.AfternoonEndAt);
                TimeSpan? checkInTime = ParseTimeSpan(tk.CheckIn);
                TimeSpan? checkOutTime = tk.CheckOut != null ? ParseTimeSpan(tk.CheckOut) : null;

                var absenceKey = ((long)tk.UserId, (DateTime)tk.DateAt);
                var hasAbsenceData = absenceDetailDict.TryGetValue(absenceKey, out var absenceData);
                if (!absenceRequestDict.TryGetValue((long)tk.UserId, out var userAbsenceRequests))
                {
                    userAbsenceRequests = new List<AbsenceRequestDto>();
                }

                bool isWFH = hasAbsenceData && absenceData.Any(d => d.RequestType == RequestType.Remote && d.RequestStatus == RequestStatus.Approved);

                bool isWFHFullday = isWFH && absenceData.Any(d => d.DateType == DayType.Fullday);
                bool isWFHMorning = isWFH && absenceData.Any(d => d.DateType == DayType.Morning);
                bool isWFHAfternoon = isWFH && absenceData.Any(d => d.DateType == DayType.Afternoon);

                bool isFullDayAbsence = tk.CheckIn == null && tk.CheckOut == null;
                bool isMorningAbsence = checkInTime.HasValue && morningEndAt.HasValue && checkInTime > morningEndAt;
                bool isAfternoonAbsence = checkInTime.HasValue && !isWFHFullday && !isWFHAfternoon &&
                    (!checkOutTime.HasValue || (afternoonStartAt.HasValue && checkOutTime.HasValue && checkOutTime < afternoonStartAt));

                if (!string.IsNullOrEmpty(tk.TrackerTime) && (isWFHFullday || isWFHMorning || isWFHAfternoon))
                {
                    if (TimeSpan.TryParse(tk.TrackerTime, out var trackerTimeSpan))
                    {
                        trackerTimeHours = Math.Round(trackerTimeSpan.TotalHours, 2);
                        double lateMinutes = CalculateLateMinutes(checkInTime, morningStartAt, afternoonStartAt, gracePeriodMinutes);
                        trackerTimeHours += lateMinutes / 60.0;
                    }
                }

                if ((isWFHFullday || isWFHMorning || isWFHAfternoon) && trackerTimeHours.GetValueOrDefault(0) == 0 && checkInTime.HasValue && checkOutTime.HasValue)
                {
                    var workingTime = (checkOutTime.Value - checkInTime.Value).TotalHours;
                    double lateMinutes = CalculateLateMinutes(checkInTime, morningStartAt, afternoonStartAt, gracePeriodMinutes);
                    workingTime += lateMinutes / 60.0;
                    if (!(isMorningAbsence || isAfternoonAbsence))
                    {
                        workingTime -= breakTime;
                    }
                    trackerTimeHours = Math.Round(workingTime, 2);
                }

                trackerActualHours = trackerTimeHours > 0 ? trackerTimeHours : 0;

                if (checkInTime.HasValue && checkOutTime.HasValue)
                {
                    var workingTime = (checkOutTime.Value - checkInTime.Value).TotalHours;
                    double lateMinutes = CalculateLateMinutes(checkInTime, morningStartAt, afternoonStartAt, gracePeriodMinutes);
                    workingTime += lateMinutes / 60.0;
                    if (!(isMorningAbsence|| isAfternoonAbsence))
                    {
                        workingTime -= breakTime;
                    }
                    officeActualHours = Math.Round(workingTime, 2);
                }
                else if (!(isWFHFullday || isWFHMorning || isWFHAfternoon) && checkInTime.HasValue && !checkOutTime.HasValue && !string.IsNullOrEmpty(tk.TrackerTime))
                {
                    if (TimeSpan.TryParse(tk.TrackerTime, out var trackerTimeSpan))
                    {
                        officeActualHours = Math.Round(trackerTimeSpan.TotalHours, 2);
                        double lateMinutes = CalculateLateMinutes(checkInTime, morningStartAt, afternoonStartAt, gracePeriodMinutes);
                        officeActualHours += lateMinutes / 60.0;
                    }
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
                    (checkInTime == null && checkOutTime == null && (isWFHFullday || isWFHMorning || isWFHAfternoon)) ||
                    (checkInTime.HasValue && checkOutTime == null && (isWFHFullday || isWFHMorning || isWFHAfternoon)))
                {
                    double? totalWorkingTime = (officeActualHours ?? 0) + (trackerActualHours ?? 0);
                    double standardWorkHours = 8.0;
                    if (isAfternoonAbsence && user.MorningWorking > 0)
                        standardWorkHours = user.MorningWorking;
                    else if (isMorningAbsence && user.AfternoonWorking > 0)
                        standardWorkHours = user.AfternoonWorking;

                    double requiredHours = 0.0;
                    bool useWfhThreshold = (isWFHFullday || isWFHMorning || isWFHAfternoon) ||
                       (!(isWFHFullday || isWFHMorning || isWFHAfternoon) && officeActualHours.HasValue && !checkOutTime.HasValue);
                    if (isWFHFullday)
                        requiredHours = standardWorkHours * wfhThreshold;
                    else if (isWFHMorning && user.MorningWorking > 0)
                        requiredHours = user.MorningWorking * wfhThreshold;
                    else if (isWFHAfternoon && user.AfternoonWorking > 0)
                        requiredHours = user.AfternoonWorking * wfhThreshold;
                    else
                        requiredHours = useWfhThreshold ? standardWorkHours * wfhThreshold : standardWorkHours;
                    if (tardinessHour > 0 || leaveEarlyHour > 0)
                        requiredHours -= (tardinessHour + leaveEarlyHour);

                    bool isTimeViolation = ((isWFHFullday || isWFHMorning || isWFHAfternoon) && trackerActualHours.HasValue && trackerActualHours.Value < requiredHours) ||
                      (!(isWFHFullday || isWFHMorning || isWFHAfternoon) && officeActualHours.HasValue && officeActualHours.Value < requiredHours);

                    bool isZeroTimeViolation = ((isWFHFullday || isWFHMorning || isWFHAfternoon) && trackerActualHours == 0.0) ||
                                               (!(isWFHFullday || isWFHMorning || isWFHAfternoon) && officeActualHours == 0.0);

                    if (isTimeViolation || isZeroTimeViolation)
                    {
                        string notes = isZeroTimeViolation ? "No tracker time for approved WFH" : "No early leave/late arrival approval";
                        ViolationStatus violationType = isZeroTimeViolation ? ViolationStatus.DatesNoTrackerTime : ViolationStatus.DatesBelowThreshold;
                        var anomalyInput = new AnomalyInputDto
                        {
                            UserId = tk.UserId.Value,
                            EmployeeName = user.FullName,
                            UserName = user.UserName,
                            Date = tk.DateAt.ToString("dd/MM/yyyy"),
                            TotalWorkingTime = totalWorkingTime,
                            Notes = notes,
                            IsYesterdayInFunction = input.IsYesterday,
                            ViolationType = violationType,
                            Branch = input.Branch
                        };
                        AddOrUpdateAnomaly(anomalyInput, yesterdayAnomalies, lastWeekAnomalies);
                    }

                    if (isWFHMorning && isMorningAbsence)
                    {
                        double afternoonRequiredHours = user.AfternoonWorking - tardinessHour - leaveEarlyHour;
                        bool isAfternoonWorkingTimeViolation = (officeActualHours ?? 0) < afternoonRequiredHours;
                        if (isAfternoonWorkingTimeViolation)
                        {
                            var anomalyInput = new AnomalyInputDto
                            {
                                UserId = tk.UserId.Value,
                                EmployeeName = user.FullName,
                                UserName = user.UserName,
                                Date = tk.DateAt.ToString("dd/MM/yyyy"),
                                TotalWorkingTime = officeActualHours,
                                Notes = "No early leave/late arrival approval",
                                IsYesterdayInFunction = input.IsYesterday,
                                ViolationType = ViolationStatus.DatesBelowThreshold,
                                Branch = input.Branch
                            };
                            AddOrUpdateAnomaly(anomalyInput, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }

                    if (isWFHAfternoon && isAfternoonAbsence)
                    {
                        double morningRequiredHours = user.MorningWorking - tardinessHour - leaveEarlyHour;
                        bool isMorningWorkingTimeViolation = (officeActualHours ?? 0) < morningRequiredHours;
                        if (isMorningWorkingTimeViolation)
                        {
                            var anomalyInput = new AnomalyInputDto
                            {
                                UserId = tk.UserId.Value,
                                EmployeeName = user.FullName,
                                UserName = user.UserName,
                                Date = tk.DateAt.ToString("dd/MM/yyyy"),
                                TotalWorkingTime = officeActualHours,
                                Notes = "No early leave/late arrival approval",
                                IsYesterdayInFunction = input.IsYesterday,
                                ViolationType = ViolationStatus.DatesBelowThreshold,
                                Branch = input.Branch
                            };
                            AddOrUpdateAnomaly(anomalyInput, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }
                }

                if (isFullDayAbsence || isMorningAbsence || isAfternoonAbsence)
                {
                    isValidAbsence = IsValidAbsenceForCase(userAbsenceRequests, tk.DateAt, isFullDayAbsence, isMorningAbsence, isAfternoonAbsence,
                        absenceDetailDict);

                    if (!isValidAbsence)
                    {
                        var anomalyInput = new AnomalyInputDto
                        {
                            UserId = tk.UserId.Value,
                            EmployeeName = user.FullName,
                            UserName = user.UserName,
                            Date = tk.DateAt.ToString("dd/MM/yyyy"),
                            TotalWorkingTime = officeActualHours,
                            Notes = "No leave/WFH record",
                            IsYesterdayInFunction = input.IsYesterday,
                            ViolationType = ViolationStatus.DatesMissed,
                            Branch = input.Branch
                        };
                        AddOrUpdateAnomaly(anomalyInput, yesterdayAnomalies, lastWeekAnomalies);
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
                .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork &&
                            u.BranchId.HasValue && branchIds.Contains(u.BranchId.Value))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    UserName = u.UserName,
                    BranchId = u.BranchId.Value,
                    MorningStartAt = u.MorningStartAt,
                    MorningEndAt = u.MorningEndAt,
                    AfternoonStartAt = u.AfternoonStartAt,
                    AfternoonEndAt = u.AfternoonEndAt,
                    MorningWorking = (double)u.MorningWorking,
                    AfternoonWorking = (double)u.AfternoonWorking
                })
                .ToListAsync();

            var allTimekeepings = await _workScope.GetAll<Timekeeping>()
                .Where(t => t.User.BranchId.HasValue && branchIds.Contains(t.User.BranchId.Value) &&
                            t.DateAt >= startDate && t.DateAt <= endDate &&
                            t.DateAt.DayOfWeek != DayOfWeek.Saturday)
                .Select(t => new TimekeepingDto
                {
                    UserId = t.UserId,
                    DateAt = t.DateAt,
                    CheckIn = t.CheckIn,
                    CheckOut = t.CheckOut,
                    TrackerTime = t.TrackerTime
                })
                .ToListAsync();

            var absenceData = await _workScope.GetAll<AbsenceDayDetail>()
                .Where(d => branchIds.Contains(d.Request.User.BranchId.Value) &&
                            !d.Request.IsDeleted &&
                            d.DateAt >= startDate && d.DateAt <= endDate)
                .Select(d => new
                {
                    Detail = new AbsenceDetailDto
                    {
                        RequestId = d.RequestId,
                        DateAt = d.DateAt.Date,
                        DateType = d.DateType,
                        AbsenceTime = d.AbsenceTime,
                        Hour = d.Hour,
                        RequestStatus = d.Request.Status,
                        RequestType = d.Request.Type,
                        UserId = d.Request.UserId
                    },
                    Request = new AbsenceRequestDto
                    {
                        Id = d.Request.Id,
                        UserId = d.Request.UserId,
                        Status = d.Request.Status,
                        Type = d.Request.Type
                    }
                })
                .ToListAsync();

            var allAbsenceDetails = absenceData.Select(x => x.Detail).ToList();
            var allAbsenceRequests = absenceData
                .GroupBy(x => x.Request.Id)
                .Select(g => new AbsenceRequestDto
                {
                    Id = g.Key,
                    UserId = g.First().Request.UserId,
                    Status = g.First().Request.Status,
                    Type = g.First().Request.Type
                })
                .ToList();

            return new AnomalyData
            {
                Users = allUsers,
                Timekeepings = allTimekeepings,
                AbsenceRequests = allAbsenceRequests,
                AbsenceDetails = allAbsenceDetails
            };
        }
        public async Task<AnomaliesTimelogReportDto> GetAnomaliesTimelogReport(GetAnomaliesTimelogReportInput input)
        {
            var now = DateTimeUtils.GetNow().Date;
            var yesterday = now.AddDays(-1);
            var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);

            var selectedBranches = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Where(b => input.BranchIds == null ||
                        input.BranchIds.Count == 0 ||
                        input.BranchIds.Contains(b.Id))
                .Select(b => new { b.Id, b.Code, b.Color })
                .AsNoTracking()
                .ToListAsync();

            var branchDict = selectedBranches.ToDictionary(b => b.Id, b => new BranchToDisplayDto { BranchName = b.Code, BranchColor = b.Color });
            var validBranchIds = selectedBranches.Select(b => b.Id).ToList();

            var data = await LoadAnomalyDataAsync(validBranchIds, lastWeekStart, yesterday.AddDays(1).AddTicks(-1));

            var result = new AnomaliesTimelogReportDto
            {
                YesterdayAnomalies = new List<YesterdayAnomalyDTO>(),
                LastWeekAnomalies = new List<LastWeekAnomalyDTO>()
            };

            foreach (var branchId in validBranchIds)
            {
                var branch = branchDict[branchId];
                var processAnomaliesInput = new ProcessAnomaliesInputDto
                {
                    AllUsers = data.Users,
                    AllTimekeepings = data.Timekeepings,
                    AllAbsenceRequests = data.AbsenceRequests,
                    AllAbsenceDetails = data.AbsenceDetails,
                    BranchId = branchId,
                    StartDate = yesterday,
                    EndDate = yesterday.AddDays(1).AddSeconds(-1),
                    IsYesterday = true,
                    Branch = branch
                };
                var (yesterdayAnomalies, _) = await ProcessAnomalies(processAnomaliesInput);
                result.YesterdayAnomalies.AddRange(yesterdayAnomalies);
            }

            var lastWeekLoopStopwatch = Stopwatch.StartNew();
            foreach (var branchId in validBranchIds)
            {
                var branch = branchDict[branchId];
                var processAnomaliesInput = new ProcessAnomaliesInputDto
                {
                    AllUsers = data.Users,
                    AllTimekeepings = data.Timekeepings,
                    AllAbsenceRequests = data.AbsenceRequests,
                    AllAbsenceDetails = data.AbsenceDetails,
                    BranchId = branchId,
                    StartDate = lastWeekStart,
                    EndDate = lastWeekEnd,
                    IsYesterday = false,
                    Branch = branch
                };
                var (_, lastWeekAnomalies) = await ProcessAnomalies(processAnomaliesInput);
                result.LastWeekAnomalies.AddRange(lastWeekAnomalies);
            }
            lastWeekLoopStopwatch.Stop();

            result.LastWeekAnomalies = result.LastWeekAnomalies
                .OrderByDescending(a => a.Count)
                .ThenBy(a => a.EmployeeName)
                .ToList();

            return result;
        }

        private static (DateTime start, DateTime end) GetLastWeekRange(DateTime reportDate)
        {
            var thisWeekStart = DateTimeUtils.FirstDayOfWeek(reportDate);   
            var lwStart = thisWeekStart.AddDays(-7).Date;
            var lwEnd = thisWeekStart.AddDays(-1).Date.AddDays(1).AddTicks(-1);
            return (lwStart, lwEnd);
        }

        private async Task<object> GetBranchDataAsync(string branchName, DateTime? startDate = null, DateTime? endDate = null, string mode = "Branch")
        {
            var branch = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Where(b => b.Name == branchName)
                .FirstOrDefaultAsync();

            if (branch == null)
            {
                throw new ArgumentException($"Branch with name '{branchName}' not found.");
            }

            BranchToDisplayDto branchDto = new BranchToDisplayDto { BranchName = branch.Name, BranchColor = branch.Color };

            var data = await LoadAnomalyDataAsync(new List<long> { branch.Id }, (DateTime) startDate, (DateTime) endDate);

            if (mode == "Yesterday")
            {
                var processAnomaliesInput = new ProcessAnomaliesInputDto
                {
                    AllUsers = data.Users,
                    AllTimekeepings = data.Timekeepings,
                    AllAbsenceRequests = data.AbsenceRequests,
                    AllAbsenceDetails = data.AbsenceDetails,
                    BranchId = branch.Id,
                    StartDate = (DateTime)startDate,
                    EndDate = (DateTime)endDate,
                    IsYesterday = true,
                    Branch = branchDto
                };
                var resultTuple = await ProcessAnomalies(processAnomaliesInput);
                return resultTuple.yesterdayAnomalies;
            }
            else if (mode == "LastWeek")
            {
                var processAnomaliesInput = new ProcessAnomaliesInputDto
                {
                    AllUsers = data.Users,
                    AllTimekeepings = data.Timekeepings,
                    AllAbsenceRequests = data.AbsenceRequests,
                    AllAbsenceDetails = data.AbsenceDetails,
                    BranchId = branch.Id,
                    StartDate = (DateTime)startDate,
                    EndDate = (DateTime)endDate,
                    IsYesterday = false,
                    Branch = branchDto
                };
                var resultTuple = await ProcessAnomalies(processAnomaliesInput);
                return resultTuple.lastWeekAnomalies;
            }
            else
            {
                return new { branch.Id, branch.Code, branch.Name };
            }
        }

        public async Task<List<YesterdayAnomalyDTO>> GetYesterdayAnomalies(string branchName, DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddSeconds(-1);
            var result = await GetBranchDataAsync(branchName, startDate, endDate, "Yesterday");
            return (List<YesterdayAnomalyDTO>)result;
        }

        public async Task<List<LastWeekAnomalyDTO>> GetLastWeekAnomalies(string branchName, DateTime startDate, DateTime endDate)
        {
            var result = await GetBranchDataAsync(branchName, startDate, endDate, "LastWeek");
            return (List<LastWeekAnomalyDTO>)result;
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
                        DateTime lastMonday = now.AddDays(-(int)now.DayOfWeek - 6).Date;
                        DateTime lastWeekEnd = lastMonday.AddDays(5).Date.AddSeconds(-1);
                        await SendSmartWeeklyReport(input.botUri, branchName, lastMonday, lastWeekEnd);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing branch {branchName}: {ex.Message}");
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

            await SendDailyHeaderMessage(botUri, branchName, reportDate);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, branchName, "Yesterday – Unplanned Absences",
                unplannedAbsences, "No leave/WFH record", true, 2, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, branchName, "Yesterday – Unapproved Short Working Hours",
                unapprovedShortHours, "No early leave/late arrival approval", true, 3, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, branchName, "Yesterday – Missing Tracker Time For Approved WFH",
                noTrackerTime, "No tracker time for approved WFH", true, 4, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);
        }

        private async Task SendSmartWeeklyReport(string botUri, string branchName, DateTime startDate, DateTime endDate)
        {
            const int BATCH_SIZE = 5;

            var allAnomalies = await GetLastWeekAnomalies(branchName, startDate, endDate);

            var unplannedAbsences = allAnomalies.Where(a => a.DatesMissed.Any()).ToList();
            var unapprovedShortHours = allAnomalies.Where(a => a.DatesBelowThreshold.Any()).ToList();
            var noTrackerTime = allAnomalies.Where(a => a.DatesNoTrackerTime.Any()).ToList();

            await SendWeeklyHeaderMessage(botUri, branchName, startDate, endDate);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, branchName, "Last Week – Unplanned Absences",
                unplannedAbsences, "No leave/WFH record", false, 2, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, branchName, "Last Week – Unapproved Short Working Hours",
                unapprovedShortHours, "No early leave/late arrival approval", false, 3, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);

            await SendAnomalyMessages(botUri, branchName, "Last Week – Missing Tracker Time For Approved WFH",
                noTrackerTime, "No tracker time for approved WFH", false, 4, BATCH_SIZE);
            await Task.Delay(MESSAGE_DELAY_MS);
        }

        private async Task SendAnomalyMessages<T>(string botUri, string branchName, string sectionTitle,
            List<T> anomalies, string notes, bool isYesterday, int basePartNumber, int batchSize)
        {
            if (!anomalies.Any())
            {
                await SendEmptySectionMessage(botUri, branchName, sectionTitle, isYesterday, basePartNumber);
                return;
            }

            var chunks = SplitAnomaliesIntoChunks(anomalies, batchSize, isYesterday, notes);
            int globalIndex = 1;

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                int partNumber = basePartNumber + i;
                bool isFirstChunk = i == 0;
                bool isLastChunk = i == chunks.Count - 1;

                string chunkSectionTitle = isFirstChunk ? sectionTitle : null;
                SendAnomalyChunkMessage(botUri, branchName, chunkSectionTitle, chunk.anomalies,
                    notes, isYesterday, partNumber, isFirstChunk, isLastChunk, ref globalIndex);
                await Task.Delay(MESSAGE_DELAY_MS);
            }
        }

        private List<(List<T> anomalies, int estimatedLength)> SplitAnomaliesIntoChunks<T>(
            List<T> anomalies, int batchSize, bool isYesterday, string notes)
        {
            var chunks = new List<(List<T>, int)>();

            for (int i = 0; i < anomalies.Count; i += batchSize)
            {
                var chunk = anomalies.Skip(i).Take(batchSize).ToList();
                int estimatedLength = chunk.Sum(a => EstimateAnomalyLength(a, isYesterday, notes));
                chunks.Add((chunk, estimatedLength));
            }

            return chunks;
        }

        private int EstimateAnomalyLength<T>(T anomaly, bool isYesterday, string notes)
        {
            if (isYesterday)
            {
                var yesterdayAnomaly = (YesterdayAnomalyDTO)(object)anomaly;
                return 200 + yesterdayAnomaly.EmployeeName.Length + notes.Length + 50;
            }
            else
            {
                var weekAnomaly = (LastWeekAnomalyDTO)(object)anomaly;
                int datesLength;
                switch (notes)
                {
                    case "No leave/WFH record":
                        datesLength = string.Join(", ", weekAnomaly.DatesMissed).Length;
                        break;
                    case "No early leave/late arrival approval":
                        datesLength = string.Join(", ", weekAnomaly.DatesBelowThreshold).Length;
                        break;
                    default:
                        datesLength = string.Join(", ", weekAnomaly.DatesNoTrackerTime).Length;
                        break;
                }
                return 250 + weekAnomaly.EmployeeName.Length + datesLength + notes.Length;
            }
        }

        private async Task SendDailyHeaderMessage(string botUri, string branchName, DateTime reportDate)
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

        private async Task SendWeeklyHeaderMessage(string botUri, string branchName, DateTime startDate, DateTime endDate)
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

        private void SendAnomalyChunkMessage<T>(string botUri, string branchName, string sectionTitle,
            List<T> chunkAnomalies, string notes, bool isYesterday, int partNumber,
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

        private async Task SendEmptySectionMessage(string botUri, string branchName, string sectionTitle,
            bool isYesterday, int partNumber)
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
