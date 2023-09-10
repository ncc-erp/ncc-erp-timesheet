using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.LogTimesheetInFutureSetting.Dto
{
    public class LogTimesheetDto
    {
        public string CanLogTimesheetInFuture { get; set; }
        public string DayAllowLogTimesheetInFuture { get; set; }
        public string DateToLockTimesheetOfLastMonth { get; set; }
        public string MaxTimeOfDayLogTimesheet { get; set; }
    }
}
