using System.Collections.Generic;

namespace Timesheet.APIs.TeamBuildingRequestHistories.dto
{
    public class DisburseTeamBuildingRequestDto
    {
        public long RequestId { get; set; }
        public float DisburseMoney { get; set; }
        public long RequesterId { get; set; }
        public List<DisburseTeamBuildingInvoiceRequestDto> InvoiceDisburseList { get; set; }
    }

    public class DisburseTeamBuildingInvoiceRequestDto
    {
        public long InvoiceId { get; set; }
        public bool HasVAT { get; set; }
    }
}
