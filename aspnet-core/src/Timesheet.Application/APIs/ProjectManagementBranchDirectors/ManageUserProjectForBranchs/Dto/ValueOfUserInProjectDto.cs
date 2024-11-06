using Ncc.Entities;
using System.Collections.Generic;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs.Dto
{
    public class ValueOfUserInProjectDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public ProjectStatus Status { get; set; }
        //public ValueOfUserType ValueOfUserType { get; set; }
        public ProjectUserType ProjectUserType { get; set; }
        public float Effort { get; set; }
        public float WorkingHours { get; set; }
    }
    public class WorkTimeByProjectDto
    {
        public List<ValueOfUserInProjectDto> AllValueOfUserInProjectDtos { get; set; }
        public float TotalWorkingHours { get; set; }
    }
}
