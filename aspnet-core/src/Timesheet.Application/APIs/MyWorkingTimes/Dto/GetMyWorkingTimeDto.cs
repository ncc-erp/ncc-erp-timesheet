using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.MyWorkingTimes.Dto
{
    public class GetMyWorkingTimeDto
    {
        public string MorningStartTime { get; set; }
        public string MorningEndTime { get; set; }
        public double MorningWorkingTime { get; set; }
        public string AfternoonStartTime { get; set; }
        public string AfternoonEndTime { get; set; }
        public double AfternoonWorkingTime { get; set; }
    }
}
