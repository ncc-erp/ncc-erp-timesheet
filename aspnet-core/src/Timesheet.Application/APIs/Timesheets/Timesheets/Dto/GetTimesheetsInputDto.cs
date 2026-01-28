using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timesheets.Timesheets.Dto
{
    public class GetTimesheetsInputDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimesheetStatus? Status { get; set; }
        public int? OpentalkTime { get; set; }
        public bool? OpentalkTimeType { get; set; }
        public long? ProjectId { get; set; }
        public long? BranchId { get; set; } = null;
        public HaveCheckInFilter? CheckInFilter { get; set; }
        public RequestType? WorkLocation { get; set; } = null;
        public TypeOfWork? TypeOfWork { get; set; } = null;
        public bool? IsCharged { get; set; } = null;
        public string SearchText { get; set; } = "";
        public long? UserId { get; set; }
    }
}
