using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.PunishmentSystems.Dto
{
    public class GetAllActivePunishmentSystemsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; } 
        public UserPunishmentType Type { get; set; } 
        public bool? IsActive { get; set; } 
    }
}