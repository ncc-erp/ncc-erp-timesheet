using Abp.AutoMapper;
using Abp.Domain.Entities;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.Projects.Dto
{
    [AutoMapTo(typeof(ProjectUser))]
    public class ProjectUsersDto :Entity<long>
    {
        public long UserId { get; set; }
        public ProjectUserType Type { get; set; }
        public bool IsTemp { get; set; }
    }
}
