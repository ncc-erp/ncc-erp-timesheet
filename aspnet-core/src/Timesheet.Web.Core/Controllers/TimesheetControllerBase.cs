using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Ncc.Controllers
{
    public abstract class TimesheetControllerBase: AbpController
    {
        protected TimesheetControllerBase()
        {
            LocalizationSourceName = TimesheetConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
