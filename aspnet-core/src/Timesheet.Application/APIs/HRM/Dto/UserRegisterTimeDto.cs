
using System;

namespace Timesheet.APIs.HRM.Dto
{
    public class UserRegisterTimeDto
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public string MorningStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
        public DateTime? DateChangeWorkingTime { get; set; }
    }
}
