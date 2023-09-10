using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.SendKomuPunishedUserCheckInOutSetting.Dto
{
    public class KomuSendNotifyPunishedUser
    {
        public string TimeSendPunishUser { get; set; }
        public string ChannelNotifyPunishUser { get; set; }
        public string PercentOfTrackerOnWorking { get; set; }
    }
}
