using Xunit;
using Timesheet.APIs.Timesheets.LogTimesheetInFutureSetting;
using Ncc.IoC;
using Abp.Configuration;
using Timesheet.APIs.Timesheets.LogTimesheetInFutureSetting.Dto;

namespace Timesheet.Application.Tests.API.TimeSheets.LogTimesheetInFutureSettings
{
    public class LogTimesheetInFutureSettingTest : TimesheetApplicationTestBase
    {
        /* <Summary>
        2/2 functions
        2 passed testcase
        Updated date: 16/11/2023
        </Summary>*/

        private readonly LogTimesheetInFutureSetting _logTimesheetInFutureSetting;

        public LogTimesheetInFutureSettingTest()
        {
            var workScope = Resolve<IWorkScope>();
            _logTimesheetInFutureSetting = new LogTimesheetInFutureSetting(workScope);
            _logTimesheetInFutureSetting.SettingManager = Resolve<ISettingManager>();
        }


        [Fact]
        public async void Get()
        {
            var expectedCanLogTimesheetInFuture = "true";
            var expectedDateToLockTimesheetOfLastMonth = "5";
            var expectedDayAllowLogTimesheetInFuture = "100";
            var expectedMaxTimeOfDayLogTimesheet = "16";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _logTimesheetInFutureSetting.Get();

                Assert.Equal(expectedCanLogTimesheetInFuture, result.CanLogTimesheetInFuture);
                Assert.Equal(expectedDayAllowLogTimesheetInFuture, result.DayAllowLogTimesheetInFuture);
                Assert.Equal(expectedDateToLockTimesheetOfLastMonth, result.DateToLockTimesheetOfLastMonth);
                Assert.Equal(expectedMaxTimeOfDayLogTimesheet, result.MaxTimeOfDayLogTimesheet);
            });
        }
        [Fact]
        public async void Change()
        {
            var logTimesheetDto = new LogTimesheetDto
            {
                DayAllowLogTimesheetInFuture = "13",
                DateToLockTimesheetOfLastMonth = "4",
                MaxTimeOfDayLogTimesheet = "15",
                CanLogTimesheetInFuture = "false"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _logTimesheetInFutureSetting.Change(logTimesheetDto);
                Assert.Equal(logTimesheetDto.DayAllowLogTimesheetInFuture, result.DayAllowLogTimesheetInFuture);
                Assert.Equal(logTimesheetDto.DateToLockTimesheetOfLastMonth, result.DateToLockTimesheetOfLastMonth);
                Assert.Equal(logTimesheetDto.MaxTimeOfDayLogTimesheet, result.MaxTimeOfDayLogTimesheet);
                Assert.Equal(logTimesheetDto.CanLogTimesheetInFuture, result.CanLogTimesheetInFuture);
            });
        }
    }
}
