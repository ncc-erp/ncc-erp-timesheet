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

    public class LastWeekAnomalyDTO
    {
        public long UserId { get; set; }
        public string EmployeeName { get; set; }
        public string UserName { get; set; }
        public List<string> DatesMissed { get; set; } = new List<string>();
        public List<string> DatesNoTrackerTime { get; set; } = new List<string>();
        public List<string> DatesBelowThreshold { get; set; } = new List<string>();
        public int Count { get; set; }
        public string Notes { get; set; }
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
        public List<AbsenceRequestDto> AbsenceRequests { get; set; } = new List<AbsenceRequestDto>();
        public List<AbsenceDetailDto> AbsenceDetails { get; set; } = new List<AbsenceDetailDto>();
    }
    public class BranchToDisplayDto
    {
        public string BranchName { get; set; }
        public string BranchColor { get; set; }
    }
}