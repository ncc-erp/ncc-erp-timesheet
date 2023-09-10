using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class TimesheetUserDto
    {
        public long UserId { get; set; }
        public bool IsStopWork { get; set; }
        public DateTime? StopWorkingDate { get; set; }
        public string EmailAddress { get; set; }
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public double? MorningWorking { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
        public double? AfternoonWorking { get; set; }
        public string UserName => EmailAddress.Contains("@") ? EmailAddress.Substring(0, EmailAddress.IndexOf("@")) : EmailAddress;
    }
}
