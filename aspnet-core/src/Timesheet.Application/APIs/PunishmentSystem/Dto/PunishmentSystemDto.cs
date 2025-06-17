using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Timesheet.Entities;
using System;

namespace Timesheet.APIs.PunishmentSystems.Dto
{
    public class PunishmentSystemDto : FullAuditedEntityDto<long>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int Money { get; set; }
        public bool IsActive { get; set; }
    }
}
