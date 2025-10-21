using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Entities
{
    public class UserPunishmentPaid : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        
        public DateTime DateAt { get; set; }
        
        public int Amount { get; set; }
        
        [MaxLength(255)]
        public string TxHash { get; set; }
    }
}
