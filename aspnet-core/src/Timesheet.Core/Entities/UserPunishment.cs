using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Entities
{
    public class UserPunishment : FullAuditedEntity<long>
    {
        public DateTime DateAt { get; set; }
        public long? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        [ForeignKey(nameof(PunishmentSystemId))]
        public PunishmentSystem PunishmentSystem { get; set; }
        public long PunishmentSystemId { get; set; }
        public string Type { get; set; } // ANT, UnlockTS, ...
        public int Count { get; set; }
        public int TotalMoney { get; set; }
        [MaxLength(1000)]
        public string UserNote { get; set; }
        [MaxLength(1000)]
        public string NoteReply { get; set; }
    }
}