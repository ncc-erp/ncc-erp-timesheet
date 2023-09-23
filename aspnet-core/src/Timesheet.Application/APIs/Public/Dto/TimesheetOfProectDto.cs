using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Public.Dto
{
    public class TimesheetOfProectDto
    {
        public long ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public float NormalWoringHour { get; set; }
        public float OverTimeHour { get; set; }        
    }
}
