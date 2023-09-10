using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.APIs.Capabilities.Dto
{
    [AutoMapTo(typeof(Capability))]
    public class GetCapabilityDto : Entity<long>
    {
        [ApplySearchAttribute]
        public string Name { get; set; }
        public CapabilityType Type { get; set; }
        public string Note { get; set; }
        public object ApplySetting { get; set; }
    }
}