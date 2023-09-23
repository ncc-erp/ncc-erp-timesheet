using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.MyWorkingTimes.Dto
{
    public class UserWorkingTimeDto : GetMyWorkingTimeDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public bool? IsWorkingTimeDefault { get; set; }
    }
}
