using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs.Dto
{
    public class GetAllValueOfUserInProjectByUserIdDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public ValueOfUserType ValueOfUserType { get; set; }
        public float ShadowPercentage { get; set; }
        public float WorkingHours { get; set; }
    }
}
