using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.Entities
{
    public class CapabilitySetting : FullAuditedEntity<long>
    {
        public long CapabilityId { get; set; }
        [ForeignKey(nameof(CapabilityId))]
        public Capability Capability { get; set; }
        public long? PositionId { get; set; }
        [ForeignKey(nameof(PositionId))]
        public Position Position { get; set; }
        public string GuildeLine { get; set; }
        public float Coefficient { get; set; }
        public Usertype UserType { get; set; }
    }
}