using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRMv2.Dto
{
    public class GetNormalWorkingForHRMv2Dto
    {
            public string NormalizedEmailAddress { get; set; }
            public double TotalWorkingHourOfMonth { get; set; }
            public double TotalOpenTalk { get; set; }
            public double TotalOffHour { get; set; }
            public IEnumerable<ListUserNormalWorkingDto> ListWorkingHour { get; set; }
            public double TotalWorkingHour
            {
                get
                {
                    return TotalWorkingHourOfMonth + (TotalOpenTalk <= 2 ? TotalOpenTalk * 4 : 8);
                }
            }
    }
    public class ListUserNormalWorkingDto
    {
        public int Day { get; set; }
        public string DayName { get; set; }
        public double WorkingHour { get; set; }
        public bool IsOpenTalk { get; set; }
    }
}
