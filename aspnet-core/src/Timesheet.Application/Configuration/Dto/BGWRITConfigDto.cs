using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class NRITConfigDto
    {
        public string NotifyEnableWorker { get; set; }
        public string NotifyAtHour { get; set; }
        public string NotifyReviewDeadline { get; set; }
        public string NotifyOnDates { get; set; }
        public string NotifyToChannels { get; set; }
        public string NotifyPenaltyFee { get; set; }
    }
}
