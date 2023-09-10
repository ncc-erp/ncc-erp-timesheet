using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.APIs.CapabilitySettings.Dto
{
    [AutoMapTo(typeof(CapabilitySetting))]
    public class CreateUpdateCapabilitySettingDto : Entity<long>
    {
        public Usertype UserType { get; set; }
        public long? PositionId { get; set; }
        public long? CapabilityId { get; set; }
        public CapabilityType CapabilityType {get; set; }
        //public string Note { get; set; }
        public string GuildeLine { get; set; }
        public float Coefficient { get; set; } = 1;
    }
}