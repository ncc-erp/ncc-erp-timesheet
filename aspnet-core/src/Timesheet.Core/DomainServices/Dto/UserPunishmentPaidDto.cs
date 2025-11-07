using System;

namespace Timesheet.DomainServices.Dto
{
    public class UserPunishmentPaidDto
    {
        public DateTime DateAt { get; set; }
        public DateTime TargetMonth { get; set; }
        public int Amount { get; set; }
        public string TxHash { get; set; }
    }
}
