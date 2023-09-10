using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Users.Dto
{
    public class UpdateWorkingTimeDto 
    {
        public long UserId { get; set; }
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
    }
}
