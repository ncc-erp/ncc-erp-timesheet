using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.TeamBuildingRequestHistories.dto
{
    public class DisburseTeamBuildingRequestInfoDto
    {
        public long RequestId { get; set; }
        public float RequestMoney { get; set; }
        public string RequesterEmail { get; set; }
        public string RequesterName { get; set; }
        public List<InvoiceRequestOfDisburseTeamBuidingRequestDto> InvoiceRequests { get; set; }
    }

    public class InvoiceRequestOfDisburseTeamBuidingRequestDto
    {
        public long InvoiceId { get; set; }
        public float? InvoiceMoney { get; set; }
        public string InvoiceResourceName { get; set; }
        public string InvoiceResourceUrl { get; set; }
        public bool? HasVAT { get; set; }
    }
}
