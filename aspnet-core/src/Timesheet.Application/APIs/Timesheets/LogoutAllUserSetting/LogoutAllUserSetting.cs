using Abp.Application.Services;
using Abp.Authorization;
using Abp.UI;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.LogoutAllUserSetting.Dto;

namespace Timesheet.APIs.Timesheets.LogoutAllUserSetting
{
    public class LogoutAllUserSetting : ApplicationService
    {
        protected IWorkScope WorkScope { get; set; }
        public LogoutAllUserSetting(IWorkScope workScope)
        {
            WorkScope = workScope;
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_LogoutAllUser_View)]
        public async Task<LogoutAllUserDto> Get()
        {
            return new LogoutAllUserDto
            {
                loginBefore = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.LogoutAllUser)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_LogoutAllUser_Edit)]
        public async Task<LogoutAllUserDto> Change(LogoutAllUserDto input)
        {
            try
            {
                DateTime.Parse(input.loginBefore);
            }catch (Exception ex)
            {
                throw new UserFriendlyException("Lỗi định dạng ngày!");
            }
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.LogoutAllUser, input.loginBefore);
            return input;
        }
    }
}
