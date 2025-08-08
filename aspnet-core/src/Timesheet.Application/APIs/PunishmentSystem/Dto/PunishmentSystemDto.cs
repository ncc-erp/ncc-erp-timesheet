using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Timesheet.Entities;
using System;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.PunishmentSystems.Dto
{
    public class PunishmentSystemDto : FullAuditedEntityDto<long>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public UserPunishmentType Type { get; set; }
        public int Money { get; set; }
        public bool IsActive { get; set; }
    }
}
