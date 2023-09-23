using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    public class TimeStatisticTaskDto
    {
        public long TaskId { get; set; }
        public string TaskName { get; set; }
        public int TotalWorkingTime { get; set; }
        public int BillableWorkingTime { get; set; }
        public bool Billable { get; set; }
    }
}
