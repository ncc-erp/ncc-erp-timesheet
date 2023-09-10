using Abp.AutoMapper;
using Abp.Domain.Entities;
using Ncc.Authorization.Users;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.Projects.Dto
{
    [AutoMapTo(typeof(ProjectTargetUser))]
    public class ProjectTargetUserDto : Entity<long>
    {
        public long UserId { get; set; }
        public string RoleName { get; set; }
    }
}
