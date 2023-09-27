using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs.Dto
{
    [AutoMapTo(typeof(Entities.ValueOfUserInProject))]
    public class CreateValueOfUserDto : Entity<long>
    {
        public long UserId { get; set; }
        public long ProjectId { get; set; }
        public ValueOfUserType Type { get; set; }
        public float ShadowPercentage { get; set; }
    }
}
