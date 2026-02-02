using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class PMOtherPunishSettingDto
    {
        public bool enable { get; set; }      
        public int hour { get; set; }
        public int dayOfMonth { get; set; }
        public string adminClanName { get; set; }
    }
}
