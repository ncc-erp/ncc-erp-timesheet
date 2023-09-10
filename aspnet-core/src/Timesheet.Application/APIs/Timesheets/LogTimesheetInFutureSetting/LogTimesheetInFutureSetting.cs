using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.LogTimesheetInFutureSetting.Dto;

namespace Timesheet.APIs.Timesheets.LogTimesheetInFutureSetting
{
    [AbpAuthorize]
    public class LogTimesheetInFutureSetting : AppServiceBase
    {
        public LogTimesheetInFutureSetting(IWorkScope workScope) : base(workScope)
        {

        }
        public async Task<LogTimesheetDto> Get()
        {
            return new LogTimesheetDto
            {
                CanLogTimesheetInFuture = await SettingManager.GetSettingValueAsync(AppSettingNames.LogTimesheetInFuture),
                DayAllowLogTimesheetInFuture = await SettingManager.GetSettingValueAsync(AppSettingNames.DayAllowLogTimesheetInFuture),
                DateToLockTimesheetOfLastMonth = await SettingManager.GetSettingValueAsync(AppSettingNames.DateToLockTimesheetOfLastMonth),
                MaxTimeOfDayLogTimesheet = await SettingManager.GetSettingValueAsync(AppSettingNames.MaxTimeSheetHourPerDay),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_LogTimesheetInFuture_Edit)]
        public async Task<LogTimesheetDto> Change(LogTimesheetDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LogTimesheetInFuture, input.CanLogTimesheetInFuture);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.DayAllowLogTimesheetInFuture, input.DayAllowLogTimesheetInFuture);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.DateToLockTimesheetOfLastMonth, input.DateToLockTimesheetOfLastMonth);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MaxTimeSheetHourPerDay, input.MaxTimeOfDayLogTimesheet);
            return input;
        }
    }
}
