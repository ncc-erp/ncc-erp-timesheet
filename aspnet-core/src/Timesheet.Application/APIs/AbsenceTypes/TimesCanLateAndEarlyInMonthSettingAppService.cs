using Abp.Authorization;
using Abp.Configuration;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.AbsenceTypes.Dto;
using Timesheet.APIs.Timesheets.GetDataFromFaceIDSetting.Dto;

namespace Timesheet.APIs.AbsenceTypes
{
    public class TimesCanLateAndEarlyInMonthSettingAppService : AppServiceBase
    {
        public TimesCanLateAndEarlyInMonthSettingAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_View)]
        public async Task<TimesCanLateAndEarlyInMonthSettingDto> Get()
        {
            return new TimesCanLateAndEarlyInMonthSettingDto
            {
                TimesCanLateAndEarlyInMonth = await SettingManager.GetSettingValueAsync(AppSettingNames.TimesCanLateAndEarlyInMonth),
                TimesCanLateAndEarlyInWeek = await SettingManager.GetSettingValueAsync(AppSettingNames.TimesCanLateAndEarlyInWeek),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_Update)]
        public async Task<TimesCanLateAndEarlyInMonthSettingDto> Change(TimesCanLateAndEarlyInMonthSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TimesCanLateAndEarlyInMonth, input.TimesCanLateAndEarlyInMonth);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TimesCanLateAndEarlyInWeek, input.TimesCanLateAndEarlyInWeek);
            return input;
        }
    }
}
