
using System;
using System.Collections.Generic;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.HRM.Dto
{
    public class UserWorkFromHomeDto
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? DateAt { get; set; }
        public string Status { get; set; }
        public DateTime CreationTime { get; set; }
        public DayType DateType { get; set; }
        public string DateTypeName => ((DayType)DateType).ToString();
    }
}

