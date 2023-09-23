using System;
using System.Collections.Generic;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class InputRequestDto
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<long> projectIds { get; set; }
        public string name { get; set; }
        public RequestType? type { get; set; }
        public int dayOffTypeId { get; set; }
        public int? status { get; set; }
        public DayType? dayType { get; set; }
        public long? BranchId { get; set; }

    }
}
