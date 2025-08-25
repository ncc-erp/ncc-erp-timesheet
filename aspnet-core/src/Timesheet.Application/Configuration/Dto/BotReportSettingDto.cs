using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class BotReportSettingDto
    {
        public bool enable { get; set; }
        public bool everyday { get; set; }
        public int hour { get; set; } 
        public string dayofweek { get; set; }
        public string botUri { get; set; }
    }
}
