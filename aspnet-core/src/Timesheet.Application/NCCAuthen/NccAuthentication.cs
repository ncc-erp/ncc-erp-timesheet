using Abp.Configuration;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Configuration;
using Ncc.MultiTenancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.NCCAuthen
{
    public class NccAuthentication : ActionFilterAttribute
    {
        private readonly TenantManager _tenantManager;
        private readonly IAbpSession _abpSession;
        private readonly ISettingManager _settingManager;

        public NccAuthentication()
        {
            _tenantManager = IocManager.Instance.Resolve<TenantManager>();
            _settingManager = IocManager.Instance.Resolve<ISettingManager>();
            _abpSession = NullAbpSession.Instance;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var secretCode = _settingManager.GetSettingValueForApplication(AppSettingNames.SecurityCode);

            var header = context.HttpContext.Request.Headers;
            var securityCodeHeader = header["X-Secret-Key"].ToString();

            if (string.IsNullOrEmpty(securityCodeHeader))
            {
                securityCodeHeader = header["securityCode"];
            }

            if (secretCode != securityCodeHeader)
                throw new UserFriendlyException($"SecretCode does not match! Timesheet: {secretCode.Substring(secretCode.Length - 3)} != {securityCodeHeader}");

            var abpTenantName = header["Abp-TenantName"].ToString();
            if (string.IsNullOrEmpty(abpTenantName)) return;

            var tenant = _tenantManager.FindByTenancyName(abpTenantName);
            if (tenant == null)
                throw new Exception($"Not Found Tenant.");

            _abpSession.Use(tenant.Id, null);
        }
    }
}
