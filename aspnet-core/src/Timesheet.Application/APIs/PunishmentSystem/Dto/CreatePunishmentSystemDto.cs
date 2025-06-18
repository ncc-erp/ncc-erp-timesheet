using Abp.AutoMapper;
using System.ComponentModel.DataAnnotations;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.PunishmentSystems.Dto
{
    [AutoMapTo(typeof(PunishmentSystem))]
    public class CreatePunishmentSystemDto
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public UserPunishmentType Type { get; set; } 

        [Range(0, int.MaxValue)]
        public int Money { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
