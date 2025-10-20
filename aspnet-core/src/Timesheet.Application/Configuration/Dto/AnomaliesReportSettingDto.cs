using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class AnomaliesReportSettingDto
    {
        public bool enable { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }
        public string dayofweek { get; set; }
        public string botUri { get; set; }
        public List<string> branchCodes { get; set; } = new List<string>();
    }
}
