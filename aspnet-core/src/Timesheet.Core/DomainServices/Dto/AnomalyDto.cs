using System;
using System.Collections.Generic;

namespace Timesheet.DomainServices.Dto
{
    public class YesterdayAnomalyDTO
    {
        public long UserId { get; set; }
        public string EmployeeName { get; set; }
        public string Date { get; set; }
        public string ActualHours { get; set; }
        public string Notes { get; set; }
    }

    public class LastWeekAnomalyDTO
    {
        public long UserId { get; set; }
        public string EmployeeName { get; set; }
        public List<string> DatesMissed { get; set; } = new List<string>();
        public List<string> DatesNoTrackerTime { get; set; } = new List<string>();
        public List<string> DatesBelowThreshold { get; set; } = new List<string>();
        public int Count { get; set; }
        public string Notes { get; set; }
    }
}