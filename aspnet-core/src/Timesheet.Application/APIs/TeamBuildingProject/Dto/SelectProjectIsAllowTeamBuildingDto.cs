using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Timesheet.APIs.TeamBuildingProject.Dto
{
    public class SelectProjectIsAllowTeamBuildingDto
    {
        public long ProjectId { get; set; }
        public bool IsAllowTeamBuilding { get; set; }
    }
}
