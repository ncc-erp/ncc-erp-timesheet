using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.SercurityCodeSetting.Dto;

namespace Timesheet.APIs.Timesheets.SercurityCodeSetting
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration)]
    public class SercurityCodeSetting : AppServiceBase
    {
        public SercurityCodeSetting(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SercurityCode_View)]
        public async Task<SercurityCodeDto> Get()
        {
            return new SercurityCodeDto
            {
                SercurityCode = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SecurityCode)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SercurityCode_Edit)]
        public async Task<SercurityCodeDto> Change(SercurityCodeDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SecurityCode, input.SercurityCode);
            return input;
        }
    }
}
