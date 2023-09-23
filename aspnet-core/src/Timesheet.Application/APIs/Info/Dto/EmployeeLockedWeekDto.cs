using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Info.Dto
{
    public class UserLockedTimesheetDto
    {
        public double Amount { get; set; }
        public double AmountPM { get; set; }
        public int LockedPM { get; set; }
        public bool IsUnlockLog { get; set; }
        public bool IsUnlockApprove { get; set; }
        public bool IsPM { get; set; }
        public List<EmployeeLockedWeekDto> LockedEmployee { get; set; }
        public string FirstDateCanLogIfUnlock { get; set; }
    }
    public class EmployeeLockedWeekDto
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<string> Day { get; set; }
    }
}
