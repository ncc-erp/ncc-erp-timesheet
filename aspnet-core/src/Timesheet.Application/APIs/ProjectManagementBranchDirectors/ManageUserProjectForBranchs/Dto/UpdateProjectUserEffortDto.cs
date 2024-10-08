using Abp.AutoMapper;
using Abp.Domain.Entities;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs.Dto
{
    public class UpdateProjectUserEffortDto
    {
        public long ProjectId { get; set; }
        public long UserId { get; set; }
        public ProjectUserType Type { get; set; }
        public float Effort { get; set; }
    }
}
