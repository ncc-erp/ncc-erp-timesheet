using Abp.Configuration;
using Abp.Domain.Uow;
using Ncc.IoC;
using Shouldly;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.Projects.SpecialProjectTaskSetting;
using Timesheet.APIs.Timesheets.Projects.SpecialProjectTaskSetting.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.Timesheets.Projects
{
    /// <summary>
    /// 2/2 functions
    /// 2/2 test cases passed
    /// update day 16/01/2023
    /// </summary>

    public class SpecialProjectTaskSetting_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;
        private readonly SpecialProjectTaskSetting _specialProjecTaskSetting;

        public SpecialProjectTaskSetting_Tests()
        {
            _workScope = Resolve<IWorkScope>();

            _specialProjecTaskSetting = new SpecialProjectTaskSetting(_workScope)
            {
                ObjectMapper = Resolve<Abp.ObjectMapping.IObjectMapper>(),
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>(),
                SettingManager = Resolve<ISettingManager>(),
            };
        }

        [Fact]
        public async Task Should_Get()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _specialProjecTaskSetting.Get();

                result.ProjectTaskId.ShouldBe("20078");
            });
        }

        [Fact]
        public async Task Should_Change()
        {
            long expectTotalCount = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                expectTotalCount = _workScope.GetAll<Setting>().Count();
                var result = await _specialProjecTaskSetting.Change(new SpecialProjectTaskSettingDto { ProjectTaskId = "30077" });
                result.ProjectTaskId.ShouldBe("30077");
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<Setting>().ToList();
                Assert.Equal(expectTotalCount + 1, result.Count());
                result.Last().Name.ShouldBe("App.OpenTalkTaskId");
                result.Last().Value.ShouldBe("30077");
            });
        }
    }
}
