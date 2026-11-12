using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.MyAbsenceDays.WFHSetting.Dto
{
    public class WFHSettingDto
    {
        public string numOfRemoteDays {get; set;}
        public string allowInternToWorkRemote { get; set;}
        public string allowProbationToWorkRemote { get; set; }
        public string totalTimeTardinessAndEarlyLeave  { get; set; }
        public string officeDaysInAdvanceToRequestRemote { get; set; }
        public string wfhMaxOfficeDaysPerWeek { get; set; }
        public string wfhDaysInAdvanceToRequestOffice { get; set; }
    }
}