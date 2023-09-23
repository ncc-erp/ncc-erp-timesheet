using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Timesheets.SingleSignOnSettings.Dto;

namespace Timesheet.Timesheets.SingleSignOnSetting
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration)]
    public class SingleSignOnConfigurationAppService : AppServiceBase
    {
        public SingleSignOnConfigurationAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_GoogleSignOn_View)]
        public async Task<SingleSignOnSettingDto> Get()
        {
            return new SingleSignOnSettingDto
            {
                ClientAppId = await SettingManager.GetSettingValueAsync(AppSettingNames.ClientAppId),
                RegisterSecretCode = await SettingManager.GetSettingValueAsync(AppSettingNames.SecretRegisterCode)
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_GoogleSignOn_Edit)]
        public async Task<SingleSignOnSettingDto> Change(SingleSignOnSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ClientAppId, input.ClientAppId);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SecretRegisterCode, input.RegisterSecretCode);
            return input;
        }
    }
}
