using Abp.Dependency;
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
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Core
{
    public class AbsenceDayServices : IAbsenceDayServices, ITransientDependency
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;

        public AbsenceDayServices(IWorkScope workScope, MezonService mezonService)
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
            List<YesterdayAnomalyDTO> yesterday, List<LastWeekAnomalyDTO> lastWeek, string violationType = "DatesBelowThreshold")
        {
            if (isYesterdayInFunction)
            {
                var yesterdayDto = new YesterdayAnomalyDTO
                {
                    UserId = userId,
                    EmployeeName = employeeName,
                    Date = date,
                    ActualHours = totalWorkingTime.HasValue ? $"{totalWorkingTime.Value.ToString("F2", CultureInfo.InvariantCulture)}h" : "0h",
                    Notes = notes
                };
                yesterday.Add(yesterdayDto);
            }
            else
            {
                var lastWeekDto = lastWeek.FirstOrDefault(a => a.UserId == userId) ?? new LastWeekAnomalyDTO { UserId = userId, EmployeeName = employeeName };
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
            foreach (var request in userAbsenceRequests)
            {
                if (absenceDetailList.ContainsKey((request.Id, absenceDetailDictKey.DateAt)))
                {
                    var detail = absenceDetailList[(request.Id, absenceDetailDictKey.DateAt)].FirstOrDefault();
                    if (detail != null && request.Status == RequestStatus.Approved)
                    {
                        if (isFullDayAbsence && detail.DateType == DayType.Fullday)
                            return true;
                        if (isFullDayAbsence && (detail.DateType == DayType.Morning || detail.DateType == DayType.Afternoon))
                        {
                            var otherDetail = absenceDetailList[(request.Id, absenceDetailDictKey.DateAt)]
                                .FirstOrDefault(d => d.DateType != detail.DateType && (d.DateType == DayType.Morning || d.DateType == DayType.Afternoon));
                            if (otherDetail != null)
                                return true;
                        }
                        if (isMorningAbsence && detail.DateType == DayType.Morning)
                            return true;
                        if (isAfternoonAbsence && detail.DateType == DayType.Afternoon)
                            return true;
                        if (isMorningPresentAfternoonAbsent && request.Type == RequestType.Remote && request.Status == RequestStatus.Approved
                            && detail.DateType == DayType.Afternoon)
                            return true;
                        if (isAfternoonPresentMorningAbsent && request.Type == RequestType.Remote && request.Status == RequestStatus.Approved
                            && detail.DateType == DayType.Morning)
                            return true;
                    }
                    else if (isFullDayAbsence && (request.Type == RequestType.Remote || request.Type == RequestType.Onsite) && request.Status == RequestStatus.Approved)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SortAnomalyDates(List<LastWeekAnomalyDTO> lastWeek)
        {
            foreach (var anomaly in lastWeek)
            {
                anomaly.DatesMissed = anomaly.DatesMissed
                    .OrderBy(date => DateTime.ParseExact(date, "yyyy/MM/dd", CultureInfo.InvariantCulture))
                    .ToList();
                anomaly.DatesNoTrackerTime = anomaly.DatesNoTrackerTime
                    .OrderBy(date => DateTime.ParseExact(date, "yyyy/MM/dd", CultureInfo.InvariantCulture))
                    .ToList();
                anomaly.DatesBelowThreshold = anomaly.DatesBelowThreshold
                    .OrderBy(date => DateTime.ParseExact(date, "yyyy/MM/dd", CultureInfo.InvariantCulture))
                    .ToList();
            }
        }

        private async Task<(List<YesterdayAnomalyDTO> yesterdayAnomalies, List<LastWeekAnomalyDTO> lastWeekAnomalies)> ProcessAnomalies(
            long branchId, DateTime startDate, DateTime endDate, bool isYesterday)
        {
            var activeUsers = await _workScope.GetAll<User>()
                .Where(u => u.IsActive && !u.IsDeleted && u.BranchId == branchId)
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
                .Where(r => userIds.Contains(r.UserId))
                .Select(r => new { r.Id, r.UserId, r.Status, r.Type })
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

                if (!string.IsNullOrEmpty(tk.TrackerTime) && (isWFHFullday || isWFHMorning || isWFHAfternoon))
                {
                    if (TimeSpan.TryParse(tk.TrackerTime, out var trackerTimeSpan))
                    {
                        trackerTimeHours = Math.Round(trackerTimeSpan.TotalHours, 2);
                        double lateMinutes = CalculateLateMinutes(checkInTime, morningStartAt, afternoonStartAt, gracePeriodMinutes);
                        trackerTimeHours += lateMinutes / 60.0;
                        if (isWFHFullday)
                        {
                            trackerTimeHours -= breakTime;
                        }
                    }
                    trackerActualHours = trackerTimeHours > 0 ? trackerTimeHours : 0;
                }

                bool isFullDayAbsence = tk.CheckIn == null && tk.CheckOut == null;
                bool isMorningAbsence = checkInTime.HasValue && morningEndAt.HasValue && checkInTime > morningEndAt;
                bool isAfternoonAbsence = checkInTime.HasValue && (!checkOutTime.HasValue || (afternoonStartAt.HasValue && checkOutTime.HasValue && checkOutTime < afternoonStartAt));
                bool isMorningPresentAfternoonAbsent = checkInTime.HasValue && morningStartAt.HasValue && morningEndAt.HasValue && afternoonStartAt.HasValue
                    && checkInTime < morningEndAt && checkOutTime.HasValue && checkOutTime < afternoonStartAt;
                bool isAfternoonPresentMorningAbsent = checkOutTime.HasValue && afternoonStartAt.HasValue && afternoonEndAt.HasValue && morningEndAt.HasValue
                    && checkOutTime > afternoonEndAt && checkInTime.HasValue && checkInTime > morningEndAt;

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
                    if (isWFHFullday)
                        requiredHours = standardWorkHours * wfhThreshold;
                    else if (isWFHMorning && user.MorningWorking.HasValue)
                        requiredHours = user.MorningWorking.Value * wfhThreshold;
                    else if (isWFHAfternoon && user.AfternoonWorking.HasValue)
                        requiredHours = user.AfternoonWorking.Value * wfhThreshold;
                    else
                        requiredHours = standardWorkHours;
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
                            AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("yyyy/MM/dd"), totalWorkingTime,
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
                            AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("yyyy/MM/dd"), totalWorkingTime,
                                "Violation: Morning WFH, Afternoon office time mismatch", isYesterday, yesterdayAnomalies, lastWeekAnomalies);
                        }
                    }

                    bool isWorkingTimeViolation = (totalWorkingTime ?? 0) < requiredHours && !isMorningOfficeAfternoonWFH && !isMorningWFHAfternoonOffice && !(isWFHFullday || isWFHMorning || isWFHAfternoon);
                    bool isTrackerTimeViolation = (isWFHFullday || isWFHMorning || isWFHAfternoon) && (
                        (string.IsNullOrEmpty(tk.TrackerTime) ||
                        (TimeSpan.TryParse(tk.TrackerTime, out var trackerTimeSpan) && trackerTimeSpan.TotalHours == 0)) ||
                        (trackerTimeHours.HasValue && trackerTimeHours.Value < requiredHours)
                    ) && !isMorningOfficeAfternoonWFH && !isMorningWFHAfternoonOffice;
                    bool isZeroTrackerTimeViolation = (isWFHFullday || isWFHMorning || isWFHAfternoon) && TimeSpan.TryParse(tk.TrackerTime, out var zeroTrackerTimeSpan) && zeroTrackerTimeSpan.TotalHours == 0;

                    if (isWorkingTimeViolation || isTrackerTimeViolation)
                    {
                        string notes = isZeroTrackerTimeViolation ? "No tracker time for approved WFH" : "No early leave/late arrival approval";
                        AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("yyyy/MM/dd"), totalWorkingTime,
                            notes, isYesterday, yesterdayAnomalies, lastWeekAnomalies);
                    }
                }

                if (isFullDayAbsence || isMorningAbsence || isAfternoonAbsence || isMorningPresentAfternoonAbsent || isAfternoonPresentMorningAbsent)
                {
                    isValidAbsence = IsValidAbsenceForCase(userAbsenceRequests, (0, tk.DateAt), isFullDayAbsence, isMorningAbsence, isAfternoonAbsence,
                        isMorningPresentAfternoonAbsent, isAfternoonPresentMorningAbsent, absenceDetailDict);

                    if (!isValidAbsence)
                    {
                        AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("yyyy/MM/dd"), officeActualHours,
                            "No leave/WFH record", isYesterday, yesterdayAnomalies, lastWeekAnomalies, "DatesMissed");
                    }
                }

                if (isWFHFullday || isWFHMorning || isWFHAfternoon)
                {
                    bool isZeroTrackerTimeViolation = TimeSpan.TryParse(tk.TrackerTime, out var zeroTrackerTimeSpan) && zeroTrackerTimeSpan.TotalHours == 0;
                    if (isZeroTrackerTimeViolation)
                    {
                        AddOrUpdateAnomaly((long)tk.UserId, user.FullName, tk.DateAt.ToString("yyyy/MM/dd"), officeActualHours,
                            "No tracker time for approved WFH", isYesterday, yesterdayAnomalies, lastWeekAnomalies, "DatesNoTrackerTime");
                    }
                }
            }

            SortAnomalyDates(lastWeekAnomalies);
            yesterdayAnomalies = yesterdayAnomalies.OrderBy(a => a.EmployeeName).ToList();
            lastWeekAnomalies = lastWeekAnomalies.OrderBy(a => a.EmployeeName).ToList();
            return (yesterdayAnomalies, lastWeekAnomalies);
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
                    var (yesterdayAnomalies, _) = await ProcessAnomalies(branch.Id, startDate.Value, endDate.Value, true);
                    return yesterdayAnomalies;
                case "LastWeek":
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        throw new ArgumentException("startDate and endDate are required for LastWeek mode.");
                    }
                    var (_, lastWeekAnomalies) = await ProcessAnomalies(branch.Id, startDate.Value, endDate.Value, false);
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

        private void BuildReportHeader(StringBuilder messageBuilder, List<object> mkList, ref int currentPos, string title, string reportPeriod, string office)
        {
            AddBoldMarkup(messageBuilder, mkList, ref currentPos, title);
            int reportPeriodPos = currentPos;
            messageBuilder.AppendLine(reportPeriod);
            currentPos += reportPeriod.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = reportPeriodPos, e = reportPeriodPos + "Report Period:".Length });

            int officePos = currentPos;
            messageBuilder.AppendLine(office);
            currentPos += office.Length + Environment.NewLine.Length;
            mkList.Add(new { type = "b", s = officePos, e = officePos + "Office:".Length });

            messageBuilder.AppendLine("════════════════════════════════════");
            currentPos += "════════════════════════════════════".Length + Environment.NewLine.Length;
        }

        private void AppendAnomaliesSection<T>(StringBuilder messageBuilder, List<object> mkList, ref int currentPos, string sectionTitle, List<T> anomalies, string notes, bool isYesterday)
        {
            AddBoldMarkup(messageBuilder, mkList, ref currentPos, sectionTitle);

            if (anomalies.Any())
            {
                int index = 1;
                foreach (var anomaly in anomalies)
                {
                    string employeeName = $"{index}. Employee Name: {(isYesterday ? (anomaly as YesterdayAnomalyDTO).EmployeeName : (anomaly as LastWeekAnomalyDTO).EmployeeName)}";
                    int employeeNamePos = currentPos;
                    messageBuilder.AppendLine(employeeName);
                    currentPos += employeeName.Length + Environment.NewLine.Length;
                    mkList.Add(new { type = "b", s = employeeNamePos, e = employeeNamePos + $"{index}. Employee Name:".Length });

                    if (isYesterday)
                    {
                        var yesterdayAnomaly = anomaly as YesterdayAnomalyDTO;
                        string date = $"Date: {yesterdayAnomaly.Date}";
                        int datePos = currentPos;
                        messageBuilder.AppendLine(date);
                        currentPos += date.Length + Environment.NewLine.Length;
                        mkList.Add(new { type = "b", s = datePos, e = datePos + "Date:".Length });

                        if (notes == "No early leave/late arrival approval")
                        {
                            string actualHours = $"Actual Hours: {yesterdayAnomaly.ActualHours}";
                            int actualHoursPos = currentPos;
                            messageBuilder.AppendLine(actualHours);
                            currentPos += actualHours.Length + Environment.NewLine.Length;
                            mkList.Add(new { type = "b", s = actualHoursPos, e = actualHoursPos + "Actual Hours:".Length });
                        }
                    }
                    else
                    {
                        var lastWeekAnomaly = anomaly as LastWeekAnomalyDTO;
                        string datesKey = notes == "No leave/WFH record" ? "Dates Missed" :
                                         notes == "No early leave/late arrival approval" ? "Dates Below Threshold" : "Dates With No Tracker Time";
                        var dates = notes == "No leave/WFH record" ? lastWeekAnomaly.DatesMissed :
                                    notes == "No early leave/late arrival approval" ? lastWeekAnomaly.DatesBelowThreshold : lastWeekAnomaly.DatesNoTrackerTime;
                        string datesLine = $"{datesKey}: {string.Join(", ", dates)}";
                        int datesPos = currentPos;
                        messageBuilder.AppendLine(datesLine);
                        currentPos += datesLine.Length + Environment.NewLine.Length;
                        mkList.Add(new { type = "b", s = datesPos, e = datesPos + $"{datesKey}:".Length });

                        string count = $"Count: {dates.Count}";
                        int countPos = currentPos;
                        messageBuilder.AppendLine(count);
                        currentPos += count.Length + Environment.NewLine.Length;
                        mkList.Add(new { type = "b", s = countPos, e = countPos + "Count:".Length });
                    }

                    int notesPos = currentPos;
                    messageBuilder.AppendLine($"Notes: {notes}");
                    currentPos += $"Notes: {notes}".Length + Environment.NewLine.Length;
                    mkList.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });

                    messageBuilder.AppendLine();
                    currentPos += Environment.NewLine.Length;
                    index++;
                }
            }
            else
            {
                string noDataMessage = $"No {(isYesterday ? sectionTitle.ToLower().Replace("yesterday – ", "") : sectionTitle.ToLower().Replace("last week – ", ""))} found";
                messageBuilder.AppendLine(noDataMessage);
                currentPos += noDataMessage.Length + Environment.NewLine.Length;
            }

            messageBuilder.AppendLine("════════════════════════════════════");
            currentPos += "════════════════════════════════════".Length + Environment.NewLine.Length;
        }

        private void SendMezonMessage(string botUri, StringBuilder messageBuilder, List<object> mkList)
        {
            var messageText = messageBuilder.ToString();
            if (!string.IsNullOrEmpty(messageText))
            {
                _mezonService.Post(botUri, new
                {
                    type = "hook",
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
                var branchResult = await GetBranchDataAsync(branchName);
                var branch = (Timesheet.Entities.Branch)branchResult;
                if (branch == null)
                {
                    continue;
                }

                if (!isWeekly)
                {
                    var messageBuilder = new StringBuilder();
                    var mkList = new List<object>();
                    int currentPos = 0;

                    List<YesterdayAnomalyDTO> yesterdayAnomalies = await GetYesterdayAnomalies(branchName, now.AddDays(-1).Date);
                    var yesterdayUnplannedAbsences = yesterdayAnomalies?.Where(a => a.Notes == "No leave/WFH record").ToList() ?? new List<YesterdayAnomalyDTO>();
                    var yesterdayUnapprovedShortHours = yesterdayAnomalies?.Where(a => a.Notes == "No early leave/late arrival approval").ToList() ?? new List<YesterdayAnomalyDTO>();
                    var yesterdayNoTrackerTimeWFH = yesterdayAnomalies?.Where(a => a.Notes == "No tracker time for approved WFH").ToList() ?? new List<YesterdayAnomalyDTO>();

                    BuildReportHeader(messageBuilder, mkList, ref currentPos, "Daily Anomalies Report",
                        $"Report Period: {now.AddDays(-1):yyyy/MM/dd}", $"Office: {branch.Name}");
                    AppendAnomaliesSection(messageBuilder, mkList, ref currentPos, "Yesterday – Unplanned Absences",
                        yesterdayUnplannedAbsences, "No leave/WFH record", true);
                    AppendAnomaliesSection(messageBuilder, mkList, ref currentPos, "Yesterday – Unapproved Short Working Hours",
                        yesterdayUnapprovedShortHours, "No early leave/late arrival approval", true);
                    AppendAnomaliesSection(messageBuilder, mkList, ref currentPos, "Yesterday – Missing tracker time for approved WFH",
                        yesterdayNoTrackerTimeWFH, "No tracker time for approved WFH", true);

                    SendMezonMessage(input.botUri, messageBuilder, mkList);
                }
                else
                {
                    var messageBuilder = new StringBuilder();
                    var mkList = new List<object>();
                    int currentPos = 0;

                    DateTime lastMonday = now.AddDays(-(int)now.DayOfWeek - 6).Date;
                    DateTime lastWeekStart = lastMonday;
                    DateTime lastWeekEnd = lastMonday.AddDays(5).Date.AddSeconds(-1);
                    List<LastWeekAnomalyDTO> lastWeekAnomalies = await GetLastWeekAnomalies(branchName, lastWeekStart, lastWeekEnd);

                    var lastWeekUnplannedAbsences = lastWeekAnomalies?.Where(a => a.Notes == "No leave/WFH record" && a.DatesMissed.Any()).ToList() ?? new List<LastWeekAnomalyDTO>();
                    var lastWeekUnapprovedShortHours = lastWeekAnomalies?.Where(a => a.Notes == "No early leave/late arrival approval" && a.DatesBelowThreshold.Any()).ToList() ?? new List<LastWeekAnomalyDTO>();
                    var lastWeekNoTrackerTimeWFH = lastWeekAnomalies?.Where(a => a.Notes == "No tracker time for approved WFH" && a.DatesNoTrackerTime.Any()).ToList() ?? new List<LastWeekAnomalyDTO>();

                    BuildReportHeader(messageBuilder, mkList, ref currentPos, "Weekly Anomalies Report",
                        $"Report Period: {lastWeekStart:yyyy/MM/dd} - {lastWeekEnd:yyyy/MM/dd}", $"Office: {branch.Name}");
                    AppendAnomaliesSection(messageBuilder, mkList, ref currentPos, "Last Week – Unplanned Absences",
                        lastWeekUnplannedAbsences, "No leave/WFH record", false);
                    AppendAnomaliesSection(messageBuilder, mkList, ref currentPos, "Last Week – Unapproved Short Working Hours",
                        lastWeekUnapprovedShortHours, "No early leave/late arrival approval", false);
                    AppendAnomaliesSection(messageBuilder, mkList, ref currentPos, "Last Week – Missing tracker time for approved WFH",
                        lastWeekNoTrackerTimeWFH, "No tracker time for approved WFH", false);

                    SendMezonMessage(input.botUri, messageBuilder, mkList);
                }
            }

            return true;
        }
    }
}