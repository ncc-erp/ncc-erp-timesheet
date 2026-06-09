using System;
using System.Collections.Generic;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class GetRequestOffViolationResultDto
    {
        public List<RequestOffViolationItemDto> TwoOrLessDaysViolations { get; set; } = new List<RequestOffViolationItemDto>();
        public List<RequestOffViolationItemDto> MoreThanTwoDaysViolations { get; set; } = new List<RequestOffViolationItemDto>();
    }

    public class RequestOffViolationItemDto
    {
        public long RequestId { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime RequestCreatedAt { get; set; }
        public DateTime DeadlineAt { get; set; }
        public int TotalOffDays { get; set; }
        public List<DateTime> OffDates { get; set; }
    }
}
