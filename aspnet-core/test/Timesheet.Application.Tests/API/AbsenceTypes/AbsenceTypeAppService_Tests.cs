using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.ObjectMapping;
using Abp.UI;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.AbsenceTypes;
using Timesheet.APIs.DayOffs.Dto;
using Timesheet.Entities;
using Xunit;

namespace Timesheet.Application.Tests.API.AbsenceTypes
{
    public class AbsenceTypeAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly AbsenceTypeAppService _absenceType;
        private readonly IWorkScope _work;

        public AbsenceTypeAppService_Tests()
        {
            _work = Resolve<IWorkScope>();
            _absenceType = new AbsenceTypeAppService(_work);
            _absenceType.ObjectMapper = Resolve<IObjectMapper>();
        }

        //Test Function GetAll
        [Fact]
        public async Task Should_Get_All()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _absenceType.GetAll();
                Assert.Equal(4, result.Count);
                result.ShouldContain(x => x.Id == 1);
                result.ShouldContain(x => x.Status == Ncc.Entities.Enum.StatusEnum.OffTypeStatus.CoPhep);
                result.ShouldContain(x => x.Name == "Nghỉ thông thường\t");
                result.ShouldContain(x => x.Name == "Nghỉ cưới bản thân (3 ngày phép)\t");
                result.ShouldContain(x => x.Length == 3);
            });
        }

        //Test Function Save
        [Fact]
        public async Task Should_Not_Allow_Save_With_Absence_Type_Already_Existed()
        {
            var input = new AbsenceTypeDto
            {
                Id = 100,
                Status = Ncc.Entities.Enum.StatusEnum.OffTypeStatus.CoPhep,
                Name = "Nghỉ thông thường\t",
                Length = 3
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var expectedMsg = string.Format("Absence type - {0} already existed", input.Name);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _absenceType.Save(input);
                });
                Assert.Equal(expectedMsg, exception.Message);
            });
        }

        [Fact]
        public async Task Should_Allow_Save_With_New_Absence_Type()
        {
            var input = new AbsenceTypeDto
            {
                Id = 0,
                Status = Ncc.Entities.Enum.StatusEnum.OffTypeStatus.CoPhep,
                Name = "New Absence Type",
                Length = 3
            };
            var result = new AbsenceTypeDto { };
            await WithUnitOfWorkAsync(async () =>
            {
                result = await _absenceType.Save(input);
                Assert.Equal(input.Name, result.Name);
                Assert.Equal(input.Status, result.Status);
                Assert.Equal(input.Length, result.Length);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var absenceTypes = _work.GetAll<DayOffType>();
                Assert.Equal(5, absenceTypes.Count());
                var absenceTepy = await _work.GetAsync<DayOffType>(result.Id);
                Assert.Equal(input.Name, absenceTepy.Name);
                Assert.Equal(input.Status, absenceTepy.Status);
                Assert.Equal(input.Length, absenceTepy.Length);
            });
        }

        [Fact]
        public async Task Should_Allow_Update_With_Absence_Type_Already_Existed()
        {
            var input = new AbsenceTypeDto
            {
                Id = 1,
                Status = Ncc.Entities.Enum.StatusEnum.OffTypeStatus.CoPhep,
                Name = "New Absence Type",
                Length = 3
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _absenceType.Save(input);
                Assert.Equal(input.Name, result.Name);
                Assert.Equal(input.Status, result.Status);
                Assert.Equal(input.Length, result.Length);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var absenceTypes = _work.GetAll<DayOffType>();
                Assert.Equal(4, absenceTypes.Count());
                absenceTypes.ShouldContain(x => x.Id == 1);
                var absenceTepy = await _work.GetAsync<DayOffType>(1);
                Assert.Equal(input.Name, absenceTepy.Name);
                Assert.Equal(input.Status, absenceTepy.Status);
                Assert.Equal(input.Length, absenceTepy.Length);
            });
        }

        // Test Function Delete
        [Fact]
        public async Task Should_Not_Allow_Delete_With_Absence_Type_Not_Exist()
        {
            var input = new EntityDto<long>(100);
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _absenceType.Delete(input);
                });
            });
        }

        [Fact]
        public async Task Should_Allow_Delete()
        {
            var input = new EntityDto<long>(1);
            await WithUnitOfWorkAsync(async () =>
            {
                await _absenceType.Delete(input);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _absenceType.GetAll();
                Assert.Equal(3, result.Count);
                result.ShouldNotContain(x => x.Id == 1);
                result.ShouldNotContain(x => x.Name == "Nghỉ thông thường\t");
                result.ShouldContain(x => x.Status == Ncc.Entities.Enum.StatusEnum.OffTypeStatus.CoPhep);
                result.ShouldContain(x => x.Name == "Nghỉ cưới bản thân (3 ngày phép)\t");
                result.ShouldContain(x => x.Length == 3);
            });
        }
    }
}
