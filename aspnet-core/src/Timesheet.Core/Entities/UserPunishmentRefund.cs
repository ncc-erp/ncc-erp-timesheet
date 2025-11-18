using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System.ComponentModel.DataAnnotations.Schema;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class UserPunishmentRefund : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public long? UserPunishmentId { get; set; }
        [ForeignKey(nameof(UserPunishmentId))]
        public UserPunishment UserPunishment { get; set; }
        public int Points { get; set; }
        public PointType Type { get; set; }
    }
}
