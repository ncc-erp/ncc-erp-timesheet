using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRMv2.Dto
{
    public class UpdateUserStatusDto: UpdateUserStatusFromHRMDto
    {
        public bool IsStopWork { get; set; }
        public bool IsActive { get; set; }

        public DateTime? StopWorkingTime { get; set; }
        
    }

    public class UpdateUserStatusFromHRMDto
    {
        public string EmailAddress { get; set; }

        public DateTime DateAt { get; set; }
    }
}
