using Abp.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Timesheets.AutoLockTimesheetSetting;
using Timesheet.Timesheets.AutoLockTimesheetSetting.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.TimeSheets.AutoLockTimesheetSettings
{
    /*<summary>
    2/2 functions
    2 passed test case
    Updated date: 16/01/2023
    </summary>*/
    public class AutoLockTimesheetSettingTest : TimesheetApplicationTestBase
    {
        private readonly AutoLockTimesheetSetting _autoLockTimesheetSetting;

        public AutoLockTimesheetSettingTest()
        {
            var workScope = Resolve<IWorkScope>();
            _autoLockTimesheetSetting = new AutoLockTimesheetSetting(workScope);
            _autoLockTimesheetSetting.SettingManager = Resolve<ISettingManager>();
        }

        [Fact]
        public async void Get()
        {
            var expectedLockDayAfterUnlock = "Thursday";
            var expectedLockDayOfPM = "Tuesday";
            var expectedLockDayOfUser = "Monday";
            var expectedLockHourAfterUnlock = "3";
            var expectedLockHourOfPM = "0";
            var expectedLockMinuteAfterUnlock = "0";
            var expectedLockMinuteOfPM = "0";
            var expectedLockMinuteOfUser = "0";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _autoLockTimesheetSetting.Get();

                Assert.Equal(expectedLockDayAfterUnlock, result.LockDayAfterUnlock);
                Assert.Equal(expectedLockDayOfPM, result.LockDayOfPM);
                Assert.Equal(expectedLockDayOfUser, result.LockDayOfUser);
                Assert.Equal(expectedLockHourAfterUnlock, result.LockHourAfterUnlock);
                Assert.Equal(expectedLockHourOfPM, result.LockHourOfPM);
                Assert.Equal(expectedLockMinuteOfPM, result.LockMinuteOfPM);
                Assert.Equal(expectedLockMinuteOfUser, result.LockMinuteOfUser);
                Assert.Equal(expectedLockMinuteAfterUnlock, result.LockMinuteAfterUnlock);
            });
        }

        [Fact]
        public async void Change()
        {
            var autoLockTimesheetDto = new AutoLockTimesheetSettingDto
            {
                LockDayOfUser = "Monday",
                LockHourOfUser = "12",
                LockMinuteOfUser = "20",
                LockDayOfPM = "Friday",
                LockHourOfPM = "1",
                LockMinuteOfPM = "20",
                LockDayAfterUnlock = "Wednesday",
                LockHourAfterUnlock = "2",
                LockMinuteAfterUnlock = "45",
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _autoLockTimesheetSetting.Change(autoLockTimesheetDto);

                Assert.Equal(autoLockTimesheetDto.LockDayAfterUnlock, result.LockDayAfterUnlock);
                Assert.Equal(autoLockTimesheetDto.LockDayOfPM, result.LockDayOfPM);
                Assert.Equal(autoLockTimesheetDto.LockDayOfUser, result.LockDayOfUser);
                Assert.Equal(autoLockTimesheetDto.LockHourAfterUnlock, result.LockHourAfterUnlock);
                Assert.Equal(autoLockTimesheetDto.LockHourOfPM, result.LockHourOfPM);
                Assert.Equal(autoLockTimesheetDto.LockMinuteOfPM, result.LockMinuteOfPM);
                Assert.Equal(autoLockTimesheetDto.LockMinuteOfUser, result.LockMinuteOfUser);
                Assert.Equal(autoLockTimesheetDto.LockMinuteAfterUnlock, result.LockMinuteAfterUnlock);
            });
        }
    }
}
