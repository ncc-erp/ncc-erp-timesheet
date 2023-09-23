using Abp.Authorization;
using Abp.Configuration;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System.Threading.Tasks;
using Timesheet.Timesheets.AutoLockTimesheetSetting.Dto;

namespace Timesheet.Timesheets.AutoLockTimesheetSetting
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration)]
    public class AutoLockTimesheetSetting : AppServiceBase
    {
        public AutoLockTimesheetSetting(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_AutoLockTimesheet_View)]
        public async Task<AutoLockTimesheetSettingDto> Get()
        {
            return new AutoLockTimesheetSettingDto
            {
                LockDayOfUser = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockDayOfUser),
                LockHourOfUser = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockHourOfUser),
                LockMinuteOfUser = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockMinuteOfUser),
                LockDayOfPM = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockDayOfPM),
                LockHourOfPM = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockHourOfPM),
                LockMinuteOfPM = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockMinuteOfPM),
                LockDayAfterUnlock = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockDayAfterUnlock),
                LockHourAfterUnlock = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockHourAfterUnlock),
                LockMinuteAfterUnlock = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LockMinuteAfterUnlock),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_AutoLockTimesheet_Edit)]
        public async Task<AutoLockTimesheetSettingDto> Change(AutoLockTimesheetSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockDayOfUser, input.LockDayOfUser);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockHourOfUser, input.LockHourOfUser);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockMinuteOfUser, input.LockMinuteOfUser);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockDayOfPM, input.LockDayOfPM);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockHourOfPM, input.LockHourOfPM);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockMinuteOfPM, input.LockMinuteOfPM);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockDayAfterUnlock, input.LockDayAfterUnlock);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockHourAfterUnlock, input.LockHourAfterUnlock);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LockMinuteAfterUnlock, input.LockMinuteAfterUnlock);
            return input;
        }
    }
}
