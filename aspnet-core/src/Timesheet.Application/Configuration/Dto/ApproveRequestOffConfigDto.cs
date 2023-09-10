using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class ApproveRequestOffConfigDto
    {
        public string ApproveRequestOffNotifyEnableWorker { get; set; }
        public string ApproveRequestOffNotifyAtHour { get; set; }
        public string ApproveRequestOffNotifyToChannels { get; set; }
        public string ApproveRequestOffNotifyTimePeriodWithPendingRequest { get; set; }
    }
}
