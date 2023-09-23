using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class ApproveTimesheetNotifyConfigDto
    {
        public string ApproveTimesheetNotifyEnableWorker { get; set; }
        public string ApproveTimesheetNotifyAtHour { get; set; }
        public string ApproveTimesheetNotifyOnDates { get; set; }
        public string ApproveTimesheetNotifyToChannels { get; set; }
        public string ApproveTimesheetNotifyTimePeriodWithPendingRequest { get; set; }
    }
}
