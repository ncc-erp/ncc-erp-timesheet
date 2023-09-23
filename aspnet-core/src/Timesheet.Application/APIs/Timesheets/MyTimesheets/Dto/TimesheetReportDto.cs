using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    public class TimesheetReportDto
    {
        public int HoursTracked { get; set; }
        public int Billable { get; set; }
        public int NonBillable { get; set; }
        public int NormalWorkingHours { get; set; }
        public int OvertimeBillable { get; set; }
        public int OvertimeNonBillable { get; set; }
    }
}
