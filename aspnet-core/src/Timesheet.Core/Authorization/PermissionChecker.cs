using Abp.Authorization;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;

namespace Ncc.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
