using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.AutoSubmitTimesheetSetting.Dto;

namespace Timesheet.APIs.Timesheets.AutoSubmitTimesheetSetting
{
    public class AutoSubmitTimesheetSetting : AppServiceBase
    {
        public AutoSubmitTimesheetSetting(IWorkScope workScope) : base(workScope)
        {

        }

        public async Task<AutoSubmitTimesheetDto> Get()
        {
            return new AutoSubmitTimesheetDto
            {
                AutoSubmitTimesheet = await SettingManager.GetSettingValueAsync(AppSettingNames.AutoSubmitTimesheet),
                AutoSubmitAt = await SettingManager.GetSettingValueAsync(AppSettingNames.AutoSubmitAt)
            };
        }

        public async Task<AutoSubmitTimesheetDto> Change(AutoSubmitTimesheetDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AutoSubmitTimesheet, input.AutoSubmitTimesheet);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AutoSubmitAt, input.AutoSubmitAt);
            return input;
        }
    }
}
