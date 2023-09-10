using Ncc.IoC;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.Timesheets.MyTimesheets;
using Xunit;

namespace Timesheet.Application.Tests.API.MyTimesheets
{
    /// <summary>
    /// 3/3 functions
    /// 3/3 test cases passed
    /// update day 12/01/2023
    /// </summary>

    public class TimeSheetProjectAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly TimeSheetProjectAppService _timeSheetProjectAppService;
        private readonly IWorkScope _workScope;

        public TimeSheetProjectAppService_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            _timeSheetProjectAppService = Resolve<TimeSheetProjectAppService>(_workScope);
        }

        [Fact]
        public async Task GetTimeSheetStatisticTasks_Test()
        {
            var expectTotalCount = 2;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timeSheetProjectAppService.GetTimeSheetStatisticTasks(1, DateTime.Parse("2022/12/1"), DateTime.Parse("2022/12/31"));
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().TaskId.ShouldBe(2);
                result.Last().TaskName.ShouldBe("Testing");
                result.Last().BillableWorkingTime.ShouldBe(0);
                result.Last().TotalWorkingTime.ShouldBe(0);
                result.Last().Billable.ShouldBe(true);
            });
        }

        [Fact]
        public async Task GetTimeSheetStatisticTeams_Test()
        {
            var expectTotalCount = 7;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timeSheetProjectAppService.GetTimeSheetStatisticTeams(1, DateTime.Parse("2022/12/1"), DateTime.Parse("2022/12/31"));
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().UserID.ShouldBe(19);
                result.Last().BillableWorkingTime.ShouldBe(0);
                result.Last().TotalWorkingTime.ShouldBe(0);
                result.Last().UserName.ShouldBe("Thử Nguyễn Văn");
                result.Last().projectUserType.ShouldBe(Ncc.Entities.ProjectUserType.Member);
            });
        }

        [Fact]
        public async Task ExportBillableTimesheets_Test()
        {
            var expectTotalCount = 9;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _timeSheetProjectAppService.ExportBillableTimesheets(1, DateTime.Parse("2022/12/1"), DateTime.Parse("2022/12/31"));
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().Id.ShouldBe(35);
                result.Last().UserName.ShouldBe("Hiếu Trần Trung");
                result.Last().TargetUserName.ShouldBe("Hiếu Trần Trung");
                result.Last().TaskName.ShouldBe("Coding");
                result.Last().TypeOfWork.ShouldBe(Ncc.Entities.Enum.StatusEnum.TypeOfWork.NormalWorkingHours);
                result.Last().TargetUserWorkingTime.ShouldBe(0);
                result.Last().WorkingTime.ShouldBe(240);
                result.Last().IsShadow.ShouldBe(false);
            });
        }
    }
}
