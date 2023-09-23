using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Public.Dto
{
    public class TimesheetUserGroupChartDto
    {
        public List<string> Labels { get; set; }
        public List<float> OfficalNormalWoringHours { get; set; }
        public List<float> OffcialOverTimeHours { get; set; }
        public List<float> PoolNormalWoringHours { get; set; }
        public List<float> PoolOverTimeHours { get; set; }
    }
}
