using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.GetDataFromFaceIDSetting.Dto;

namespace Timesheet.APIs.Timesheets.GetDataFromFaceIDSetting
{
    [AbpAuthorize]
    public class GetDataFromFaceIDSetting : AppServiceBase
    {
        public GetDataFromFaceIDSetting(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CheckInSetting_View)]
        public async Task<GetDataFromFaceIDDto> Get()
        {
            return new GetDataFromFaceIDDto
            {
                GetDataAt = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInInternalAtHour),
                AccountID = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInInternalAccount),
                SecretCode = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInInternalXSecretKey),
                Uri = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInInternalUrl)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CheckInSetting_Update)]
        public async Task<GetDataFromFaceIDDto> Change(GetDataFromFaceIDDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInInternalAtHour, input.GetDataAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInInternalAccount, input.AccountID);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInInternalXSecretKey, input.SecretCode);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInInternalUrl, input.Uri);
            return input;
        }
    }
}
