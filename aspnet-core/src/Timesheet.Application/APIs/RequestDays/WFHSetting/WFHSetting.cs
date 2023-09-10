using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Timesheet.APIs.MyAbsenceDays.WFHSetting.Dto;

namespace Timesheet.APIs.MyAbsenceDays.WFHSetting
{
    public class WFHSetting: AppServiceBase
    {
        public WFHSetting(IWorkScope workScope) : base(workScope)
        {

        }
    
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_RemoteSetting_View)]
        public async Task<WFHSettingDto> Get()
        {
            return new WFHSettingDto
            {
                numOfRemoteDays = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.WFHSetting),
                allowInternToWorkRemote = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AllowInternToWorkRemote),
                totalTimeTardinessAndEarlyLeave = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.TotalTimeAbsenceTime)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_RemoteSetting_Edit)]
        public async Task<WFHSettingDto> Change(WFHSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.WFHSetting, input.numOfRemoteDays);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AllowInternToWorkRemote, input.allowInternToWorkRemote);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TotalTimeAbsenceTime, input.totalTimeTardinessAndEarlyLeave);
            return input;
        }

    }
}
