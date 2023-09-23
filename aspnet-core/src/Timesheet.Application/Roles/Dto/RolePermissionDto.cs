using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Authorization.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Roles.Dto
{
    [AutoMap(typeof(Role))]
    public class RolePermissionDto : EntityDto<int>
    {
        public List<string> Permissions { get; set; }
    }
}
