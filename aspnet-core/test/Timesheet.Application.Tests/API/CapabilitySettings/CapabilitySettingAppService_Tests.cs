using Abp.Domain.Entities;
using Abp.ObjectMapping;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.IoC;
using Shouldly;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.CapabilitySettings;
using Timesheet.APIs.CapabilitySettings.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.CapabilitySettings
{
    /// <summary>
    /// 10/10 functions
    /// 30/30 test cases passed
    /// update day 10/01/2023
    /// </summary>
    
    public class CapabilitySettingAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly CapabilitySettingAppService _capabilitySetting;
        private readonly IWorkScope _workScope;

        public CapabilitySettingAppService_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            _capabilitySetting = Resolve<CapabilitySettingAppService>(_workScope);
            _capabilitySetting.ObjectMapper = Resolve<IObjectMapper>();
        }

        [Fact]
        public async Task GetAllPaging_Test1()
        {
            var expectTotalCount = 4;
            var expectItemCount = 3;

            //Type = Point, total > max
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 3,
                },
                Type = CapabilityType.Point
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(3);
                result.Items.Last().PositionName.ShouldBe("IT");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(4);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(13);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(5);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("English");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Point);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test2()
        {
            var expectTotalCount = 4;
            var expectItemCount = 2;

            //Type = Point, total > max, skip < total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 3,
                    SkipCount = 2,
                },
                Type = CapabilityType.Point
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(7);
                result.Items.Last().PositionName.ShouldBe("BA");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(4);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(18);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(6);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("Kinh nghiệm\t");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Point);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(2);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test3()
        {
            var expectTotalCount = 4;
            var expectItemCount = 0;

            //Type = Point, total > max, skip > total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 3,
                    SkipCount = 10,
                },
                Type = CapabilityType.Point
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test4()
        {
            var expectTotalCount = 4;
            var expectItemCount = 4;

            //Type = Point, total < max
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 10,
                },
                Type = CapabilityType.Point
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(7);
                result.Items.Last().PositionName.ShouldBe("BA");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(4);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(18);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(6);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("Kinh nghiệm\t");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Point);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(2);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test5()
        {
            var expectTotalCount = 4;
            var expectItemCount = 2;

            //Type = Point, total < max, skip < total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 10,
                    SkipCount = 2,
                },
                Type = CapabilityType.Point
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(7);
                result.Items.Last().PositionName.ShouldBe("BA");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(4);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(18);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(6);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("Kinh nghiệm\t");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Point);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(2);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test6()
        {
            var expectTotalCount = 4;
            var expectItemCount = 0;

            //Type = Point, total < max, skip > total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 10,
                    SkipCount = 15,
                },
                Type = CapabilityType.Point
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test7()
        {
            var expectTotalCount = 4;
            var expectItemCount = 4;

            //Type = Note, total > max
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 10,
                },
                Type = CapabilityType.Note
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(7);
                result.Items.Last().PositionName.ShouldBe("BA");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(1);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(17);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(12);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("HR đánh giá chung");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Note);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test8()
        {
            var expectTotalCount = 4;
            var expectItemCount = 2;

            //Type = Note, total > max, skip < total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 5,
                    SkipCount = 2,
                },
                Type = CapabilityType.Note
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(7);
                result.Items.Last().PositionName.ShouldBe("BA");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(1);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(17);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(12);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("HR đánh giá chung");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Note);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test9()
        {
            var expectTotalCount = 4;
            var expectItemCount = 0;

            //Type = Note, total > max, skip > total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 5,
                    SkipCount = 25,
                },
                Type = CapabilityType.Note
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test10()
        {
            var expectTotalCount = 4;
            var expectItemCount = 4;

            //Type = Note, total < max
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 15,
                },
                Type = CapabilityType.Note
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(7);
                result.Items.Last().PositionName.ShouldBe("BA");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(1);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(17);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(12);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("HR đánh giá chung");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Note);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test11()
        {
            var expectTotalCount = 4;
            var expectItemCount = 1;

            //Type = Note, total < max, skip < total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 15,
                    SkipCount = 3,
                },
                Type = CapabilityType.Note
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.Last().UserType.ShouldBe(Usertype.Internship);
                result.Items.Last().UserTypeName.ShouldBe("Internship");
                result.Items.Last().PositionId.ShouldBe(7);
                result.Items.Last().PositionName.ShouldBe("BA");
                result.Items.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Count.ShouldBe(1);
                result.Items.Last().Capabilities.Last().Id.ShouldBe(17);
                result.Items.Last().Capabilities.Last().CapabilityId.ShouldBe(12);
                result.Items.Last().Capabilities.Last().CapabilityName.ShouldBe("HR đánh giá chung");
                result.Items.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Note);
                result.Items.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Items.Last().Capabilities.Last().Coefficient.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetAllPaging_Test12()
        {
            var expectTotalCount = 4;
            var expectItemCount = 0;

            //Type = Note, total < max, skip > total
            var paramCapability = new ParamCapability
            {
                Param = new GridParam
                {
                    MaxResultCount = 15,
                    SkipCount = 25,
                },
                Type = CapabilityType.Note
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllPaging(paramCapability);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        public async Task GetAllCapabilitySettings_Test1()
        {
            var expectTotalCount = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllCapabilitySettings(Usertype.Internship, 7);
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().UserType.ShouldBe(Usertype.Internship);
                result.Last().UserTypeName.ShouldBe("Internship");
                result.Last().PositionId.ShouldBe(7);
                result.Last().PositionName.ShouldBe("BA");
                result.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Last().Capabilities.Count.ShouldBe(5);
                result.Last().Capabilities.Last().CapabilityId.ShouldBe(12);
                result.Last().Capabilities.Last().CapabilityName.ShouldBe("HR đánh giá chung");
                result.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Note);
                result.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Last().Capabilities.Last().Coefficient.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetAllCapabilitySettings_Test2()
        {
            var expectTotalCount = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllCapabilitySettings(Usertype.Staff, 7);
                Assert.Equal(expectTotalCount, result.Count);
            });
        }

        [Fact]
        public async Task GetCapabilitiesByUserTypeAndPositionId_Test1()
        {
            var expectTotalCount = 1;
            var userType = Usertype.Internship;
            var positionId = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllCapabilitySettings(userType, positionId);
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().UserType.ShouldBe(Usertype.Internship);
                result.Last().UserTypeName.ShouldBe("Internship");
                result.Last().PositionId.ShouldBe(1);
                result.Last().PositionName.ShouldBe("Dev");
                result.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Last().Capabilities.Count.ShouldBe(4);
                result.Last().Capabilities.Last().CapabilityId.ShouldBe(12);
                result.Last().Capabilities.Last().CapabilityName.ShouldBe("HR đánh giá chung");
                result.Last().Capabilities.Last().Type.ShouldBe(CapabilityType.Note);
                result.Last().Capabilities.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Last().Capabilities.Last().Coefficient.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetCapabilitiesByUserTypeAndPositionId_Test2()
        {
            var expectTotalCount = 0;
            var userType = Usertype.Staff;
            var positionId = 2;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetAllCapabilitySettings(userType, positionId);
                Assert.Equal(expectTotalCount, result.Count);
            });
        }

        [Fact]
        public async Task GetRemainCapabilitiesByUserTypeAndPositionId_Test()
        {
            var expectTotalCount = 7;
            var userType = Usertype.Internship;
            var positionId = 7;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.GetRemainCapabilitiesByUserTypeAndPositionId(userType, positionId);
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().CapabilityId.ShouldBe(11);
                result.Last().CapabilityName.ShouldBe("ReactJs\t");
                result.Last().Coefficient.ShouldBe(1);
                result.Last().GuildeLine.ShouldBeNullOrEmpty();
                result.Last().UserType.ShouldBeNull();
                result.Last().UserTypeName.ShouldBeNullOrEmpty();
                result.Last().PositionId.ShouldBeNull();
                result.Last().PositionName.ShouldBeNullOrEmpty();
                result.Last().GuildeLine.ShouldBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task CapabilitySettingClone_Should_Clone_CapabilitySetting()
        {
            var expectTotalCount = 22;
            var capabilitySettingClone = new CapabilitySettingCloneDto
            {
                FromUserType = Usertype.Internship,
                ToUserType = Usertype.Internship,
                FromPositionId = 1,
                ToPositionId = 17,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.CapabilitySettingClone(capabilitySettingClone);

                result.FromUserType.ShouldBe(Usertype.Internship);
                result.FromUserTypeName.ShouldBe("Internship");
                result.FromPositionId.ShouldBe(1);
                result.ToUserType.ShouldBe(Usertype.Internship);
                result.ToUserTypeName.ShouldBe("Internship");
                result.ToPositionId.ShouldBe(17);
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<CapabilitySetting>();
                Assert.Equal(expectTotalCount, result.Count());

                result.Last().Id.ShouldBe(22);
                result.Last().UserType.ShouldBe(Usertype.Internship);
                result.Last().PositionId.ShouldBe(17);
                result.Last().CapabilityId.ShouldBe(12);
                result.Last().Coefficient.ShouldBe(1);
                result.Last().IsDeleted.ShouldBe(false);
            });
        }

        [Fact]
        public async Task CapabilitySettingClone_Should_Not_Clone_CapabilitySetting_Already_Exist()
        {
            var capabilitySettingClone = new CapabilitySettingCloneDto
            {
                FromUserType = Usertype.Internship,
                ToUserType = Usertype.Internship,
                FromPositionId = 1,
                ToPositionId = 3,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var positionName = await _workScope.GetAll<CapabilitySetting>().Where(s => s.PositionId == capabilitySettingClone.ToPositionId).Select(s => s.Position.Name).FirstOrDefaultAsync();
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _capabilitySetting.CapabilitySettingClone(capabilitySettingClone);
                });

                Assert.Equal($"Không thể Clone vì đã tồn tại Capabilities ở CapabilitySetting: {capabilitySettingClone.ToUserType} {positionName}", exception.Message);
            });
        }

        [Fact]
        public async Task CapabilitySettingClone_Should_Not_Clone_CapabilitySetting_Not_Exist()
        {
            var capabilitySettingClone = new CapabilitySettingCloneDto
            {
                FromUserType = Usertype.Internship,
                ToUserType = Usertype.Internship,
                FromPositionId = 17,
                ToPositionId = 18,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _capabilitySetting.CapabilitySettingClone(capabilitySettingClone);
                });

                Assert.Equal("No Capabilites Exist", exception.Message);
            });
        }

        [Fact]
        public async Task CreateCapabilitySetting_Should_Create_Valid_CapabilitySetting()
        {
            var expectTotalCount = 19;

            var capabilitySetting = new CreateUpdateCapabilitySettingDto
            {
                UserType = Usertype.Internship,
                PositionId = 4,
                CapabilityId = 1,
                CapabilityType = CapabilityType.Point,
                GuildeLine = "guide",
                Coefficient = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.CreateCapabilitySetting(capabilitySetting);
                result.ShouldNotBeNull();
                capabilitySetting.Id = result.Id;
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allCapabilitySettings = _workScope.GetAll<CapabilitySetting>();
                Assert.Equal(expectTotalCount, allCapabilitySettings.Count());

                var createdCapabilitySetting = await _workScope.GetAsync<CapabilitySetting>(capabilitySetting.Id);
                createdCapabilitySetting.UserType.ShouldBe(capabilitySetting.UserType);
                createdCapabilitySetting.PositionId.ShouldBe(capabilitySetting.PositionId);
                createdCapabilitySetting.CapabilityId.ShouldBe<long>((long)capabilitySetting.CapabilityId);
                createdCapabilitySetting.GuildeLine.ShouldBe(capabilitySetting.GuildeLine);
                createdCapabilitySetting.Coefficient.ShouldBe(capabilitySetting.Coefficient);
            });
        }

        [Fact]
        public async Task CreateCapabilitySetting_Should_Not_Create_Valid_CapabilitySetting_Exist()
        {
            var expectTotalCount = 18;

            var capabilitySetting = new CreateUpdateCapabilitySettingDto
            {
                UserType = Usertype.Internship,
                PositionId = 1,
                CapabilityId = 1,
                CapabilityType = CapabilityType.Point,
                GuildeLine = "guide",
                Coefficient = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.CreateCapabilitySetting(capabilitySetting);
                result.ShouldNotBeNull();
                capabilitySetting.Id = result.Id;
            });

            WithUnitOfWork(() =>
           {
               var allCapabilitySettings = _workScope.GetAll<CapabilitySetting>();
               Assert.Equal(expectTotalCount, allCapabilitySettings.Count());
           });
        }


        [Fact]
        public async Task UpdateCapabilitySetting_Should_Update_Valid_CapabilitySetting()
        {
            var capabilitySetting = new CreateUpdateCapabilitySettingDto
            {
                Id = 1,
                UserType = Usertype.Internship,
                PositionId = 1,
                CapabilityId = 1,
                CapabilityType = CapabilityType.Point,
                GuildeLine = "guide",
                Coefficient = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.UpdateCapabilitySetting(capabilitySetting);

                result.Id.ShouldBe(capabilitySetting.Id);
                result.UserType.ShouldBe(capabilitySetting.UserType);
                result.PositionId.ShouldBe(capabilitySetting.PositionId);
                result.CapabilityId.ShouldBe<long>((long)capabilitySetting.CapabilityId);
                result.GuildeLine.ShouldBe(capabilitySetting.GuildeLine);
                result.Coefficient.ShouldBe(capabilitySetting.Coefficient);
            });
        }

        [Fact]
        public async Task UpdateCapabilitySetting_Should_Not_Update_CapabilitySetting_Not_Exist()
        {
            var capabilitySetting = new CreateUpdateCapabilitySettingDto
            {
                Id = 999999,
                UserType = Usertype.Internship,
                PositionId = 1,
                CapabilityId = 1,
                CapabilityType = CapabilityType.Point,
                GuildeLine = "guide",
                Coefficient = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    var exception = await _capabilitySetting.UpdateCapabilitySetting(capabilitySetting);
                });
            });
        }

        [Fact]
        public async Task DeleteCapabilitySetting_Should_Delete_CapabilitySetting()
        {
            var expectId = 1;
            var expectTotalCount = 17;

            await WithUnitOfWorkAsync(async () =>
            {
                await _capabilitySetting.DeleteCapabilitySetting(expectId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = _workScope.GetAll<CapabilitySetting>();
                Assert.Equal(expectTotalCount, result.Count());
            });
        }

        [Fact]
        public async Task DeleteCapabilitySetting_Should_Not_Delete_CapabilitySetting_Not_Exist()
        {
            var expectId = 9999;

            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _capabilitySetting.DeleteCapabilitySetting(expectId);
                });
            });
        }

        [Fact]
        public async Task DeleteGroupCapabilitySettings_Should_Delete_Group_CapabilitySetting()
        {
            var userType = Usertype.Internship;
            var positionId = 1;
            var expectTotalCount = 14;

            await WithUnitOfWorkAsync(async () =>
            {
                await _capabilitySetting.DeleteGroupCapabilitySettings(userType, positionId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = _workScope.GetAll<CapabilitySetting>();
                Assert.Equal(expectTotalCount, result.Count());
            });
        }

        [Fact]
        public async Task GetUserTypeForCapabilitySettings_Test()
        {
            var expectTotalCount = 2;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = _capabilitySetting.GetUserTypeForCapabilitySettings();
                Assert.Equal(expectTotalCount, result.Count);

                result.Last().Id.ShouldBe(1);
                result.Last().Name.ShouldBe("Internship");
            });
        }

        [Fact]
        public async Task DeActive_Should_DeActive_CapabilitySetting()
        {
            var expectId = 1;
            var userType = Usertype.Internship;
            var positionId = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _capabilitySetting.DeActive(expectId, userType, positionId);
                Assert.Equal(expectId, result);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _workScope.GetAsync<CapabilitySetting>(expectId);
                result.IsDeleted.ShouldBeFalse();
            });
        }

        [Fact]
        public async Task DeActive_Should_Not_DeActive_CapabilitySetting_Not_Exist()
        {
            var expectId = 99999;
            var userType = Usertype.Internship;
            var positionId = 4;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _capabilitySetting.DeActive(expectId, userType, positionId);
                });

                Assert.Equal("Không có", exception.Message);
            });
        }
    }
}
