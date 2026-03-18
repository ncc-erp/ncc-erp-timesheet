using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class WhitelistSystem : FullAuditedEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
        [Required]
        [MaxLength(256)]
        [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Code must be uppercase letters, numbers, and underscores only.")]
        public string Code { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [Required]
        public WhitelistType Type { get; set; }
        public bool IsActive { get; set; }
    }
}
