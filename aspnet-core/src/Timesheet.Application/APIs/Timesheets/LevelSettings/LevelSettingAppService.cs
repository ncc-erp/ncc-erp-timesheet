using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.Dto.LevelSettings;

namespace Timesheet.APIs.Timesheets.LevelSettings
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration)]
    public class LevelSettingAppService : AppServiceBase
    {
        public LevelSettingAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_LevelSetting_View)]
        public async Task<LevelSettingBaseDto> Get()
        {
            return new LevelSettingBaseDto
            {
                UserLevelSetting = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.UserLevelSetting),
                PercentSalaryProbationary = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PercentSalaryProbationary)
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_LevelSetting_Edit)]
        public async Task<LevelSettingBaseDto> Set(LevelSettingBaseDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.UserLevelSetting, input.UserLevelSetting);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PercentSalaryProbationary, input.PercentSalaryProbationary);
            return input;
        }
    }
}
