using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Entities
{
    public class RemoteBlacklist : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public int PenaltyDays { get; set; }
    }
}
