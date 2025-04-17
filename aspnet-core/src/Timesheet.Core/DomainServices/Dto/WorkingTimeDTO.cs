using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class WorkingTimeDto
    {
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
    }
}
