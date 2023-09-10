using Abp.Configuration;
using Ncc.IoC;
using Xunit;
using Timesheet.Timesheets.EmailSettings;
using Timesheet.Timesheets.EmailSettings.Dto;

namespace Timesheet.Application.Tests.API.TimeSheets.EmailSettingAppServices
{
     /* <Summary>
     2/2 functions
     2 passed testcase
     Updated date: 16/11/2023
     </Summary>*/

    public class EmailSettingAppServiceTest : TimesheetApplicationTestBase
    {
        private readonly EmailSettingAppService _emailSettingAppService;

        public EmailSettingAppServiceTest()
        {
            var workScope = Resolve<IWorkScope>();
            _emailSettingAppService = new EmailSettingAppService(workScope);
            _emailSettingAppService.SettingManager = Resolve<ISettingManager>();
        }

        [Fact]
        public async void Get()
        {
            var expectedFromDisplayName = "mydomain.com mailer";
            var expectedEnalbleSsl = "false";
            var expectedUseDefaultCredentials = "true";
            var expectedDefaultFromAddress = "admin@mydomain.com";
            var expectedHost = "127.0.0.1";
            var expectedPort = "25";
            var expectedPassword = "";
            var expectedUseName = "";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _emailSettingAppService.Get();

                Assert.Equal(expectedDefaultFromAddress, result.DefaultFromAddress);
                Assert.Equal(expectedHost, result.Host);
                Assert.Equal(expectedPort, result.Port);
                Assert.Equal(expectedPassword, result.Password);
                Assert.Equal(expectedEnalbleSsl, result.EnableSsl);
                Assert.Equal(expectedUseName, result.UserName);
                Assert.Equal(expectedUseDefaultCredentials, result.UseDefaultCredentials);
                Assert.Equal(expectedFromDisplayName, result.FromDisplayName);
            });
        }
        [Fact]
        public async void Change()
        {
            var mailSettingDto = new MailSettingDto
            {
                FromDisplayName = "DisplayName",
                Host = "127.0.0.1",
                Password = "pass",
                Port = "26",
                UserName= "unitest@ncc.aisa",
                DefaultFromAddress = "domain",
                UseDefaultCredentials = "false",
                EnableSsl = "true"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _emailSettingAppService.Change(mailSettingDto);

                Assert.Equal(mailSettingDto.FromDisplayName, result.FromDisplayName);
                Assert.Equal(mailSettingDto.Host, result.Host);
                Assert.Equal(mailSettingDto.Password, result.Password);
                Assert.Equal(mailSettingDto.Port, result.Port);
                Assert.Equal(mailSettingDto.UserName, result.UserName);
                Assert.Equal(mailSettingDto.DefaultFromAddress, result.DefaultFromAddress);
                Assert.Equal(mailSettingDto.UseDefaultCredentials, result.UseDefaultCredentials);
                Assert.Equal(mailSettingDto.EnableSsl, result.EnableSsl);
            });
        }
    }
}
