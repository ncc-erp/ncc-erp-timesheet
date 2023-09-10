using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class SendMessageRequestPendingTeamBuildingToHRConfigDto
    {
        public string SendMessageRequestPendingTeamBuildingToHREnableWorker { get; set; }
        public string SendMessageRequestPendingTeamBuildingToHRAtHour { get; set; }
        public string SendMessageRequestPendingTeamBuildingToHRToChannels { get; set; }
        public string SendMessageRequestPendingTeamBuildingToHREmail { get; set; }
    }
}
