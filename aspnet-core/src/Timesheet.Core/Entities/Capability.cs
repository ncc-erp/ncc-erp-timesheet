using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.Entities
{
    public class Capability : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public CapabilityType Type { get; set; }
        public string Note { get; set; }
        public ICollection<CapabilitySetting> CapabilitySettings { get; set; }
    }
}