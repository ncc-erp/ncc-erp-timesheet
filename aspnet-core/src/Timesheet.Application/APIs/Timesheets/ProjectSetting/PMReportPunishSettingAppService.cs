using Abp.Application.Services;
using Abp.Authorization;
using System.Threading.Tasks;
using Ncc.Configuration;
using Timesheet.APIs.Timesheets.ProjectSetting.Dto;

namespace Timesheet.APIs.ProjectSetting
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration)]
    public class PMReportPunishSettingAppService : ApplicationService
    {
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MezonSetting_View)]
        public async Task<PMReportPunishSettingDto> GetAsync()
        {
            return new PMReportPunishSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PMReportPunishEnable)),
                hour = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PMReportPunishAtHour)),
                dayofweek = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PMReportPunishAtDayOfWeek)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MezonSetting_Edit)]
        public async Task<PMReportPunishSettingDto> ChangeAsync(PMReportPunishSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PMReportPunishEnable, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PMReportPunishAtHour, input.hour.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PMReportPunishAtDayOfWeek, input.dayofweek);
            return input;
        }
    }
}
