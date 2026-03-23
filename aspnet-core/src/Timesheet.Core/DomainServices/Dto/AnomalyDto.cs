using System;
using System.Collections.Generic;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class YesterdayAnomalyDTO
    {
        public long UserId { get; set; }
        public string EmployeeName { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public string ActualHours { get; set; }
        public string Notes { get; set; }
        public BranchToDisplayDto Branch { get; set; }
    }

    public class DateWithNoteDto
    {
        public string Date { get; set; }
        public string Note { get; set; }
    }

    public class LastWeekAnomalyDTO
    {
        public long UserId { get; set; }
        public string EmployeeName { get; set; }
        public string UserName { get; set; }
        public List<DateWithNoteDto> DatesMissed { get; set; } = new List<DateWithNoteDto>();
        public List<DateWithNoteDto> DatesNoTrackerTime { get; set; } = new List<DateWithNoteDto>();
        public List<DateWithNoteDto> DatesBelowThreshold { get; set; } = new List<DateWithNoteDto>();
        public int Count { get; set; }
        public BranchToDisplayDto Branch { get; set; }
    }

    public class TimekeepingDto
    {
        public long? UserId { get; set; }
        public DateTime DateAt { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string TrackerTime { get; set; }
    }

    public class AbsenceRequestDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public RequestStatus Status { get; set; }
        public RequestType Type { get; set; }
    }
    public class AbsenceDetailDto
    {
        public long RequestId { get; set; }
        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public OnDayType? AbsenceTime { get; set; }
        public double Hour { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public RequestType RequestType { get; set; }
        public long UserId { get; set; }
    }
    public class AnomalyData
    {
        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public List<TimekeepingDto> Timekeepings { get; set; } = new List<TimekeepingDto>();
        public List<AbsenceDetailDto> AbsenceDetails { get; set; } = new List<AbsenceDetailDto>();
    }
    public class UserWithAnomalyDto
    {
        public long UserId { get; set; }
        public string EmployeeName { get; set; }
        public string UserName { get; set; }
        public DateTime DateAt { get; set; }
        public BranchToDisplayDto Branch { get; set; }
        public bool IsYesterday { get; set; }
    }
    public class CalculateOfficeWorkingTimeDto
    {
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }
        public TimeSpan? MorningStartAt { get; set; }
        public TimeSpan? AfternoonStartAt { get; set; }
        public bool IsMorningAbsence { get; set; }
        public bool IsAfternoonAbsence { get; set; }
        public double GracePeriodMinutes { get; set; }
        public double BreakTime { get; set; }
    }
    public class TrackerTimeInputDto
    {
        public string TrackerTimeStr { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? MorningStartAt { get; set; }
        public TimeSpan? AfternoonStartAt { get; set; }
        public double GracePeriodMinutes { get; set; }
    }
    public class AnomalyInputDto
    {
        public long UserId { get; set; }
        public string EmployeeName { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public double? TotalWorkingTime { get; set; }
        public string Notes { get; set; }
        public bool IsYesterdayInFunction { get; set; }
        public ViolationStatus ViolationType { get; set; }
        public BranchToDisplayDto Branch { get; set; }
    }
    public class ProcessAnomaliesInputDto
    {
        public List<UserDto> AllUsers { get; set; }
        public List<TimekeepingDto> AllTimekeepings { get; set; }
        public List<AbsenceRequestDto> AllAbsenceRequests { get; set; }
        public List<AbsenceDetailDto> AllAbsenceDetails { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsYesterday { get; set; }
    }
}