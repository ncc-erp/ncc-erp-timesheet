using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRM.Dto
{
    public class UserNormalWokingTimeOfDayDto
    {
        public int Day { get; set; }
        public DayOfWeek DayName { get; set; }
        public long UserId { get; set; }
        public int NormalWokingTime { get; set; }
        public double OffHour { get; set; }
        public bool IsOpenTalk => DayName == DayOfWeek.Saturday && (double)NormalWokingTime / 60 >= 4;
        public double TotalHourWokinngOfDay => !IsNormalDay ? 4 : Math.Min(8 - OffHour, (double)NormalWokingTime / 60);
        public bool IsNormalDay { get; set; }

    }
}