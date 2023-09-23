using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Timesheets.MyTimesheets.Dto
{
    public class TimeStatisticCustomerDto
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalWorkingTime { get; set; }
        public int BillableWorkingTime { get; set; }
    }
}
