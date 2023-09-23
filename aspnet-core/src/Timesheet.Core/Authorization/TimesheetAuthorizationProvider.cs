using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace Ncc.Authorization
{
    public class TimesheetAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            foreach (var permission in SystemPermission.ListPermissions)
            {
                context.CreatePermission(permission.Name, L(permission.DisplayName), multiTenancySides: permission.MultiTenancySides);
            }
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, TimesheetConsts.LocalizationSourceName);
        }
    }
}
