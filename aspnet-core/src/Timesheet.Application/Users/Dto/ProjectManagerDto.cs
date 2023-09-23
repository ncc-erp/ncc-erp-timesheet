using Abp.Application.Services.Dto;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Users.Dto
{

    public class ProjectManagerDto : EntityDto<long>
    {
        public virtual string Surname { get; set; }
        public virtual string Name { get; set; }
    }
}
