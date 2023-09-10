using System.Collections.Generic;
using Timesheet.APIs.TeamBuildingDetailsPM.dto;

namespace Timesheet.APIs.TeamBuildingRequestMoney.dto
{
    public class SelectDetailPopupDto
    {
        public float LastRemainMoney { get; set; }
        public List<SelectTeamBuildingDetailDto> TeamBuildingDetailDtos { get; set; }
        public string Note { get; set; }
        public float DisburseMoney { get; set; }
    }
}
