using System;

namespace Timesheet.APIs.UserPunishmentPaids.Dto
{
    public class GetUserPunishmentPaidInput
    {
        public long? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
