using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    public class TimeStatisticProjectDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int BillableWorkingTime { get; set; }
        public int TotalWorkingTime { get; set; }
    }
}
