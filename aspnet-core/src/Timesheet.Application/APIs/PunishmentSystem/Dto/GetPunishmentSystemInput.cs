using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.PunishmentSystems.Dto
{
    public class GetPunishmentSystemsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; } 
        public string Type { get; set; } 
        public bool? IsActive { get; set; } 
    }
}