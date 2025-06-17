using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.PunishmentSystems.Dto
{
    public class GetPunishmentSystemsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; } // Để tìm kiếm theo Name, Description, Type
        public string Type { get; set; } // Lọc theo Type cụ thể
        public bool? IsActive { get; set; } // Lọc theo trạng thái Active
    }
}