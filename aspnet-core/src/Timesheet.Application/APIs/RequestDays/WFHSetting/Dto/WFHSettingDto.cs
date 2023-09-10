using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.MyAbsenceDays.WFHSetting.Dto
{
    public class WFHSettingDto
    {
        public string numOfRemoteDays {get; set;}
        public string allowInternToWorkRemote { get; set;}
        public string totalTimeTardinessAndEarlyLeave  { get; set; }
        
    }
}