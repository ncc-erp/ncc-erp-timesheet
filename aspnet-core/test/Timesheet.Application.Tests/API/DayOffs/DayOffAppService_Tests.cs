using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.ObjectMapping;
using Abp.UI;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.DayOffs;
using Timesheet.APIs.DayOffs.Dto;
using Timesheet.Entities;
using Xunit;

namespace Timesheet.Application.Tests.API.DayOffs
{
    /// <summary>
    /// 3/3 Function
    /// 5/5 Test cases passed
    /// Update day 11/01/2023
    /// </summary>
    public class DayOffAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly DayOffAppService _dayOff;
        private readonly IWorkScope _work;
        public DayOffAppService_Tests()
        {
            _work = Resolve<IWorkScope>();
            _dayOff = new DayOffAppService(_work);
            _dayOff.ObjectMapper = Resolve<IObjectMapper>();
        }

        //Test Function GetAll
        [Fact]
        public async Task Should_Get_All()
        {
            var inputYear = 2022;
            var inputMonth = 12;
            var inputBranchId = 1;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _dayOff.GetAll(inputYear, inputMonth, inputBranchId);
                Assert.Equal(4, result.Count);
                result.ShouldContain(x => x.Id == 1);
                result.ShouldContain(x => x.Id == 2);
                result.ShouldContain(x => x.DayOff == new DateTime(2022,12,4));
                result.ShouldContain(x => x.DayOff == new DateTime(2022, 12, 11));
                result.ShouldContain(x => x.Coefficient==2);
                result.ShouldContain(x => x.Name==null);
                result.ShouldContain(x => x.Branch == null);
            });
        }

        //Test Function Save
        [Fact]
        public async Task Should_Not_Allow_Save_With_Day_Off_Already_Existed()
        {
            var input = new DayOffDto
            {
                Id = 100,
                DayOff = new DateTime(2022, 12, 4),
                Coefficient= 2,
                Name="Tet Nguyen Dan",
                Branch= Ncc.Entities.Enum.StatusEnum.Branch.HaNoi,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = string.Format("DayOff - {0} already existed", input.DayOff.ToString("MM/dd/yyyy"));
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _dayOff.Save(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Save_With_New_Day_Off_Valid()
        {
            var input = new DayOffDto
            {
                Id = 0,
                DayOff = new DateTime(2022, 12, 31),
                Coefficient = 2,
                Name = "Tet Nguyen Dan",
                Branch = Ncc.Entities.Enum.StatusEnum.Branch.HaNoi,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _dayOff.Save(input);
                Assert.Equal(input.DayOff,result.DayOff);
                Assert.Equal(input.Coefficient, result.Coefficient);
                Assert.Equal(input.Name, result.Name);
                Assert.Equal(input.Branch, result.Branch);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var dayOffs = _work.GetAll<DayOffSetting>();
                Assert.Equal(18, dayOffs.Count());
                dayOffs.ShouldContain(x => x.DayOff == input.DayOff);
                dayOffs.ShouldContain(x => x.Name == input.Name);
                dayOffs.ShouldContain(x => x.Branch == input.Branch);
                dayOffs.ShouldContain(x => x.Coefficient == input.Coefficient);
            });
        }

        [Fact]
        public async Task Should_Allow_Update_With_Day_Off_Already_Existed()
        {
            var input = new DayOffDto
            {
                Id = 1,
                DayOff = new DateTime(2022, 12, 31),
                Coefficient = 2,
                Name = "Tet Nguyen Dan",
                Branch = Ncc.Entities.Enum.StatusEnum.Branch.HaNoi,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _dayOff.Save(input);
                Assert.Equal(input.DayOff, result.DayOff);
                Assert.Equal(input.Coefficient, result.Coefficient);
                Assert.Equal(input.Name, result.Name);
                Assert.Equal(input.Branch, result.Branch);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var dayOffs = _work.GetAll<DayOffSetting>();
                Assert.Equal(17, dayOffs.Count());
                var dayOff = await _work.GetAsync<DayOffSetting>(1);
                Assert.Equal(input.DayOff, dayOff.DayOff);
                Assert.Equal(input.Coefficient, dayOff.Coefficient);
                Assert.Equal(input.Name, dayOff.Name);
                Assert.Equal(input.Branch, dayOff.Branch);
            });
        }

        //Test function Delete     
        [Fact]
        public async Task Should_Allow_Delete()
        {
            var input = new EntityDto<long>(1);
            await WithUnitOfWorkAsync(async () =>
            {
                await _dayOff.Delete(input);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var dayOffs = _work.GetAll<DayOffSetting>();
                Assert.Equal(16, dayOffs.Count());
                dayOffs.ShouldNotContain(x => x.Id == 1);
;
            });
        }
    }
}
