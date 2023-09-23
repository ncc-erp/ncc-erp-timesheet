using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRM.Dto
{
    public class PunishmentUnlockTSDto
    {
        public long UserId { get; set; }
        public int Times { get; set; }
        public double Amount { get; set; }
        public string NormalizedEmailAddress { get; set; }
    }
}
