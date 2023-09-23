using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.UI;
using Ncc.IoC;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.Capabilities;
using Timesheet.APIs.Capabilities.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.Capabilities
{
    /// <summary>
    /// 5/5 functions
    /// 14/14 test cases passed
    /// update day 10/01/2023
    /// </summary>

    public class CapabilityAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly CapabilityAppService _capabilityAppService;
        private readonly IWorkScope _wordkScope;

        public CapabilityAppService_Tests()
        {
            _wordkScope = Resolve<IWorkScope>();
            _capabilityAppService = Resolve<CapabilityAppService>(_wordkScope);
        }

        [Fact]
        public async Task GetAllPaging_Test1()
        {
            var expectTotalCount = 12;
            var expectItemCount = 12;

            //Total < max
            var input = new GridParam
            {
                MaxResultCount = 15,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.GetAllPaging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.First().Id.ShouldBe(1);
                result.Items.First().Name.ShouldBe("Database");
                result.Items.First().Type.ShouldBe(CapabilityType.Point);
                result.Items.First().Note.ShouldBeNullOrEmpty();

                var list = (List<GetCapabilitySettingByCapabilityIdDto>)result.Items.First().ApplySetting;
                list.Count.ShouldBe(4);
                list.Last().Coefficient.ShouldBe(1);
                list.Last().PositionName.ShouldBe("BA");
                list.Last().Usertype.ShouldBe(Usertype.Internship);
                list.Last().UsertypeName.ShouldBe("Intern");
            });
        }

        [Fact]
        public async Task GetAllPaging_Test2()
        {
            var expectTotalCount = 12;
            var expectItemCount = 10;

            //Total < max, Skip < total
            var input = new GridParam
            {
                MaxResultCount = 15,
                SkipCount = 2,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.GetAllPaging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().Name.ShouldBe("HR đánh giá chung");
                result.Items.Last().Type.ShouldBe(CapabilityType.Note);
                result.Items.Last().Note.ShouldBeNullOrEmpty();

                var list = (List<GetCapabilitySettingByCapabilityIdDto>)result.Items.Last().ApplySetting;
                list.Count.ShouldBe(4);
                list.Last().Coefficient.ShouldBe(1);
                list.Last().PositionName.ShouldBe("BA");
                list.Last().Usertype.ShouldBe(Usertype.Internship);
                list.Last().UsertypeName.ShouldBe("Intern");
            });
        }

        [Fact]
        public async Task GetAllPaging_Test3()
        {
            var expectTotalCount = 12;
            var expectItemCount = 0;

            //Total < max, Skip > total
            var input = new GridParam
            {
                MaxResultCount = 15,
                SkipCount = 20,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.GetAllPaging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test4()
        {
            var expectTotalCount = 12;
            var expectItemCount = 5;

            //Total > max
            var input = new GridParam
            {
                MaxResultCount = 5,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.GetAllPaging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.First().Id.ShouldBe(1);
                result.Items.First().Name.ShouldBe("Database");
                result.Items.First().Type.ShouldBe(CapabilityType.Point);
                result.Items.First().Note.ShouldBeNullOrEmpty();

                var list = (List<GetCapabilitySettingByCapabilityIdDto>)result.Items.First().ApplySetting;
                list.Count.ShouldBe(4);
                list.Last().Coefficient.ShouldBe(1);
                list.Last().PositionName.ShouldBe("BA");
                list.Last().Usertype.ShouldBe(Usertype.Internship);
                list.Last().UsertypeName.ShouldBe("Intern");
            });
        }

        [Fact]
        public async Task GetAllPaging_Test5()
        {
            var expectTotalCount = 12;
            var expectItemCount = 2;

            //Total > max, Skip < total
            var input = new GridParam
            {
                MaxResultCount = 5,
                SkipCount = 10,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.GetAllPaging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().Name.ShouldBe("HR đánh giá chung");
                result.Items.Last().Type.ShouldBe(CapabilityType.Note);
                result.Items.Last().Note.ShouldBeNullOrEmpty();

                var list = (List<GetCapabilitySettingByCapabilityIdDto>)result.Items.Last().ApplySetting;
                list.Count.ShouldBe(4);
                list.Last().Coefficient.ShouldBe(1);
                list.Last().PositionName.ShouldBe("BA");
                list.Last().Usertype.ShouldBe(Usertype.Internship);
                list.Last().UsertypeName.ShouldBe("Intern");
            });
        }

        [Fact]
        public async Task GetAllPaging_Test6()
        {
            var expectTotalCount = 12;
            var expectItemCount = 0;

            //Total < max, Skip > total
            var input = new GridParam
            {
                MaxResultCount = 5,
                SkipCount = 20,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.GetAllPaging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAll_Test()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.GetAll();
                result.Count.ShouldBeGreaterThanOrEqualTo(1);

                result.First().Id.ShouldBe(1);
                result.First().Name.ShouldBe("Database");
                result.First().Type.ShouldBe(CapabilityType.Point);
                result.First().Note.ShouldBeNullOrEmpty();

                var list = (List<GetCapabilitySettingByCapabilityIdDto>)result.First().ApplySetting;
                list.Count.ShouldBe(4);
                list.Last().Coefficient.ShouldBe(1);
                list.Last().PositionName.ShouldBe("BA");
                list.Last().Usertype.ShouldBe(Usertype.Internship);
                list.Last().UsertypeName.ShouldBe("Intern");
            });
        }

        [Fact]
        public async Task Create_Should_Create_A_Valid_Capability()
        {
            var expectTotalCount = 13;
            var expectCapability = new GetCapabilityDto
            {
                Name = "Test",
                Type = CapabilityType.Point,
                Note = "Test",
                ApplySetting = new List<GetCapabilitySettingByCapabilityIdDto>() { new GetCapabilitySettingByCapabilityIdDto {
                        Usertype = Usertype.Internship,
                        PositionName = "BA",
                        Coefficient = 1
                } }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.Create(expectCapability);
                result.ShouldNotBeNull();
                expectCapability.Id = result.Id;
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var capabilities = _wordkScope.GetAll<Capability>();
                capabilities.Count().ShouldBe(expectTotalCount);

                var capability = await _wordkScope.GetAsync<Capability>(expectCapability.Id);
                capability.ShouldNotBeNull();
                capability.Name.ShouldBe(expectCapability.Name);
                capability.Type.ShouldBe(expectCapability.Type);
                capability.Note.ShouldBe(expectCapability.Note);
            });
        }

        [Fact]
        public async Task Create_Should_Not_Create_Capability_Name_Exist()
        {
            var expectCapability = new GetCapabilityDto
            {
                Name = "HR đánh giá chung",
                Type = CapabilityType.Point,
                Note = "Test",
                ApplySetting = new List<GetCapabilitySettingByCapabilityIdDto>() { new GetCapabilitySettingByCapabilityIdDto {
                        Usertype = Usertype.Internship,
                        PositionName = "BA",
                        Coefficient = 1
                } }
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _capabilityAppService.Create(expectCapability);
                });
                Assert.Equal("This Capability already exist", exception.Message);
            });
        }

        [Fact]
        public async Task Update_Should_Update_A_Valid_Capability()
        {
            var expectTotalCount = 12;
            var expectId = 1;
            var expectCapability = new GetCapabilityDto
            {
                Id = 1,
                Name = "Test",
                Type = CapabilityType.Note,
                Note = "Test",
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilityAppService.Update(expectCapability);
                result.Id.ShouldBe(expectCapability.Id);
                result.Name.ShouldBe(expectCapability.Name);
                result.Type.ShouldBe(expectCapability.Type);
                result.Note.ShouldBe(expectCapability.Note);
                result.ApplySetting.ShouldBe(expectCapability.ApplySetting);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var capabilities = _wordkScope.GetAll<Capability>();
                var capability = await _wordkScope.GetAsync<Capability>(expectId);

                capabilities.Count().ShouldBe(expectTotalCount);
                capability.ShouldNotBeNull();
                capability.Name.ShouldBe(expectCapability.Name);
                capability.Type.ShouldBe(expectCapability.Type);
                capability.Note.ShouldBe(expectCapability.Note);
            });
        }

        [Fact]
        public async Task Update_Should_Not_Update_Capability_Name_Exist()
        {
            var expectCapability = new GetCapabilityDto
            {
                Id = 1,
                Name = "HR đánh giá chung",
                Type = CapabilityType.Point,
                Note = "Test",
                ApplySetting = "Test"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _capabilityAppService.Update(expectCapability);
                });
                Assert.Equal("This Capability already exist", exception.Message);
            });
        }

        [Fact]
        public async Task Delete_Should_Delete_A_Valid_Capability()
        {
            var expectTotalCount = 11;
            var capabilityToDelete = new EntityDto<long>
            {
                Id = 11,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _capabilityAppService.Delete(capabilityToDelete);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var capabilities = _wordkScope.GetAll<Capability>();
                capabilities.Count().ShouldBe(expectTotalCount);

                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _wordkScope.GetAsync<Capability>(capabilityToDelete.Id);
                });
            });
        }

        [Fact]
        public async Task Delete_Should_Not_Delete_Capability_Is_Exist_SomeWhere()
        {
            var capabilityToDelete = new EntityDto<long>
            {
                Id = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _capabilityAppService.Delete(capabilityToDelete);
                });
                Assert.Equal($"This Capability Id = {capabilityToDelete.Id} is already in somewhere", exception.Message);
            });
        }

        [Fact]
        public async Task Delete_Should_Not_Delete_Capability_Not_Exist()
        {
            var capabilityToDelete = new EntityDto<long>
            {
                Id = 9999,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _capabilityAppService.Delete(capabilityToDelete);
                });
            });
        }
    }
}
