using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.TeamBuildingRequestHistories.dto
{
    [AutoMapTo(typeof(TeamBuildingRequestHistoryFile))]
    public class EditOneInvoiceDto : EntityDto<long>
    {
        public float? InvoiceAmount { get; set; }
        public bool? HasVAT { get; set; }
    }
}
