using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Timesheet.APIs.TeamBuildingRequestMoney.dto
{

    public class SubmitRequestMoneyDto
    {
        public string PMRequest { get; set; }
        public List<IFormFile> ListFile { get; set; }
    }
    public class PMRequestDto
    {
        public string TitleRequest { get; set; }
        public List<long> ListDetailId { get; set; }
        public string Note { get; set; }
        public float InvoiceAmount { get; set; }
        public List<InvoiceRequestDto> ListInvoiceRequestDto { get; set; }
    }

    public class InvoiceRequestDto
    {
        public string InvoiceImageName { get; set; }
        public string InvoiceUrl { get; set; }
        public float Amount { get; set; }
        public bool HasVat { get; set; }
    }

    public class EditRequestDto
    {
        public long Id { get; set; }
        public List<long> ListDetailId { get; set; }
    }
}
