using Abp.Configuration;
using Microsoft.Extensions.Configuration;
using Ncc.IoC;
using System;
using Timesheet.Timesheets.SingleSignOnSetting;
using Timesheet.Timesheets.SingleSignOnSettings.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.TimeSheets.SingleSignOnSettingAppServices
{
    /* <Summary>
    2/2 functions
    2 passed testcase
    Updated date: 16/11/2023
    </Summary>*/

    public class SingleSignOnSettingAppServiceTest : TimesheetApplicationTestBase
    {
        private readonly SingleSignOnConfigurationAppService _singleSignOnSettingAppService;

        public SingleSignOnSettingAppServiceTest()
        {
            var workScope = Resolve<IWorkScope>();
            _singleSignOnSettingAppService = new SingleSignOnConfigurationAppService(workScope);
            _singleSignOnSettingAppService.SettingManager = Resolve<ISettingManager>();
        }

        [Fact]
        public async void Get()
        {
            var expectedClientAppId = "null";
            var expectedRegisterSecretCode = Configuration["DefaultSettings:SecretRegisterCode"]; ;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _singleSignOnSettingAppService.Get();
                Assert.Equal(expectedClientAppId, result.ClientAppId);
                Assert.Equal(expectedRegisterSecretCode, result.RegisterSecretCode);
            });
        }

        [Fact]
        public async void Change()
        {
            var singleSignOnSettingDto = new SingleSignOnSettingDto
            {
                ClientAppId = "1",
                RegisterSecretCode = "SecretCode",
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _singleSignOnSettingAppService.Change(singleSignOnSettingDto);

                Assert.Equal(singleSignOnSettingDto.ClientAppId, result.ClientAppId);
                Assert.Equal(singleSignOnSettingDto.RegisterSecretCode, result.RegisterSecretCode);
            });
        }
    }
}
