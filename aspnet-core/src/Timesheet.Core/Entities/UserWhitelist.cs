using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Entities
{
    public class UserWhitelist : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public long WhitelistSystemId { get; set; }
        [ForeignKey(nameof(WhitelistSystemId))]
        public WhitelistSystem WhitelistSystem { get; set; }
    }
}
