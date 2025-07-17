using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timekeepings.Dto
{
    public class UserPunishmentDailyDto
    {
        public int DailyLate { get; set; }
        public int DailyNoCheckIn { get; set; }
        public int DailyNoCheckOut { get; set; }
        public int DailyLateAndNoCheckOut { get; set; }
        public int DailyNoCheckInAndNoCheckOut { get; set; }
        public int DailyDaily { get; set; }
        public int DailyMention { get; set; }
        public int DailyTracker20k { get; set; }
        public int DailyTracker50k { get; set; }
        public int DailyTracker100k { get; set; }
        public int DailyTracker200k { get; set; }
        public int TotalDayPunishment { get; set; }
        public int TotalDayPunishmentTotal { get; set; }
    }
}
