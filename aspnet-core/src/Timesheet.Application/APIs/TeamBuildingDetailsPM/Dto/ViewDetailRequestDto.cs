using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.TeamBuildingDetailsPM.dto;
using Timesheet.APIs.TeamBuildingRequestMoney.dto;

namespace Timesheet.APIs.TeamBuildingDetailsPM.Dto
{
    public class ViewDetailRequestDto : SelectDetailPopupDto
    {
        public List<string> ListUrl { get; set; }
        public List<string> Files { get; set; }
    }
}
