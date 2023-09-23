using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.EmailSaoDoSetting.Dto;

namespace Timesheet.APIs.Timesheets.EmailSaoDoSetting
{
    public class EmailSaoDoSetting : AppServiceBase
    {
        public EmailSaoDoSetting(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_EmailSaoDo_View)]
        public async Task<EmaiSaoDoDto> Get()
        {
            return new EmaiSaoDoDto
            {
                CanSendEmailToSaoDo = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailToSaoDo),
                EmailSaoDo = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.EmailSaoDo)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_EmailSaoDo_Edit)]
        public async Task<EmaiSaoDoDto> Change(EmaiSaoDoDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendEmailToSaoDo, input.CanSendEmailToSaoDo);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.EmailSaoDo, input.EmailSaoDo);
            return input;
        }
    }
}
