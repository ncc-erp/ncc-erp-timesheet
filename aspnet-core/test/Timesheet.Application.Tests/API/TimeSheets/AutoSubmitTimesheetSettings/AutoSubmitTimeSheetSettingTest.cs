using Abp.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.Timesheets.AutoSubmitTimesheetSetting;
using Timesheet.APIs.Timesheets.AutoSubmitTimesheetSetting.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.TimeSheets.AutoSubmitTimesheetSettings
{
    /*<summary>
    2/2 functions
    2 passed test case
    Updated date: 16/01/2023
    </summary>*/

    public class AutoSubmitTimeSheetSettingTest : TimesheetApplicationTestBase
    {
        private readonly AutoSubmitTimesheetSetting _autoSubmitTimesheetSetting;

        public AutoSubmitTimeSheetSettingTest()
        {
            var workScope = Resolve<IWorkScope>();
            _autoSubmitTimesheetSetting = new AutoSubmitTimesheetSetting(workScope);
            _autoSubmitTimesheetSetting.SettingManager = Resolve<ISettingManager>();
        }

        [Fact]
        public async void Get()
        {
            var expectedAutoSubmitAt = "Monday";
            var expectedAutoSubmitTS = "true";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _autoSubmitTimesheetSetting.Get();

                Assert.Equal(expectedAutoSubmitAt, result.AutoSubmitAt);
                Assert.Equal(expectedAutoSubmitTS, result.AutoSubmitTimesheet);
            });
        }

        [Fact]
        public async void Change()
        {
            var autoSubmitTSDto = new AutoSubmitTimesheetDto 
            {
                AutoSubmitTimesheet = "true",
                AutoSubmitAt = "Firday"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _autoSubmitTimesheetSetting.Change(autoSubmitTSDto);

                Assert.Equal(autoSubmitTSDto.AutoSubmitTimesheet, result.AutoSubmitTimesheet);
                Assert.Equal(autoSubmitTSDto.AutoSubmitAt, result.AutoSubmitAt);
            });
        }
    }
}
