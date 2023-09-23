using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Public.Dto
{
    public class EffortChartDto
    {
        public List<string> Labels { get; set; }
        public List<float> NormalWorkingHours { get; set; }
        public List<double> OTxCofficientHours { get; set; }
        public List<double> ManDays { get; set; }
    }
}
