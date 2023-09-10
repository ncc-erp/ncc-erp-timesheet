using Abp.Application.Services.Dto;
using System.Collections.Generic;
using Timesheet.APIs.TeamBuildingDetailsPM.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingRequestMoney.dto
{
    public class ReturnRequestHistoryDto :EntityDto<long>
    {
        public long RequesterId { get; set; }
        public string TitleRequest { get; set; }
        public float RequestMoney { get; set; }
        public float? DisbursedMoney { get; set; }
        public float? RemainingMoney { get; set; }
        public RemainingMoneyStatus RemainingMoneyStatus { get; set; }
        public TeamBuildingRequestStatus Status { get; set; }
        public string Note { get; set; }
        public List<TeamBuildingRequestHistoryFileDto> TeamBuildingRequestHistoryFileDtos { get; set; } 
    }
}
