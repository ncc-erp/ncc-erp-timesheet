using Abp;
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
using Timesheet.APIs.Timesheets.TimeStartChangingCheckinToCheckoutSetting.Dto;

namespace Timesheet.APIs.Timesheets.TimeStartChangingCheckinToCheckoutSetting
{
    public class TimeStartChangingCheckinToCheckoutSettingAppService: AppServiceBase
    {
        public TimeStartChangingCheckinToCheckoutSettingAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_View)]
        [HttpGet]
        public async Task<TimeStartChangingCheckinToCheckoutSettingDto> Get()
        {
            return new TimeStartChangingCheckinToCheckoutSettingDto
            {
                TimeStartCheckOut = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.TimeStartChangingCheckinToCheckout),
                TimeStartCheckOutCaseOffAfternoon = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.TimeStartChangingCheckinToCheckoutCaseOffAfternoon),
                EnableTimeStartChangingToCheckout = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.TimeStartChangingCheckinToCheckoutEnable)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_Update)]
        [HttpPut]
        public async Task<TimeStartChangingCheckinToCheckoutSettingDto> Change(TimeStartChangingCheckinToCheckoutSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TimeStartChangingCheckinToCheckout, input.TimeStartCheckOut);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TimeStartChangingCheckinToCheckoutCaseOffAfternoon, input.TimeStartCheckOutCaseOffAfternoon);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TimeStartChangingCheckinToCheckoutEnable, input.EnableTimeStartChangingToCheckout);
            return input;
        }
    }
}
