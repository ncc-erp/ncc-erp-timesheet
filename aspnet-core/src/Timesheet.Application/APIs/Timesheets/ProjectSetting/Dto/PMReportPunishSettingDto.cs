using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.ProjectSetting.Dto
{
    public class PMReportPunishSettingDto
    {
        public bool enable { get; set; }      
        public int hour { get; set; } 
        public string dayofweek { get; set; }   
    }
}
