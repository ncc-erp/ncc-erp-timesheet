using Abp.Dependency;
using Amazon.Runtime.Internal.Util;
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
using Timesheet.Extension;
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
                .Where(r => userIds.Contains(r.UserId) && r.Status == RequestStatus.Approved)
                .Select(r => new { r.Id, r.UserId, r.Status, r.Type })
                .ToListAsync();

            var requestIds = absenceRequests.Select(r => r.Id).ToList();

            var absenceDetails = await _workScope.GetAll<AbsenceDayDetail>()
                .Where(d => requestIds.Contains(d.RequestId) && d.DateAt >= startDate && d.DateAt <= endDate)
                .Select(d => new { d.RequestId, d.DateAt, d.DateType, d.AbsenceTime, d.Hour })
                .ToListAsync();

            var allAbsenceRequests = await _workScope.GetAll<AbsenceDayRequest>()
                .Where(r => userIds.Contains(r.UserId))
                .Select(r => new { r.Id, r.UserId, r.Status, r.Type })
                .ToListAsync();

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

                TimeSpan? morningStartAt = TimeSpan.TryParse(user.MorningStartAt, out var msa) ? msa : (TimeSpan?)null;
                TimeSpan? morningEndAt = TimeSpan.TryParse(user.MorningEndAt, out var mea) ? mea : (TimeSpan?)null;
                TimeSpan? afternoonStartAt = TimeSpan.TryParse(user.AfternoonStartAt, out var asa) ? asa : (TimeSpan?)null;
                TimeSpan? afternoonEndAt = TimeSpan.TryParse(user.AfternoonEndAt, out var aea) ? aea : (TimeSpan?)null;
                TimeSpan? checkInTime = TimeSpan.TryParse(tk.CheckIn, out var ci) ? ci : (TimeSpan?)null;
                TimeSpan? checkOutTime = tk.CheckOut != null && TimeSpan.TryParse(tk.CheckOut, out var co) ? co : (TimeSpan?)null;

                bool isWFHFullday = absenceRequests.Any(r => r.UserId == tk.UserId && r.Type == RequestType.Remote && r.Status == RequestStatus.Approved
                    && absenceDetails.Any(d => d.RequestId == r.Id && d.DateAt == tk.DateAt && d.DateType == DayType.Fullday));
                bool isWFHMorning = absenceRequests.Any(r => r.UserId == tk.UserId && r.Type == RequestType.Remote && r.Status == RequestStatus.Approved
                    && absenceDetails.Any(d => d.RequestId == r.Id && d.DateAt == tk.DateAt && d.DateType == DayType.Morning));
                bool isWFHAfternoon = absenceRequests.Any(r => r.UserId == tk.UserId && r.Type == RequestType.Remote && r.Status == RequestStatus.Approved
                    && absenceDetails.Any(d => d.RequestId == r.Id && d.DateAt == tk.DateAt && d.DateType == DayType.Afternoon));

                if (!string.IsNullOrEmpty(tk.TrackerTime) && (isWFHFullday || isWFHMorning || isWFHAfternoon))
                {
                    if (TimeSpan.TryParse(tk.TrackerTime, out var trackerTimeSpan))
                    {
                        trackerTimeHours = Math.Round(trackerTimeSpan.TotalHours, 2);
                        double lateMinutes = 0.0;

                        if (morningStartAt.HasValue && checkInTime > morningStartAt)
                        {
                            var delay = checkInTime.Value - morningStartAt.Value;
                            if (delay.TotalMinutes <= gracePeriodMinutes)
                            {
                                lateMinutes = Math.Min(delay.TotalMinutes, gracePeriodMinutes);
                            }
                        }
                        else if (afternoonStartAt.HasValue && checkInTime > afternoonStartAt)
                        {
                            var delay = checkInTime.Value - afternoonStartAt.Value;
                            if (delay.TotalMinutes <= gracePeriodMinutes)
                            {
                                lateMinutes = Math.Min(delay.TotalMinutes, gracePeriodMinutes);
                            }
                        }

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
                    double lateMinutes = 0.0;

                    if (morningStartAt.HasValue && checkInTime > morningStartAt)
                    {
                        var delay = checkInTime.Value - morningStartAt.Value;
                        if (delay.TotalMinutes <= gracePeriodMinutes)
                        {
                            lateMinutes = Math.Min(delay.TotalMinutes, gracePeriodMinutes);
                        }
                    }
                    else if (afternoonStartAt.HasValue && checkInTime > afternoonStartAt)
                    {
                        var delay = checkInTime.Value - afternoonStartAt.Value;
                        if (delay.TotalMinutes <= gracePeriodMinutes)
                        {
                            lateMinutes = Math.Min(delay.TotalMinutes, gracePeriodMinutes);
                        }
                    }

                    workingTime += lateMinutes / 60.0;
                    if (!(isMorningPresentAfternoonAbsent || isAfternoonPresentMorningAbsent))
                    {
                        workingTime -= breakTime;
                    }
                    officeActualHours = Math.Round(workingTime, 2);
                }

                var relatedRequests = allAbsenceRequests.Where(r => r.UserId == tk.UserId).ToList();
                double tardinessHour = 0, leaveEarlyHour = 0;

                foreach (var request in relatedRequests)
                {
                    var details = absenceDetails.Where(d => d.RequestId == request.Id && d.DateAt == tk.DateAt && d.AbsenceTime != null).ToList();
                    foreach (var detail in details)
                    {
                        if (detail.AbsenceTime == OnDayType.DiMuon && detail.Hour > 0)
                            tardinessHour += detail.Hour;
                        if (detail.AbsenceTime == OnDayType.VeSom && detail.Hour > 0)
                            leaveEarlyHour += detail.Hour;
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
                    var userRequest = absenceRequests.FirstOrDefault(r => r.UserId == tk.UserId);
                    if (userRequest != null && absenceDetails.Any(d => d.RequestId == userRequest.Id && d.DateType == DayType.Afternoon && d.DateAt == tk.DateAt)
                        && timekeepings.Any(t => t.UserId == tk.UserId && TimeSpan.TryParse(t.CheckIn, out var ci1) && TimeSpan.TryParse(t.CheckOut, out var co1)
                            && ci1 < morningEndAt && co1 < afternoonStartAt && !string.IsNullOrEmpty(t.TrackerTime) && double.TryParse(t.TrackerTime, out var tr) && tr > 0))
                    {
                        isMorningOfficeAfternoonWFH = true;
                        double morningRequiredHours = (user.MorningWorking ?? 0) - tardinessHour - leaveEarlyHour;
                        double afternoonRequiredTrackerHours = (user.AfternoonWorking ?? 0) * wfhThreshold - tardinessHour - leaveEarlyHour;
                        if ((officeActualHours ?? 0) < morningRequiredHours || (trackerTimeHours.HasValue && trackerTimeHours.Value < afternoonRequiredTrackerHours))
                        {
                            if (isYesterday)
                            {
                                var yesterdayDto = new YesterdayAnomalyDTO
                                {
                                    UserId = (long)tk.UserId,
                                    EmployeeName = user.FullName,
                                    Date = tk.DateAt.ToString("yyyy/MM/dd"),
                                    ActualHours = totalWorkingTime.HasValue ? $"{totalWorkingTime.Value.ToString("F2", CultureInfo.InvariantCulture)}h" : "0h",
                                    Notes = "Violation: Morning office, Afternoon WFH time mismatch"
                                };
                                yesterdayAnomalies.Add(yesterdayDto);
                            }
                            else
                            {
                                var lastWeekDto = lastWeekAnomalies.FirstOrDefault(a => a.UserId == tk.UserId) ?? new LastWeekAnomalyDTO { UserId = (long)tk.UserId, EmployeeName = user.FullName };
                                lastWeekDto.DatesBelowThreshold.Add(tk.DateAt.ToString("yyyy/MM/dd"));
                                lastWeekDto.Count++;
                                lastWeekDto.Notes = "Violation: Morning office, Afternoon WFH time mismatch";
                                if (!lastWeekAnomalies.Any(a => a.UserId == tk.UserId))
                                    lastWeekAnomalies.Add(lastWeekDto);
                            }
                        }
                    }

                    bool isMorningWFHAfternoonOffice = false;
                    if (timekeepings.Any(t => t.UserId == tk.UserId && TimeSpan.TryParse(t.CheckIn, out var ci2) && TimeSpan.TryParse(t.CheckOut, out var co2)
                        && co2 > afternoonEndAt && ci2 > morningEndAt && !string.IsNullOrEmpty(t.TrackerTime) && double.TryParse(t.TrackerTime, out var tr) && tr > 0))
                    {
                        isMorningWFHAfternoonOffice = true;
                        double afternoonRequiredHours = (user.AfternoonWorking ?? 0) - tardinessHour - leaveEarlyHour;
                        double morningRequiredTrackerHours = (user.MorningWorking ?? 0) * wfhThreshold - tardinessHour - leaveEarlyHour;
                        if ((officeActualHours ?? 0) < afternoonRequiredHours || (trackerTimeHours.HasValue && trackerTimeHours.Value < morningRequiredTrackerHours))
                        {
                            if (isYesterday)
                            {
                                var yesterdayDto = new YesterdayAnomalyDTO
                                {
                                    UserId = (long)tk.UserId,
                                    EmployeeName = user.FullName,
                                    Date = tk.DateAt.ToString("yyyy/MM/dd"),
                                    ActualHours = totalWorkingTime.HasValue ? $"{totalWorkingTime.Value.ToString("F2", CultureInfo.InvariantCulture)}h" : "0h",
                                    Notes = "Violation: Morning WFH, Afternoon office time mismatch"
                                };
                                yesterdayAnomalies.Add(yesterdayDto);
                            }
                            else
                            {
                                var lastWeekDto = lastWeekAnomalies.FirstOrDefault(a => a.UserId == tk.UserId) ?? new LastWeekAnomalyDTO { UserId = (long)tk.UserId, EmployeeName = user.FullName };
                                lastWeekDto.DatesBelowThreshold.Add(tk.DateAt.ToString("yyyy/MM/dd"));
                                lastWeekDto.Count++;
                                lastWeekDto.Notes = "Violation: Morning WFH, Afternoon office time mismatch";
                                if (!lastWeekAnomalies.Any(a => a.UserId == tk.UserId))
                                    lastWeekAnomalies.Add(lastWeekDto);
                            }
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
                        if (isYesterday)
                        {
                            var yesterdayDto = new YesterdayAnomalyDTO
                            {
                                UserId = (long)tk.UserId,
                                EmployeeName = user.FullName,
                                Date = tk.DateAt.ToString("yyyy/MM/dd"),
                                ActualHours = totalWorkingTime.HasValue ? $"{totalWorkingTime.Value.ToString("F2", CultureInfo.InvariantCulture)}h" : "0h",
                                Notes = isZeroTrackerTimeViolation ? "No tracker time for approved WFH" : "No early leave/late arrival approval"
                            };
                            yesterdayAnomalies.Add(yesterdayDto);
                        }
                        else
                        {
                            var lastWeekDto = lastWeekAnomalies.FirstOrDefault(a => a.UserId == tk.UserId) ?? new LastWeekAnomalyDTO { UserId = (long)tk.UserId, EmployeeName = user.FullName };
                            lastWeekDto.DatesBelowThreshold.Add(tk.DateAt.ToString("yyyy/MM/dd"));
                            lastWeekDto.Count++;
                            lastWeekDto.Notes = isZeroTrackerTimeViolation ? "No tracker time for approved WFH" : "No early leave/late arrival approval";
                            if (!lastWeekAnomalies.Any(a => a.UserId == tk.UserId))
                                lastWeekAnomalies.Add(lastWeekDto);
                        }
                    }
                }

                if (isFullDayAbsence || isMorningAbsence || isAfternoonAbsence || isMorningPresentAfternoonAbsent || isAfternoonPresentMorningAbsent)
                {
                    foreach (var request in allAbsenceRequests.Where(r => r.UserId == tk.UserId))
                    {
                        var detail = absenceDetails.FirstOrDefault(d => d.RequestId == request.Id && d.DateAt == tk.DateAt);
                        if (detail != null)
                        {
                            if (isFullDayAbsence && detail.DateType == DayType.Fullday)
                            {
                                isValidAbsence = true;
                                break;
                            }
                            else if (isFullDayAbsence && (detail.DateType == DayType.Morning || detail.DateType == DayType.Afternoon))
                            {
                                var otherDetail = absenceDetails.FirstOrDefault(d => d.RequestId == request.Id
                                    && d.DateAt == tk.DateAt
                                    && d.DateType != detail.DateType
                                    && (d.DateType == DayType.Morning || d.DateType == DayType.Afternoon));
                                if (otherDetail != null)
                                {
                                    isValidAbsence = true;
                                    break;
                                }
                            }
                            else if (isMorningAbsence && detail.DateType == DayType.Morning)
                            {
                                isValidAbsence = true;
                                break;
                            }
                            else if (isAfternoonAbsence && detail.DateType == DayType.Afternoon)
                            {
                                isValidAbsence = true;
                                break;
                            }
                            else if (isMorningPresentAfternoonAbsent && request.Type == RequestType.Remote && request.Status == RequestStatus.Approved
                                && detail.DateType == DayType.Afternoon)
                            {
                                isValidAbsence = true;
                                break;
                            }
                            else if (isAfternoonPresentMorningAbsent && request.Type == RequestType.Remote && request.Status == RequestStatus.Approved
                                && detail.DateType == DayType.Morning)
                            {
                                isValidAbsence = true;
                                break;
                            }
                        }
                        else if (isFullDayAbsence && request.Type == RequestType.Remote && request.Status == RequestStatus.Approved
                            && absenceDetails.Any(d => d.RequestId == request.Id && d.DateAt == tk.DateAt))
                        {
                            isValidAbsence = true;
                            break;
                        }
                    }

                    if (!isValidAbsence)
                    {
                        if (isYesterday)
                        {
                            var yesterdayDto = new YesterdayAnomalyDTO
                            {
                                UserId = (long)tk.UserId,
                                EmployeeName = user.FullName,
                                Date = tk.DateAt.ToString("yyyy/MM/dd"),
                                ActualHours = officeActualHours.HasValue ? $"{officeActualHours.Value.ToString("F2", CultureInfo.InvariantCulture)}h" : "0h",
                                Notes = "No leave/WFH record"
                            };
                            yesterdayAnomalies.Add(yesterdayDto);
                        }
                        else
                        {
                            var lastWeekDto = lastWeekAnomalies.FirstOrDefault(a => a.UserId == tk.UserId) ?? new LastWeekAnomalyDTO { UserId = (long)tk.UserId, EmployeeName = user.FullName };
                            lastWeekDto.DatesMissed.Add(tk.DateAt.ToString("yyyy/MM/dd"));
                            lastWeekDto.Notes = "No leave/WFH record";
                            if (!lastWeekAnomalies.Any(a => a.UserId == tk.UserId))
                                lastWeekAnomalies.Add(lastWeekDto);
                        }
                    }
                }

                if (isWFHFullday || isWFHMorning || isWFHAfternoon)
                {
                    bool isZeroTrackerTimeViolation = TimeSpan.TryParse(tk.TrackerTime, out var zeroTrackerTimeSpan) && zeroTrackerTimeSpan.TotalHours == 0;
                    if (isZeroTrackerTimeViolation)
                    {
                        if (isYesterday)
                        {
                            var yesterdayDto = new YesterdayAnomalyDTO
                            {
                                UserId = (long)tk.UserId,
                                EmployeeName = user.FullName,
                                Date = tk.DateAt.ToString("yyyy/MM/dd"),
                                ActualHours = officeActualHours.HasValue ? $"{officeActualHours.Value.ToString("F2", CultureInfo.InvariantCulture)}h" : "0h",
                                Notes = "No tracker time for approved WFH"
                            };
                            yesterdayAnomalies.Add(yesterdayDto);
                        }
                        else
                        {
                            var lastWeekDto = lastWeekAnomalies.FirstOrDefault(a => a.UserId == tk.UserId) ?? new LastWeekAnomalyDTO { UserId = (long)tk.UserId, EmployeeName = user.FullName };
                            lastWeekDto.DatesNoTrackerTime.Add(tk.DateAt.ToString("yyyy/MM/dd"));
                            lastWeekDto.Notes = "No tracker time for approved WFH";
                            if (!lastWeekAnomalies.Any(a => a.UserId == tk.UserId))
                                lastWeekAnomalies.Add(lastWeekDto);
                        }
                    }
                }
            }

            foreach (var anomaly in lastWeekAnomalies)
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

            yesterdayAnomalies = yesterdayAnomalies.OrderBy(a => a.EmployeeName).ToList();
            lastWeekAnomalies = lastWeekAnomalies.OrderBy(a => a.EmployeeName).ToList();
            return (yesterdayAnomalies, lastWeekAnomalies);
        }

        public async Task<List<YesterdayAnomalyDTO>> GetYesterdayAnomalies(string branchName, DateTime date)
        {
            var branch = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Where(b => b.Name == branchName)
                .FirstOrDefaultAsync();

            if (branch == null)
            {
                throw new ArgumentException($"Branch with name '{branchName}' not found.");
            }

            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddSeconds(-1);
            var (yesterdayAnomalies, _) = await ProcessAnomalies(branch.Id, startDate, endDate, true);
            return yesterdayAnomalies;
        }

        public async Task<List<LastWeekAnomalyDTO>> GetLastWeekAnomalies(string branchName, DateTime startDate, DateTime endDate)
        {
            var branch = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Where(b => b.Name == branchName)
                .FirstOrDefaultAsync();

            if (branch == null)
            {
                throw new ArgumentException($"Branch with name '{branchName}' not found.");
            }

            var (_, lastWeekAnomalies) = await ProcessAnomalies(branch.Id, startDate, endDate, false);
            return lastWeekAnomalies;
        }

        private async Task<Timesheet.Entities.Branch> GetBranchByNameAsync(string branchName)
        {
            var branch = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .FirstOrDefaultAsync(b => b.Name == branchName);

            if (branch == null)
            {
                throw new ArgumentException($"Branch with name '{branchName}' not found.");
            }

            return branch;
        }

        public async Task<bool> SendDailyAnomaliesToMezon(BotReportSettingDto input, bool isWeekly)
        {
            DateTime now = DateTime.Now;

            if (!input.enable)
            {
                return true;
            }

            foreach (var branchName in input.branchCodes)
            {
                var branch = await GetBranchByNameAsync(branchName);
                if (branch == null)
                {
                    continue;
                }

                if (!isWeekly)
                {
                    List<YesterdayAnomalyDTO> yesterdayAnomalies = await GetYesterdayAnomalies(branchName, now.AddDays(-1).Date);
                    var messageBuilderYesterday = new StringBuilder();
                    var mkListYesterday = new List<object>();
                    int currentPosYesterday = 0;

                    void AddBoldMarkupYesterday(string title)
                    {
                        int titlePos = currentPosYesterday;
                        messageBuilderYesterday.AppendLine(title);
                        currentPosYesterday += title.Length + Environment.NewLine.Length;
                        mkListYesterday.Add(new { type = "b", s = titlePos, e = titlePos + title.Length });
                    }

                    AddBoldMarkupYesterday("🤖 Daily Anomalies Report");
                    string reportPeriodYesterday = $"Report Period: {now.AddDays(-1):yyyy/MM/dd}";
                    int reportPeriodPosYesterday = currentPosYesterday;
                    messageBuilderYesterday.AppendLine(reportPeriodYesterday);
                    currentPosYesterday += reportPeriodYesterday.Length + Environment.NewLine.Length;
                    mkListYesterday.Add(new { type = "b", s = reportPeriodPosYesterday, e = reportPeriodPosYesterday + "Report Period:".Length });

                    string officeYesterday = $"Office: {branch.Name}";
                    int officePosYesterday = currentPosYesterday;
                    messageBuilderYesterday.AppendLine(officeYesterday);
                    currentPosYesterday += officeYesterday.Length + Environment.NewLine.Length;
                    mkListYesterday.Add(new { type = "b", s = officePosYesterday, e = officePosYesterday + "Office:".Length });

                    messageBuilderYesterday.AppendLine("════════════════════════════════════");
                    currentPosYesterday += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    var yesterdayUnplannedAbsences = yesterdayAnomalies?.Where(a => a.Notes == "No leave/WFH record").ToList() ?? new List<YesterdayAnomalyDTO>();
                    var yesterdayUnapprovedShortHours = yesterdayAnomalies?.Where(a => a.Notes == "No early leave/late arrival approval").ToList() ?? new List<YesterdayAnomalyDTO>();
                    var yesterdayNoTrackerTimeWFH = yesterdayAnomalies?.Where(a => a.Notes == "No tracker time for approved WFH").ToList() ?? new List<YesterdayAnomalyDTO>();

                    if (yesterdayUnplannedAbsences.Any())
                    {
                        AddBoldMarkupYesterday("📅 Yesterday – Unplanned Absences");
                        int index = 1;
                        foreach (var anomaly in yesterdayUnplannedAbsences)
                        {
                            string employeeName = $"{index}. Employee Name: {anomaly.EmployeeName}";
                            int employeeNamePos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(employeeName);
                            currentPosYesterday += employeeName.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = employeeNamePos, e = employeeNamePos + $"{index}. Employee Name:".Length });

                            string date = $"Date: {anomaly.Date}";
                            int datePos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(date);
                            currentPosYesterday += date.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = datePos, e = datePos + "Date:".Length });

                            string notes = $"Notes: No leave/WFH record";
                            int notesPos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(notes);
                            currentPosYesterday += notes.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });

                            messageBuilderYesterday.AppendLine();
                            currentPosYesterday += Environment.NewLine.Length;
                            index++;
                        }
                    }
                    else
                    {
                        AddBoldMarkupYesterday("📅 Yesterday – Unplanned Absences");
                        messageBuilderYesterday.AppendLine("No unplanned absences found");
                        currentPosYesterday += "No unplanned absences found".Length + Environment.NewLine.Length;
                    }

                    messageBuilderYesterday.AppendLine("════════════════════════════════════");
                    currentPosYesterday += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    if (yesterdayUnapprovedShortHours.Any())
                    {
                        AddBoldMarkupYesterday("⏰ Yesterday – Unapproved Short Working Hours");
                        int index = 1;
                        foreach (var anomaly in yesterdayUnapprovedShortHours)
                        {
                            string employeeName = $"{index}. Employee Name: {anomaly.EmployeeName}";
                            int employeeNamePos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(employeeName);
                            currentPosYesterday += employeeName.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = employeeNamePos, e = employeeNamePos + $"{index}. Employee Name:".Length });

                            string date = $"Date: {anomaly.Date}";
                            int datePos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(date);
                            currentPosYesterday += date.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = datePos, e = datePos + "Date:".Length });

                            string actualHours = $"Actual Hours: {anomaly.ActualHours}";
                            int actualHoursPos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(actualHours);
                            currentPosYesterday += actualHours.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = actualHoursPos, e = actualHoursPos + "Actual Hours:".Length });

                            string notes = $"Notes: No early leave/late arrival approval";
                            int notesPos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(notes);
                            currentPosYesterday += notes.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });

                            messageBuilderYesterday.AppendLine();
                            currentPosYesterday += Environment.NewLine.Length;
                            index++;
                        }
                    }
                    else
                    {
                        AddBoldMarkupYesterday("⏰ Yesterday – Unapproved Short Working Hours");
                        messageBuilderYesterday.AppendLine("No unapproved short working hours found");
                        currentPosYesterday += "No unapproved short working hours found".Length + Environment.NewLine.Length;
                    }

                    messageBuilderYesterday.AppendLine("════════════════════════════════════");
                    currentPosYesterday += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    if (yesterdayNoTrackerTimeWFH.Any())
                    {
                        AddBoldMarkupYesterday("🏠 Yesterday – No tracker time for approved WFH");
                        int index = 1;
                        foreach (var anomaly in yesterdayNoTrackerTimeWFH)
                        {
                            string employeeName = $"{index}. Employee Name: {anomaly.EmployeeName}";
                            int employeeNamePos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(employeeName);
                            currentPosYesterday += employeeName.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = employeeNamePos, e = employeeNamePos + $"{index}. Employee Name:".Length });

                            string date = $"Date: {anomaly.Date}";
                            int datePos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(date);
                            currentPosYesterday += date.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = datePos, e = datePos + "Date:".Length });

                            string notes = $"Notes: No tracker time for approved WFH";
                            int notesPos = currentPosYesterday;
                            messageBuilderYesterday.AppendLine(notes);
                            currentPosYesterday += notes.Length + Environment.NewLine.Length;
                            mkListYesterday.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });

                            messageBuilderYesterday.AppendLine();
                            currentPosYesterday += Environment.NewLine.Length;
                            index++;
                        }
                    }
                    else
                    {
                        AddBoldMarkupYesterday("🏠 Yesterday – No tracker time for approved WFH");
                        messageBuilderYesterday.AppendLine("No WFH tracker time issues found");
                        currentPosYesterday += "No WFH tracker time issues found".Length + Environment.NewLine.Length;
                    }

                    messageBuilderYesterday.AppendLine("════════════════════════════════════");
                    currentPosYesterday += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    var messageTextYesterday = messageBuilderYesterday.ToString();
                    if (!string.IsNullOrEmpty(messageTextYesterday))
                    {
                        _mezonService.Post(input.botUri, new
                        {
                            type = "hook",
                            message = new
                            {
                                t = messageTextYesterday,
                                mk = mkListYesterday
                            }
                        });
                    }
                }
                else
                {
                    DateTime lastMonday = now.AddDays(-(int)now.DayOfWeek - 6).Date;
                    DateTime lastWeekStart = lastMonday;
                    DateTime lastWeekEnd = lastMonday.AddDays(5).Date.AddSeconds(-1);
                    List<LastWeekAnomalyDTO> lastWeekAnomalies = await GetLastWeekAnomalies(branchName, lastWeekStart, lastWeekEnd);

                    var messageBuilderLastWeek = new StringBuilder();
                    var mkListLastWeek = new List<object>();
                    int currentPosLastWeek = 0;

                    void AddBoldMarkupLastWeek(string title)
                    {
                        int titlePos = currentPosLastWeek;
                        messageBuilderLastWeek.AppendLine(title);
                        currentPosLastWeek += title.Length + Environment.NewLine.Length;
                        mkListLastWeek.Add(new { type = "b", s = titlePos, e = titlePos + title.Length });
                    }

                    AddBoldMarkupLastWeek("🤖 Weekly Anomalies Report");
                    string reportPeriodLastWeek = $"Report Period: {lastWeekStart:yyyy/MM/dd} - {lastWeekEnd:yyyy/MM/dd}";
                    int reportPeriodPosLastWeek = currentPosLastWeek;
                    messageBuilderLastWeek.AppendLine(reportPeriodLastWeek);
                    currentPosLastWeek += reportPeriodLastWeek.Length + Environment.NewLine.Length;
                    mkListLastWeek.Add(new { type = "b", s = reportPeriodPosLastWeek, e = reportPeriodPosLastWeek + "Report Period:".Length });

                    string officeLastWeek = $"Office: {branch.Name}";
                    int officePosLastWeek = currentPosLastWeek;
                    messageBuilderLastWeek.AppendLine(officeLastWeek);
                    currentPosLastWeek += officeLastWeek.Length + Environment.NewLine.Length;
                    mkListLastWeek.Add(new { type = "b", s = officePosLastWeek, e = officePosLastWeek + "Office:".Length });

                    messageBuilderLastWeek.AppendLine("════════════════════════════════════");
                    currentPosLastWeek += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    var lastWeekUnplannedAbsences = lastWeekAnomalies?.Where(a => a.Notes == "No leave/WFH record" && a.DatesMissed.Any()).ToList() ?? new List<LastWeekAnomalyDTO>();
                    var lastWeekUnapprovedShortHours = lastWeekAnomalies?.Where(a => a.Notes == "No early leave/late arrival approval" && a.DatesBelowThreshold.Any()).ToList() ?? new List<LastWeekAnomalyDTO>();
                    var lastWeekNoTrackerTimeWFH = lastWeekAnomalies?.Where(a => a.Notes == "No tracker time for approved WFH" && a.DatesNoTrackerTime.Any()).ToList() ?? new List<LastWeekAnomalyDTO>();

                    if (lastWeekUnplannedAbsences.Any())
                    {
                        AddBoldMarkupLastWeek("📅 Last Week – Unplanned Absences");
                        int index = 1;
                        foreach (var anomaly in lastWeekUnplannedAbsences)
                        {
                            string employeeName = $"{index}. Employee Name: {anomaly.EmployeeName}";
                            int employeeNamePos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(employeeName);
                            currentPosLastWeek += employeeName.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = employeeNamePos, e = employeeNamePos + $"{index}. Employee Name:".Length });

                            string datesMissed = $"Dates Missed: {string.Join(", ", anomaly.DatesMissed)}";
                            int datesMissedPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(datesMissed);
                            currentPosLastWeek += datesMissed.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = datesMissedPos, e = datesMissedPos + "Dates Missed:".Length });

                            string count = $"Count: {anomaly.DatesMissed.Count}";
                            int countPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(count);
                            currentPosLastWeek += count.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = countPos, e = countPos + "Count:".Length });

                            string notes = $"Notes: No leave/WFH record";
                            int notesPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(notes);
                            currentPosLastWeek += notes.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });

                            messageBuilderLastWeek.AppendLine();
                            currentPosLastWeek += Environment.NewLine.Length;
                            index++;
                        }
                    }
                    else
                    {
                        AddBoldMarkupLastWeek("📅 Last Week – Unplanned Absences");
                        messageBuilderLastWeek.AppendLine("No unplanned absences found");
                        currentPosLastWeek += "No unplanned absences found".Length + Environment.NewLine.Length;
                    }

                    messageBuilderLastWeek.AppendLine("════════════════════════════════════");
                    currentPosLastWeek += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    if (lastWeekUnapprovedShortHours.Any())
                    {
                        AddBoldMarkupLastWeek("⏰ Last Week – Unapproved Short Working Hours");
                        int index = 1;
                        foreach (var anomaly in lastWeekUnapprovedShortHours)
                        {
                            string employeeName = $"{index}. Employee Name: {anomaly.EmployeeName}";
                            int employeeNamePos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(employeeName);
                            currentPosLastWeek += employeeName.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = employeeNamePos, e = employeeNamePos + $"{index}. Employee Name:".Length });

                            string datesBelowThreshold = $"Dates Below Threshold: {string.Join(", ", anomaly.DatesBelowThreshold)}";
                            int datesBelowThresholdPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(datesBelowThreshold);
                            currentPosLastWeek += datesBelowThreshold.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = datesBelowThresholdPos, e = datesBelowThresholdPos + "Dates Below Threshold:".Length });

                            string count = $"Count: {anomaly.DatesBelowThreshold.Count}";
                            int countPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(count);
                            currentPosLastWeek += count.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = countPos, e = countPos + "Count:".Length });

                            string notes = $"Notes: No early leave/late arrival approval";
                            int notesPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(notes);
                            currentPosLastWeek += notes.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });

                            messageBuilderLastWeek.AppendLine();
                            currentPosLastWeek += Environment.NewLine.Length;
                            index++;
                        }
                    }
                    else
                    {
                        AddBoldMarkupLastWeek("⏰ Last Week – Unapproved Short Working Hours");
                        messageBuilderLastWeek.AppendLine("No unapproved short working hours found");
                        currentPosLastWeek += "No unapproved short working hours found".Length + Environment.NewLine.Length;
                    }

                    messageBuilderLastWeek.AppendLine("════════════════════════════════════");
                    currentPosLastWeek += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    if (lastWeekNoTrackerTimeWFH.Any())
                    {
                        AddBoldMarkupLastWeek("🏠 Last Week – No tracker time for approved WFH");
                        int index = 1;
                        foreach (var anomaly in lastWeekNoTrackerTimeWFH)
                        {
                            string employeeName = $"{index}. Employee Name: {anomaly.EmployeeName}";
                            int employeeNamePos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(employeeName);
                            currentPosLastWeek += employeeName.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = employeeNamePos, e = employeeNamePos + $"{index}. Employee Name:".Length });

                            string datesNoTrackerTime = $"Dates With No Tracker Time: {string.Join(", ", anomaly.DatesNoTrackerTime)}";
                            int datesNoTrackerTimePos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(datesNoTrackerTime);
                            currentPosLastWeek += datesNoTrackerTime.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = datesNoTrackerTimePos, e = datesNoTrackerTimePos + "Dates With No Tracker Time:".Length });

                            string count = $"Count: {anomaly.DatesNoTrackerTime.Count}";
                            int countPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(count);
                            currentPosLastWeek += count.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = countPos, e = countPos + "Count:".Length });

                            string notes = $"Notes: No tracker time for approved WFH";
                            int notesPos = currentPosLastWeek;
                            messageBuilderLastWeek.AppendLine(notes);
                            currentPosLastWeek += notes.Length + Environment.NewLine.Length;
                            mkListLastWeek.Add(new { type = "b", s = notesPos, e = notesPos + "Notes:".Length });

                            messageBuilderLastWeek.AppendLine();
                            currentPosLastWeek += Environment.NewLine.Length;
                            index++;
                        }
                    }
                    else
                    {
                        AddBoldMarkupLastWeek("🏠 Last Week – No tracker time for approved WFH");
                        messageBuilderLastWeek.AppendLine("No WFH tracker time issues found");
                        currentPosLastWeek += "No WFH tracker time issues found".Length + Environment.NewLine.Length;
                    }

                    messageBuilderLastWeek.AppendLine("════════════════════════════════════");
                    currentPosLastWeek += "════════════════════════════════════".Length + Environment.NewLine.Length;

                    var messageTextLastWeek = messageBuilderLastWeek.ToString();
                    if (!string.IsNullOrEmpty(messageTextLastWeek))
                    {
                        _mezonService.Post(input.botUri, new
                        {
                            type = "hook",
                            message = new
                            {
                                t = messageTextLastWeek,
                                mk = mkListLastWeek
                            }
                        });
                    }
                }
            }

            return true;
        }
    }
}