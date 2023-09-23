using Abp.Configuration;
using Ncc.IoC;
using Timesheet.APIs.Timesheets.GetDataFromFaceIDSetting;
using Xunit;
using Timesheet.APIs.Timesheets.GetDataFromFaceIDSetting.Dto;
using Microsoft.Extensions.Configuration;
using System;

namespace Timesheet.Application.Tests.API.TimeSheets.GetDataFromFaceIDSettings
{
    /* <Summary>
    2/2 functions
    2 passed testcase
    Updated date: 16/11/2023
    </Summary>*/

    public class GetDataFromFaceIDSettingTest : TimesheetApplicationTestBase
    {
        private readonly GetDataFromFaceIDSetting _getDataFromFaceIDSetting;

        public GetDataFromFaceIDSettingTest()
        {
            var workScope = Resolve<IWorkScope>();
            _getDataFromFaceIDSetting = new GetDataFromFaceIDSetting(workScope);
            _getDataFromFaceIDSetting.SettingManager = Resolve<ISettingManager>();

        }

        [Fact]
        public async void Get()
        {
            var expectedAccountId = Configuration["DefaultSettings:CheckInInternalAccount"];
            var expectedGetDataAt = "11";
            var expectedSecretCode = Configuration["DefaultSettings:CheckInInternalXSecretKey"];
            var expectedUri = Configuration["DefaultSettings:CheckInInternalUrl"];
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _getDataFromFaceIDSetting.Get();
                Assert.Equal(expectedUri, result.Uri);
                Assert.Equal(expectedSecretCode, result.SecretCode);
                Assert.Equal(expectedGetDataAt, result.GetDataAt);
                Assert.Equal(expectedAccountId, result.AccountID);
            });
        }

        [Fact]
        public async void Change()
        {
            var getDataFromFaceIDDto = new GetDataFromFaceIDDto
            {
                GetDataAt = "12",
                AccountID = "123456789",
                SecretCode = "secretCode",
                Uri = "http"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _getDataFromFaceIDSetting.Change(getDataFromFaceIDDto);
                Assert.Equal(getDataFromFaceIDDto.GetDataAt, result.GetDataAt);
                Assert.Equal(getDataFromFaceIDDto.AccountID, result.AccountID);
                Assert.Equal(getDataFromFaceIDDto.SecretCode, result.SecretCode);
                Assert.Equal(getDataFromFaceIDDto.Uri, result.Uri);
            });
        }
    }
}
