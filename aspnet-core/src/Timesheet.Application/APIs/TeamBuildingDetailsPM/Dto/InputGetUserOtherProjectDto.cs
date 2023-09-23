using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.TeamBuildingDetailsPM.Dto
{
    public class InputGetUserOtherProjectDto
    {
        public List<long> Ids { get; set; }
        public long? BranchId { get; set; }
        public string SearchText { get; set; }
    }
}
