using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.OverTimeHours.Dto
{
    public class GetOverTimeHourHRMDto
    {
        public long UserId { get; set; }
        public string NormalizedEmailAddress { get; set; }
        public string Branch { get; set; }
        public List<OverTimeHourHRMDto> ListOverTimeHour { get; set; }
    }

    public class OverTimeHourHRMDto
    {
        public int Day { get; set; }
        public double WorkingHour { get; set; }
        public double Coefficient { get; set; }
        public double OTHour { get; set; }
        public double OTHourWithCoefficient
        {
            get
            {
                return OTHour * Coefficient;
            }
        }

    }
}
