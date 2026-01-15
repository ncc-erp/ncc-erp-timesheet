using System.Collections.Generic;

namespace Timesheet.Timesheets.Timesheets.Dto
{
    public class RejectTimesheetDto
    {
        public List<long> Ids { get; set; }
        public string Reason { get; set; }
    }
}
