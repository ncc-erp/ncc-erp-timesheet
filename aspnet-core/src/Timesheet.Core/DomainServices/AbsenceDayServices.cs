using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.IoC;
using System;
using System.Collections.Generic;
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
    public class AbsenceDayService : IAbsenceDayService, ITransientDependency
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private const int MESSAGE_DELAY_MS = 1000;
        public AbsenceDayService(IWorkScope workScope, MezonService mezonService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
        }

        private TimeSpan? ParseTimeSpan(string input)
        {
            return TimeSpan.TryParse(input, out var result) ? result : (TimeSpan?)null;
        }

        private bool IsWFHForDayType(dynamic[] userAbsenceRequests, (long RequestId, DateTime DateAt) absenceDetailDictKey, DayType dayType, Dictionary<(long RequestId, DateTime DateAt), List<dynamic>> absenceDetailDict)
        {
            return userAbsenceRequests.Any(r => r.Type == RequestType.Remote && r.Status == RequestStatus.Approved
                && absenceDetailDict.ContainsKey((r.Id, absenceDetailDictKey.DateAt))
                && absenceDetailDict[(r.Id, absenceDetailDictKey.DateAt)].Any(d => d.DateType == dayType));
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

        private void AddOrUpdateAnomaly(long userId, string employeeName, string date, double? totalWorkingTime, string notes, bool isYesterdayInFunction,
            List<YesterdayAnomalyDTO> yesterday, List<LastWeekAnomalyDTO> lastWeek, string violationType = "DatesBelowThreshold", string branchCode = null)
        {
            if (isYesterdayInFunction)
            {
                var yesterdayDto = new YesterdayAnomalyDTO
                {
                    UserId = userId,
                    EmployeeName = employeeName,
                    Date = date,
                    ActualHours = totalWorkingTime.HasValue ? $"{totalWorkingTime.Value.ToString("F2", CultureInfo.InvariantCulture)}h" : "0h",
                    Notes = notes,
                    Branch = branchCode
                };
                yesterday.Add(yesterdayDto);
            }
            else
            {
                var lastWeekDto = lastWeek.FirstOrDefault(a => a.UserId == userId) ?? new LastWeekAnomalyDTO { UserId = userId, EmployeeName = employeeName, Branch = branchCode };
                if (violationType == "DatesMissed")
                    lastWeekDto.DatesMissed.Add(date);
                else if (violationType == "DatesNoTrackerTime")
                    lastWeekDto.DatesNoTrackerTime.Add(date);
                else
                    lastWeekDto.DatesBelowThreshold.Add(date);
                lastWeekDto.Count++;
                lastWeekDto.Notes = notes;
                if (!lastWeek.Any(a => a.UserId == userId))
                    lastWeek.Add(lastWeekDto);
            }
        }

        private bool IsValidAbsenceForCase(dynamic[] userAbsenceRequests, (long RequestId, DateTime DateAt) absenceDetailDictKey,
            bool isFullDayAbsence, bool isMorningAbsence, bool isAfternoonAbsence, bool isMorningPresentAfternoonAbsent, bool isAfternoonPresentMorningAbsent,
            Dictionary<(long RequestId, DateTime DateAt), List<dynamic>> absenceDetailList)
        {
            var relevantRequests = userAbsenceRequests
                .Where(r => absenceDetailList.ContainsKey((r.Id, absenceDetailDictKey.DateAt)))
                .ToList();

            bool hasMorningRequest = false, hasAfternoonRequest = false;
            bool hasMorningApproved = false, hasAfternoonApproved = false;
            bool hasMorningRemote = false, hasAfternoonRemote = false;
            bool hasMorningOffOnsite = false, hasAfternoonOffOnsite = false;

            foreach (var request in relevantRequests)
            {
                if (!absenceDetailList.ContainsKey((request.Id, absenceDetailDictKey.DateAt)))
                    continue;

                var details = absenceDetailList[(request.Id, absenceDetailDictKey.DateAt)];
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
                        if ((isMorningAbsence || isAfternoonPresentMorningAbsent) && detail.DateType == DayType.Morning)
                            return true;
                        if ((isAfternoonAbsence || isMorningPresentAfternoonAbsent) && detail.DateType == DayType.Afternoon)
                            return true;
                        if ((isMorningAbsence || isAfternoonAbsence) && detail.DateType == DayType.Fullday && request.Type == RequestType.Remote)
                            return true;
                        if (isFullDayAbsence && request.Type == RequestType.Remote)
                            return true;
                        if (isFullDayAbsence && (detail.DateType == DayType.Morning || detail.DateType == DayType.Afternoon))
                        {
                            var otherDetail = details.FirstOrDefault(d => d.DateType != detail.DateType && (d.DateType == DayType.Morning || d.DateType == DayType.Afternoon));
                            if (otherDetail != null)
                                return true;
                        }

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
                ((hasMorningOffOnsite && hasAfternoonRemote) || (hasAfternoonOffOnsite && hasMorningRemote)) &&
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

        private async Task<(List<YesterdayAnomalyDTO> yesterdayAnomalies, List<LastWeekAnomalyDTO> lastWeekAnomalies)> ProcessAnomalies(
            long branchId, DateTime startDate, DateTime endDate, bool isYesterday, string branchCode)
        {
            var activeUsers = await _workScope.GetAll<User>()
                .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork && u.BranchId == branchId)
                .Select(u => new { u.Id, u.FullName, u.BranchId, BranchName = u.Branch.Name, u.MorningStartAt, u.MorningEndAt, u.AfternoonStartAt, u.AfternoonEndAt, u.MorningWorking, u.AfternoonWorking })
                .ToListAsync();

            var userIds = activeUsers.Select(u => u.Id).ToList();

            var timekeepings = await _workScope.GetAll<Timekeeping>()
                .Where(t => userIds.Contains((long)t.UserId)
                    && t.DateAt >= startDate && t.DateAt <= endDate
                    && t.DateAt.DayOfWeek != DayOfWeek.Saturday)
                .Select(t => new { t.UserId, t.DateAt, t.CheckIn, t.CheckOut, t.TrackerTime })
                .ToListAsync();

            var absenceRequests = await _workScope.GetAll<AbsenceDayRequest>()
                .Where(r => userIds.Contains(r.UserId) && r.IsDeleted == false)
                .Select(r => new { r.Id, r.UserId, r.Status, r.Type, r.IsDeleted })
                .ToListAsync();

            var absenceRequestDict = absenceRequests
                .GroupBy(r => r.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Cast<dynamic>().ToList()
                );

            var absenceDetails = await _workScope.GetAll<AbsenceDayDetail>()
                .Where(d => absenceRequests.Select(r => r.Id).Contains(d.RequestId) && d.DateAt >= startDate && d.DateAt <= endDate)
                .Select(d => new { d.RequestId, d.DateAt, d.DateType, d.AbsenceTime, d.Hour })
                .ToListAsync();

            var absenceDetailDict = absenceDetails
                .GroupBy(d => (d.RequestId, d.DateAt))
                .ToDictionary(
                    g => g.Key,
                    g => g.Cast<dynamic>().ToList()
                );

            var yesterdayAnomalies = new List<YesterdayAnomalyDTO>();
            var lastWeekAnomalies = new List<LastWeekAnomalyDTO>();
            const double breakTime = 1.0;
            const double wfhThreshold = 0.85;
            const double gracePeriodMinutes = 15.0;

            foreach (var tk in timekeepings)
            {
                var user = activeUsers.FirstOrDefault(u => u.Id == tk.UserId);
                if (user == null) continue;

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

                var userAbsenceRequests = tk.UserId.HasValue && absenceRequestDict.ContainsKey(tk.UserId.Value)
                                ? absenceRequestDict[tk.UserId.Value].ToArray()
                                : Array.Empty<dynamic>();

                bool isWFHFullday = IsWFHForDayType(userAbsenceRequests, (0, tk.DateAt), DayType.Fullday, absenceDetailDict);
                bool isWFHMorning = IsWFHForDayType(userAbsenceRequests, (0, tk.DateAt), DayType.Morning, absenceDetailDict);
                bool isWFHAfternoon = IsWFHForDayType(userAbsenceRequests, (0, tk.DateAt), DayType.Afternoon, absenceDetailDict);

                bool isFullDayAbsence = tk.CheckIn == null && tk.CheckOut == null;
                bool isMorningAbsence = checkInTime.HasValue && morningEndAt.HasValue && checkInTime > morningEndAt;
                bool isAfternoonAbsence = checkInTime.HasValue && !isWFHFullday && !isWFHAfternoon &&
                    (!checkOutTime.HasValue || (afternoonStartAt.HasValue && checkOutTime.HasValue && checkOutTime < afternoonStartAt));
                bool isMorningPresentAfternoonAbsent = checkInTime.HasValue && morningStartAt.HasValue && morningEndAt.HasValue && afternoonStartAt.HasValue
                    && checkInTime < morningEndAt && checkOutTime.HasValue && checkOutTime < afternoonStartAt;
                bool isAfternoonPresentMorningAbsent = checkOutTime.HasValue && afternoonStartAt.HasValue && afternoonEndAt.HasValue && morningEndAt.HasValue
                    && checkOutTime > afternoonEndAt && checkInTime.HasValue && checkInTime > morningEndAt;

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
                    if (!(isMorningPresentAfternoonAbsent || isAfternoonPresentMorningAbsent))
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
                    if (!(isMorningPresentAfternoonAbsent || isAfternoonPresentMorningAbsent))
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
                foreach (var request in userAbsenceRequests)
                {
                    if (absenceDetailDict.ContainsKey((request.Id, tk.DateAt)))
                    {
                        var details = absenceDetailDict[(request.Id, tk.DateAt)];
                        foreach (var detail in details)
                        {
                            if (detail.AbsenceTime == OnDayType.DiMuon && detail.Hour > 0)
                                tardinessHour += detail.Hour;
                            if (detail.AbsenceTime == OnDayType.VeSom && detail.Hour > 0)
                                leaveEarlyHour += detail.Hour;
                        }
                    }
                }

                if ((checkInTime.HasValue && checkOutTime.HasValue && !isFullDayAbsence) ||
                    (checkInTime == null && checkOutTime == null && (isWFHFullday || isWFHMorning || isWFHAfternoon)) ||
                    (checkInTime.HasValue && checkOutTime == null && (isWFHFullday || isWFHMorning || isWFHAfternoon)))
                {
                    double? totalWorkingTime = (officeActualHours ?? 0) + (trackerActualHours ?? 0);
                    double standardWorkHours = 8.0;
                    if (isMorningPresentAfternoonAbsent && user.MorningWorking.HasValue)
                        standardWorkHours = user.MorningWorking.Value;
                    else if (isAfternoonPresentMorningAbsent && user.AfternoonWorking.HasValue)
                        standardWorkHours = user.AfternoonWorking.Value;

                    double requiredHours = 0.0;
                    bool useWfhThreshold = (isWFHFullday || isWFHMorning || isWFHAfternoon) ||
                       (!(isWFHFullday || isWFHMorning || isWFHAfternoon) && officeActualHours.HasValue && !checkOutTime.HasValue);
                    if (isWFHFullday)
                        requiredHours = standardWorkHours * wfhThreshold;
                    else if (isWFHMorning && user.MorningWorking.HasValue)
                        requiredHours = user.MorningWorking.Value * wfhThreshold;
                    else if (isWFHAfternoon && user.AfternoonWorking.HasValue)
                        requiredHours = user.AfternoonWorking.Value * wfhThreshold;
                    else
                        requiredHours = useWfhThreshold ? standardWorkHours * wfhThreshold : standardWorkHours;
                    if (tardinessHour > 0 || leaveEarlyHour > 0)
                        requiredHours -= (tardinessHour + leaveEarlyHour);

                    bool isMorningOfficeAfternoonWFH = false;
                    var userRequestAfternoonWFH = userAbsenceRequests.FirstOrDefault(r => r.Status == RequestStatus.Approved);
                    if (userRequestAfternoonWFH != null && absenceDetailDict.ContainsKey((userRequestAfternoonWFH.Id, tk.DateAt))
                        && absenceDetailDict[(userRequestAfternoonWFH.Id, tk.DateAt)].Any(d => d.DateType == DayType.Afternoon)
                        && timekeepings.Any(t => t.UserId == tk.UserId && TimeSpan.TryParse(t.CheckIn, out var checkInMorning) && TimeSpan.TryParse(t.CheckOut, out var checkOutMorning)
                            && checkInMorning < morningEndAt && checkOutMorning < afternoonStartAt && !string.IsNullOrEmpty(t.TrackerTime) && double.TryParse(t.TrackerTime, out var tr) && tr > 0))
                    {
                        isMorningOfficeAfternoonWFH = true;
                        double morningRequiredHours = (user.MorningWorking ?? 0) - tardinessHour - leaveEarlyHour;
                        double afternoonRequiredTrackerHours = (user.AfternoonWorking ?? 0) * wfhThreshold - tardinessHour - leaveEarlyHour;
                        if ((officeActualHours ?? 0) < morningRequiredHours || (trackerTimeHours.HasValue && trackerTimeHours.Value < afternoonRequiredTrackerHours))
                        {
                            AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("dd/MM/yyyy"), totalWorkingTime,
                                "Violation: Morning office, Afternoon WFH time mismatch", isYesterday, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }

                    bool isMorningWFHAfternoonOffice = false;
                    var userRequestMorningWFH = userAbsenceRequests.FirstOrDefault(r => r.Status == RequestStatus.Approved);
                    if (userRequestMorningWFH != null && absenceDetailDict.ContainsKey((userRequestMorningWFH.Id, tk.DateAt))
                        && absenceDetailDict[(userRequestMorningWFH.Id, tk.DateAt)].Any(d => d.DateType == DayType.Morning)
                        && timekeepings.Any(t => t.UserId == tk.UserId && TimeSpan.TryParse(t.CheckIn, out var checkInAfternoon) && TimeSpan.TryParse(t.CheckOut, out var checkOutAfternoon)
                            && checkOutAfternoon > afternoonEndAt && checkInAfternoon > morningEndAt && !string.IsNullOrEmpty(t.TrackerTime) && double.TryParse(t.TrackerTime, out var tr) && tr > 0))
                    {
                        isMorningWFHAfternoonOffice = true;
                        double afternoonRequiredHours = (user.AfternoonWorking ?? 0) - tardinessHour - leaveEarlyHour;
                        double morningRequiredTrackerHours = (user.MorningWorking ?? 0) * wfhThreshold - tardinessHour - leaveEarlyHour;
                        if ((officeActualHours ?? 0) < afternoonRequiredHours || (trackerTimeHours.HasValue && trackerTimeHours.Value < morningRequiredTrackerHours))
                        {
                            AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("dd/MM/yyyy"), totalWorkingTime,
                                "Violation: Morning WFH, Afternoon office time mismatch", isYesterday, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }

                    bool isTimeViolation = ((isWFHFullday || isWFHMorning || isWFHAfternoon) && trackerActualHours.HasValue && trackerActualHours.Value < requiredHours) ||
                      (!(isWFHFullday || isWFHMorning || isWFHAfternoon) && officeActualHours.HasValue && officeActualHours.Value < requiredHours) &&
                      !isMorningOfficeAfternoonWFH && !isMorningWFHAfternoonOffice;

                    bool isZeroTimeViolation = ((isWFHFullday || isWFHMorning || isWFHAfternoon) && trackerActualHours == 0.0) ||
                                               (!(isWFHFullday || isWFHMorning || isWFHAfternoon) && officeActualHours == 0.0);

                    if (isTimeViolation || isZeroTimeViolation)
                    {
                        string notes = isZeroTimeViolation ? "No tracker time for approved WFH" : "No early leave/late arrival approval";
                        string violationType = isZeroTimeViolation ? "DatesNoTrackerTime" : "DatesBelowThreshold";
                        AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("dd/MM/yyyy"), totalWorkingTime,
                            notes, isYesterday, yesterdayAnomalies, lastWeekAnomalies, violationType, branchCode);
                    }

                    if (isWFHMorning && isAfternoonPresentMorningAbsent && !isMorningOfficeAfternoonWFH && !isMorningWFHAfternoonOffice)
                    {
                        double afternoonRequiredHours = (user.AfternoonWorking ?? 0) - tardinessHour - leaveEarlyHour;
                        bool isAfternoonWorkingTimeViolation = (officeActualHours ?? 0) < afternoonRequiredHours;
                        if (isAfternoonWorkingTimeViolation)
                        {
                            AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("dd/MM/yyyy"), officeActualHours,
                                "No early leave/late arrival approval", isYesterday, yesterdayAnomalies, lastWeekAnomalies, "DatesBelowThreshold", branchCode);
                        }
                    }

                    if (isWFHAfternoon && isMorningPresentAfternoonAbsent && !isMorningOfficeAfternoonWFH && !isMorningWFHAfternoonOffice)
                    {
                        double morningRequiredHours = (user.MorningWorking ?? 0) - tardinessHour - leaveEarlyHour;
                        bool isMorningWorkingTimeViolation = (officeActualHours ?? 0) < morningRequiredHours;
                        if (isMorningWorkingTimeViolation)
                        {
                            AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("dd/MM/yyyy"), officeActualHours,
                                "No early leave/late arrival approval", isYesterday, yesterdayAnomalies, lastWeekAnomalies, "DatesBelowThreshold", branchCode);
                        }
                    }
                }

                if (isFullDayAbsence || isMorningAbsence || isAfternoonAbsence || isMorningPresentAfternoonAbsent || isAfternoonPresentMorningAbsent)
                {
                    isValidAbsence = IsValidAbsenceForCase(userAbsenceRequests, (0, tk.DateAt), isFullDayAbsence, isMorningAbsence, isAfternoonAbsence,
                        isMorningPresentAfternoonAbsent, isAfternoonPresentMorningAbsent, absenceDetailDict);

                    if (!isValidAbsence)
                    {
                        AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("dd/MM/yyyy"), officeActualHours,
                            "No leave/WFH record", isYesterday, yesterdayAnomalies, lastWeekAnomalies, "DatesMissed", branchCode);
                    }
                }
            }

            SortAnomalyDates(lastWeekAnomalies);
            yesterdayAnomalies = yesterdayAnomalies.OrderBy(a => a.EmployeeName).ToList();
            lastWeekAnomalies = lastWeekAnomalies.OrderBy(a => a.EmployeeName).ToList();
            return (yesterdayAnomalies, lastWeekAnomalies);
        }

        public async Task<object> GetAnomaliesTimelogReport(GetAnomaliesTimelogReportRequestDto request)
        {
            var param = request.Param;
            var input = request.Input;

            var now = DateTimeUtils.GetNow().Date;
            var yesterday = now.AddDays(-1);
            var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);

            var allBranches = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Select(b => new { b.Id, b.Code })
                .AsNoTracking()
                .ToListAsync();

            var branchDict = allBranches.ToDictionary(b => b.Id, b => b.Code);
            var validBranchIds = new List<long>();
            List<long> branchIds;

            if (input.BranchIds == null || !input.BranchIds.Any())
            {
                validBranchIds = allBranches.Select(b => b.Id).ToList();
            }
            else
            {
                validBranchIds = input.BranchIds
                    .Where(id => branchDict.ContainsKey(id))
                    .ToList();

                if (!validBranchIds.Any())
                {
                    throw new UserFriendlyException("No valid branch codes provided.");
                }
            }

            var yesterdayAnomalies = new List<YesterdayAnomalyDTO>();
            var lastWeekAnomalies = new List<LastWeekAnomalyDTO>();

            foreach (var branchId in validBranchIds)
            {
                var branchCode = branchDict[branchId];
                var (yest, _) = await ProcessAnomalies(branchId, yesterday, yesterday.AddDays(1).AddSeconds(-1), true, branchCode);
                yesterdayAnomalies.AddRange(yest);
            }

            foreach (var branchId in validBranchIds)
            {
                var branchCode = branchDict[branchId];
                var (_, lastWeek) = await ProcessAnomalies(branchId, lastWeekStart, lastWeekEnd, false, branchCode);
                lastWeekAnomalies.AddRange(lastWeek);
            }

            lastWeekAnomalies = lastWeekAnomalies
                .OrderByDescending(a => a.Count)
                .ThenBy(a => a.EmployeeName)
                .ToList();

            var yesterdayQuery = yesterdayAnomalies.AsQueryable();
            var yesterdayTotal = yesterdayQuery.Count();
            var yesterdayItems = yesterdayQuery
                .Skip(param.SkipCount)
                .Take(param.MaxResultCount)
                .ToList();

            var lastWeekQuery = lastWeekAnomalies.AsQueryable();
            var lastWeekTotal = lastWeekQuery.Count();
            var lastWeekItems = lastWeekQuery
                .Skip(param.SkipCount)
                .Take(param.MaxResultCount)
                .ToList();

            return new
            {
                Yesterday = new PagedYesterdayAnomalyDto
                {
                    Yesterday = yesterday.ToString("dd/MM/yyyy"),
                    TotalCount = yesterdayTotal,
                    Items = yesterdayItems
                },
                LastWeek = new PagedLastWeekAnomalyDto
                {
                    LastWeekStart = lastWeekStart.ToString("dd/MM/yyyy"),
                    LastWeekEnd = lastWeekEnd.ToString("dd/MM/yyyy"),
                    TotalCount = lastWeekTotal,
                    Items = lastWeekItems
                }
            };
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

            switch (mode)
            {
                case "Yesterday":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        throw new ArgumentException("startDate and endDate are required for Yesterday mode.");
                    }
                    var (yesterdayAnomalies, _) = await ProcessAnomalies(branch.Id, startDate.Value, endDate.Value, true, branch.Code);
                    return yesterdayAnomalies;
                case "LastWeek":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        throw new ArgumentException("startDate and endDate are required for LastWeek mode.");
                    }
                    var (_, lastWeekAnomalies) = await ProcessAnomalies(branch.Id, startDate.Value, endDate.Value, false, branch.Code);
                    return lastWeekAnomalies;
                case "Branch":
                default:
                    return branch;
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
                    var branchResult = await GetBranchDataAsync(branchName);
                    var branch = (Timesheet.Entities.Branch)branchResult;
                    if (branch == null)
                    {
                        continue;
                    }

                    if (!isWeekly)
                    {
                        await SendSmartDailyReport(input.botUri, branch.Name, now.AddDays(-1).Date);
                    }
                    else
                    {
                        DateTime lastMonday = now.AddDays(-(int)now.DayOfWeek - 6).Date;
                        DateTime lastWeekEnd = lastMonday.AddDays(5).Date.AddSeconds(-1);
                        await SendSmartWeeklyReport(input.botUri, branch.Name, lastMonday, lastWeekEnd);
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

        private async Task SendFooterMessage(string botUri, string branchName, bool isWeekly, string completionMessage)
        {
            var sb = new StringBuilder();
            var mkList = new List<object>();
            int pos = 0;

            SendMezonMessage(botUri, sb, mkList);
        }
    }
}
