using Abp.Application.Services;
using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.MezonSetting.Dto;

namespace Timesheet.APIs.Timesheets.MezonSetting
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration)]
    public class MezonSetting : ApplicationService
    {
        public MezonSetting(){}

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MezonSetting_View)]
        public async Task<MezonSettingDto> Get()
        {
            return new MezonSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AddDataToOpenTalkEnable)),
                hour = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtHour)),
                dayofweek = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtDayOfWeek),
                secretCode = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MezonSecurityCode),
                uri = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MezonBaseAddress)
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MezonSetting_Edit)]
        public async Task<MezonSettingDto> Change(MezonSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AddDataToOpenTalkEnable, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtHour, input.hour.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtDayOfWeek, input.dayofweek);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MezonSecurityCode, input.secretCode);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MezonBaseAddress, input.uri);
            return input;
        }
    }
}
