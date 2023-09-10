using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.HRM.Dto
{
    public class GetPunishedUserDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public LockUnlockTimesheetType UnlockType;
    }
}
