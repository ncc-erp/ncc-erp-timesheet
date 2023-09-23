using Abp.Domain.Entities;
using Timesheet.Anotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.TeamBuildingProject.Dto
{
    public class TeamBuildingProjectDto :Entity<long>
    {  
        [ApplySearch]
        public string Name { get; set; }
        public ProjectType ProjectType { get; set; }
        public string PMEmail { get; set; }
        public bool IsAllowTeamBuilding { get; set; }
    }
}
