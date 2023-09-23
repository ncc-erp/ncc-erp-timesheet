using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class RetroNotifyConfigDto
    {
        public string RetroNotifyEnableWorker { get; set; }
        public string RetroNotifyAtHour { get; set; }
        public string RetroNotifyDeadline { get; set; }
        public string RetroNotifyOnDates { get; set; }
        public string RetroNotifyToChannels { get; set; }
    }
}
