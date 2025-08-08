using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timekeepings.Dto
{
    public class UserPunishmentSummaryDto
    {
        public int TotalLate { get; set; }
        public int TotalNoCheckIn { get; set; }
        public int TotalNoCheckOut { get; set; }
        public int TotalLateAndNoCheckOut { get; set; }
        public int TotalNoCheckInAndNoCheckOut { get; set; }
        public int TotalDaily { get; set; }
        public int TotalMention { get; set; }
        public int TotalTracker20k { get; set; }
        public int TotalTracker50k { get; set; }
        public int TotalTracker100k { get; set; }
        public int TotalTracker200k { get; set; }
        public int TotalAllDailyMoney { get; set; }
        public int TotalAllMentionMoney { get; set; }
        public int TotalMonthlyPunishment { get; set; }
    }
}
