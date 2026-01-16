using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Public.Dto
{
    public class InputGetEffortMonthlyChartDto
    {
        public string ProjectCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
