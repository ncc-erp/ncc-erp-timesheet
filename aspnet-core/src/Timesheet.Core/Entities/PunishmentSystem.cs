using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class PunishmentSystem : FullAuditedEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }
        [Required]
        public UserPunishmentType Type { get; set; } 

        public int Money { get; set; }

        public bool IsActive { get; set; } = true;
    }
}