using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Public.Dto
{
    public class TimesheetChartDto
    {
        public List<string> Labels { get; set; }
        public List<float> NormalWoringHours { get; set; }
        public List<float> OverTimeHours { get; set; }
        public List<float> OTNoChargeHours { get; set; }
    }
}
