using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class NotifyHRTheEmployeeMayHaveLeftConfigDto
    {
        public string NotifyHRTheEmployeeMayHaveLeftEnableWorker { get; set; }
        public string NotifyHRTheEmployeeMayHaveLeftAtHour { get; set; }
        public string NotifyHRTheEmployeeMayHaveLeftToChannels { get; set; }
        public string NotifyHRTheEmployeeMayHaveLeftToHREmail { get; set; }
        public string NotifyHRTheEmployeeMayHaveLeftTimePeriod { get; set; }
    }
}
