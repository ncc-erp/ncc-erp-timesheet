using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRM.Dto
{
    public class GetTimekeepingByMonthDto
    {
        public long? UserId { get; set; }
        public string NormalizedEmailAddress { get; set; }
        public int NumbleOfCheckInLateOrNoCheckIn { get; set; }

    }
}
