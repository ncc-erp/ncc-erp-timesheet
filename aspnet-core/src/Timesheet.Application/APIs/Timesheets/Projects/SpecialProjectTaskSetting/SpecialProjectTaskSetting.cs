using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.Projects.SpecialProjectTaskSetting.Dto;

namespace Timesheet.APIs.Timesheets.Projects.SpecialProjectTaskSetting
{
    [AbpAuthorize]
    public class SpecialProjectTaskSetting : AppServiceBase
    {
        public SpecialProjectTaskSetting(IWorkScope workScope) : base(workScope)
        {

        }
        public async Task<SpecialProjectTaskSettingDto> Get()
        {
            return new SpecialProjectTaskSettingDto
            {
                ProjectTaskId = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OpenTalkTaskId)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SpecialProjectTaskSetting_Edit)]
        public async Task<SpecialProjectTaskSettingDto> Change(SpecialProjectTaskSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.OpenTalkTaskId, input.ProjectTaskId);
            return input;
        }

    }

}
