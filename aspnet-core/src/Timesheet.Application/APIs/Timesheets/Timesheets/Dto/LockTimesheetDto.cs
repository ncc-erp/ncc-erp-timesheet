using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timesheets.Timesheets.Dto
{
    public class LockTimesheetDto
    {
        public long UserId { get; set; }
        public LockUnlockTimesheetType Type { get; set; }
    }
}
