using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Ncc.Authorization.Users;
using Timesheet.Anotations;

using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UpdateRoleDto : EntityDto<long>
    {
        public string[] RoleNames { get; set; }
    }
    
}

