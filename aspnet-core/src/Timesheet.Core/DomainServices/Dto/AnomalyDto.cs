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
        public double ActualHours { get; set; }
        public string Notes { get; set; }
        public BranchToDisplayDto Branch { get; set; }
        public bool IsUnplannedAbsence { get; set; }
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
        public UserDto User { get; set; }
    }

    public class AbsenceDetailDto
    {
        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public RequestType RequestType { get; set; }
        public long UserId { get; set; }
    }
    public class AnomalyData
    {
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
    public class ProcessAnomaliesInputDto
    {
        public List<TimekeepingDto> AllTimekeepings { get; set; }
        public List<AbsenceDetailDto> AllAbsenceDetails { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsYesterday { get; set; }
    }
}