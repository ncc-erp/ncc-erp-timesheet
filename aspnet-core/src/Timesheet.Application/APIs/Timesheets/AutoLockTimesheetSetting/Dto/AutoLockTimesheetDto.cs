using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Timesheets.AutoLockTimesheetSetting.Dto
{
    public class AutoLockTimesheetSettingDto
    {
        public string LockDayOfUser { get; set; }
        public string LockHourOfUser { get; set; }
        public string LockMinuteOfUser { get; set; }
        public string LockDayOfPM { get; set; }
        public string LockHourOfPM { get; set; }
        public string LockMinuteOfPM { get; set; }
        public string LockDayAfterUnlock { get; set; }
        public string LockHourAfterUnlock { get; set; }
        public string LockMinuteAfterUnlock { get; set; }
    }
}
