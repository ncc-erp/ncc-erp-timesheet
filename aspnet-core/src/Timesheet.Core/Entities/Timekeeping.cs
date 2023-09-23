using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class Timekeeping : FullAuditedEntity<long>
    {
        public long? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public string UserEmail { get; set; }//from faceId or Empty
        [MaxLength(5)]
        public string CheckIn { get; set; }//from faceId
        [MaxLength(5)]
        public string CheckOut { get; set; }//from faceId
        [MaxLength(5)]
        public string RegisterCheckIn { get; set; }
        [MaxLength(5)]
        public string RegisterCheckOut { get; set; }
        public DateTime DateAt { get; set; }
        public bool IsPunishedCheckIn { get; set; }
        public bool IsPunishedCheckOut { get; set; }
        public bool IsLocked { get; set; }
        public string UserNote { get; set; }
        public string NoteReply { get; set; }
        public CheckInCheckOutPunishmentType StatusPunish { get; set; }
        public int MoneyPunish { get; set; }
        public string TrackerTime { get; set; }
    }
}
