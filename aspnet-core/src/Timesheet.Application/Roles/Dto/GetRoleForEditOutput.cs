using Ncc.Authorization;
using System.Collections.Generic;
using Timesheet.Users.Dto;

namespace Ncc.Roles.Dto
{
    public class GetRoleForEditOutput
    {
        public RoleEditDto Role { get; set; }

        public List<SystemPermission> Permissions { get; set; }

        public List<string> GrantedPermissionNames { get; set; }
       
        public List<Timesheet.DomainServices.Dto.GetUserDto> Users { get; set; }
    }
}