using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

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
        [MaxLength(50)]
        public string Type { get; set; } 

        public int Money { get; set; }

        public bool IsActive { get; set; } = true;
    }
}