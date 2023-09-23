using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.OverTimeHours.Dto
{
    public class GetOverTimeHourHRMv2Dto
    {
        public string NormalizedEmailAddress { get; set; }
        public List<OverTimeHourHRMv2Dto> ListOverTimeHour { get; set; }
    }

    public class OverTimeHourHRMv2Dto
    {
        public DateTime Date { get; set; }
        public double OTHour { get; set; }
    }
}
