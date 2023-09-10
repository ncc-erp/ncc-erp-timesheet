using Abp.Application.Services.Dto;
using Abp.ObjectMapping;
using Abp.UI;
using DocumentFormat.OpenXml.InkML;
using Ncc.IoC;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.OverTimeHours;
using Timesheet.APIs.Positions;
using Timesheet.APIs.Positions.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.Application.Tests.API.PositionAppServiceTest
{
    /// <summary>
    /// 7/7 function public
    /// 24/24 test cases passed 
    /// update day 16/01/2023
    /// </summary>
    public class PositionAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly PositionAppService _positionAppService;
        private readonly IWorkScope _wordkScope;
        private readonly RetroResult _retroReSultAppService;

        public PositionAppService_Tests()
        {
            _retroReSultAppService = Resolve<RetroResult>();
            _wordkScope = Resolve<IWorkScope>();

            _positionAppService = Resolve<PositionAppService>(_wordkScope);
        }

        [Fact]
        public async Task GetAllTest()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAll();

                result.Count.ShouldBeGreaterThanOrEqualTo(1);

                result.First().Name.ShouldBe("Dev");
                result.First().ShortName.ShouldBe("Dev");
                result.First().Code.ShouldBe("Dev\t");
                result.First().Color.ShouldBe("#c81919");
            });
        }

        [Fact]
        //get all paging no skip, no take
        //if no MaxResultCount then take = 10
        public async Task GetAllPagingTest1()
        {
            var expectTotalCount = 8;
            var expectItemCount = 8;

            var input = new GridParam { };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPagging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        //get all paging skip = 3, no take
        public async Task GetAllPagingTest2()
        {
            var expectTotalCount = 8;
            var expectItemCount = 5;
            var skipCount = 3;

            var input = new GridParam
            {
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPagging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
                Assert.NotEqual(expectTotalCount, result.Items.Count);
                Assert.Equal(expectItemCount, result.TotalCount - skipCount);

                result.Items.ShouldContain(Position => Position.Name == "PM");
                result.Items.ShouldContain(Position => Position.ShortName == "PM");
                result.Items.ShouldContain(Position => Position.Code == "PM\t");
                result.Items.ShouldContain(Position => Position.Color == "#2bb65b");
                result.Items.ShouldNotContain(Position => Position.Name == "IT");
            });
        }

        [Fact]
        //get all paging with skip > 8 (max = 8 records) no take
        public async Task GetAllPagingTest3()
        {
            var expectItemCount = 0;
            var skipCount = 9;

            var input = new GridParam
            {
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPagging(input);
                Assert.Equal(expectItemCount, result.Items.Count);
                result.Items.ShouldNotContain(Position => Position.Name == "Art");
            });
        }

        [Fact]
        //get all paging with take = 3  
        public async Task GetAllPagingTest4()
        {
            var takeCount = 3;

            var input = new GridParam
            {
                MaxResultCount = takeCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPagging(input);

                Assert.Equal(takeCount, result.Items.Count);
                result.Items.ShouldContain(Position => Position.Name == "IT");
                result.Items.ShouldContain(Position => Position.ShortName == "IT");
                result.Items.ShouldContain(Position => Position.Code == "IT");
                result.Items.ShouldContain(Position => Position.Color == "#389951");
                result.Items.ShouldNotContain(Position => Position.Name == "PM");
            });
        }

        [Fact]
        //get all paging with skip = 2, take = 3  
        public async Task GetAllPagingTest5()
        {
            var skipCount = 2;
            var takeCount = 3;

            var input = new GridParam
            {
                MaxResultCount = takeCount,
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPagging(input);

                Assert.Equal(takeCount, result.Items.Count);
                Assert.Matches(result.Items[0].ShortName, "IT");
                result.Items.ShouldContain(Position => Position.Name == "IT");
                result.Items.ShouldContain(Position => Position.ShortName == "IT");
                result.Items.ShouldContain(Position => Position.Code == "IT");
                result.Items.ShouldContain(Position => Position.Color == "#389951");
                result.Items.ShouldNotContain(Position => Position.Name == "Tester");
                result.Items.ShouldNotContain(Position => Position.Name == "Sale");
            });
        }

        [Fact]
        //Create position
        public async Task CreateTest1()
        {
            var input = new PositionCreateEditDto
            {
                Id = 9,
                Name = "position9",
                ShortName = "p9",
                Code = "Test",
                Color = "red"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.Create(input);
                
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allpositions = _wordkScope.GetAll<Position>();
                var createdPosition = await _wordkScope.GetAsync<Position>(input.Id);

                allpositions.Count().ShouldBe(9);
                createdPosition.ShouldNotBeNull();
                createdPosition.Name.ShouldBe(input.Name);
                createdPosition.ShortName.ShouldBe(input.ShortName);
                createdPosition.Code.ShouldBe(input.Code);
                createdPosition.Color.ShouldBe(input.Color);
            });
        }

        [Fact]
        //Create position name existed
        public async Task CreateTest2()
        {
            var input = new PositionCreateEditDto
            {
                Name = "Dev",
                ShortName = "p1",
                Code = "Test",
                Color = "red"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Create(input);
                });
                Assert.Equal($"Position name {input.Name} already existed", exception.Message);
            });
        }
        [Fact]
        //Create position short name existed
        public async Task CreateTest3()
        {
            var input = new PositionCreateEditDto
            {
                Name = "position111",
                ShortName = "Dev",
                Code = "Test11",
                Color = "red1"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Create(input);
                });
                Assert.Equal($"Short name {input.ShortName} already existed", exception.Message);
            });
        }
        [Fact]
        //Create position code existed
        public async Task CreateTest4()
        {
            var input = new PositionCreateEditDto
            {
                Name = "position1111",
                ShortName = "p1111",
                Code = "Dev\t",
                Color = "red1"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Create(input);
                });
                Assert.Equal($"Code {input.Code} already existed", exception.Message);
            });
        }

        [Fact]
        //Update position
        public async Task UpdateTest1()
        {
            var expectId = 1;
            var input = new PositionCreateEditDto
            {
                Id = 1,
                Name = "position1111",
                ShortName = "p1111",
                Code = "Test",
                Color = "red1"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.Update(input);
                var getAllPositions = _positionAppService.GetAll();
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allpositions = _wordkScope.GetAll<Position>();
                var updatedPosition = await _wordkScope.GetAsync<Position>(expectId);

                updatedPosition.ShouldNotBeNull();
                updatedPosition.Name.ShouldBe(input.Name);
                updatedPosition.ShortName.ShouldBe(input.ShortName);
                updatedPosition.Code.ShouldBe(input.Code);
                updatedPosition.Color.ShouldBe(input.Color);
            });
        }

        [Fact]
        //Update position name existed
        public async Task UpdateTest2()
        {
            var input = new PositionCreateEditDto
            {
                Id = 1,
                Name = "PM",
                ShortName = "p11",
                Code = "Test11",
                Color = "red11"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Update(input);
                });
                Assert.Equal($"Position name {input.Name} already existed", exception.Message);
            });
        }
        [Fact]
        //Update position short name existed
        public async Task UpdateTest3()
        {
            var input = new PositionCreateEditDto
            {
                Id = 1,
                Name = "position111",
                ShortName = "PM",
                Code = "Test11",
                Color = "red1"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Update(input);
                });
                Assert.Equal($"Short name {input.ShortName} already existed", exception.Message);
            });
        }
        [Fact]
        //Update position code existed
        public async Task UpdateTest4()
        {
            var input = new PositionCreateEditDto
            {
                Id = 1,
                Name = "position1111",
                ShortName = "p1111",
                Code = "PM\t",
                Color = "red1"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Update(input);
                });
                Assert.Equal($"Code {input.Code} already existed", exception.Message);
            });
        }

        [Fact]
        //get all Position no skip, no take
        //if no MaxResultCount then take = 10
        public async Task GetAllPositionTest1()
        {
            var expectTotalCount = 8;
            var expectItemCount = 8;

            var input = new GridParam { };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPosition(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        //get all Position skip = 3, no take
        public async Task GetAllPositionTest2()
        {
            var expectTotalCount = 8;
            var expectItemCount = 5;
            var skipCount = 3;

            var input = new GridParam
            {
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPosition(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
                Assert.NotEqual(expectTotalCount, result.Items.Count);
                Assert.Equal(expectItemCount, result.TotalCount - skipCount);

                result.Items.ShouldContain(Position => Position.Name == "PM");
                result.Items.ShouldContain(Position => Position.ShortName == "PM");
                result.Items.ShouldContain(Position => Position.Code == "PM\t");
                result.Items.ShouldContain(Position => Position.Color == "#2bb65b");
                result.Items.ShouldNotContain(Position => Position.Name == "IT");
            });
        }

        [Fact]
        //get all Position with skip > 8 (max = 8 records) no take
        public async Task GetAllPositionTest3()
        {
            var expectItemCount = 0;
            var skipCount = 9;

            var input = new GridParam
            {
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPosition(input);
                Assert.Equal(expectItemCount, result.Items.Count);
                result.Items.ShouldNotContain(Position => Position.Name == "position14");
            });
        }

        [Fact]
        //get all Position with take = 3  
        public async Task GetAllPositionTest4()
        {
            var takeCount = 3;

            var input = new GridParam
            {
                MaxResultCount = takeCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPosition(input);

                Assert.Equal(takeCount, result.Items.Count);
                result.Items.ShouldContain(Position => Position.Name == "IT");
                result.Items.ShouldContain(Position => Position.ShortName == "IT");
                result.Items.ShouldContain(Position => Position.Code == "IT");
                result.Items.ShouldContain(Position => Position.Color == "#389951");
                result.Items.ShouldNotContain(Position => Position.Name == "PM");
            });
        }

        [Fact]
        //get all Position with skip = 2, take = 3  
        public async Task GetAllPositionTest5()
        {
            var skipCount = 2;
            var takeCount = 3;

            var input = new GridParam
            {
                MaxResultCount = takeCount,
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPosition(input);

                Assert.Equal(takeCount, result.Items.Count);
                Assert.Matches(result.Items[0].ShortName, "IT");
                result.Items.ShouldContain(Position => Position.Name == "IT");
                result.Items.ShouldContain(Position => Position.ShortName == "IT");
                result.Items.ShouldContain(Position => Position.Code == "IT");
                result.Items.ShouldContain(Position => Position.Color == "#389951");
                result.Items.ShouldNotContain(Position => Position.Name == "Tester");
                result.Items.ShouldNotContain(Position => Position.Name == "Sale");
            });
        }

        [Fact]
        public async Task DeleteTest1()
        {
            var expectTotalCount = 7;
            var positionToDelete = new EntityDto<long>
            {
                Id = 6,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _positionAppService.Delete(positionToDelete);
            });

            await WithUnitOfWorkAsync(() => {
                var allPositions = _wordkScope.GetAll<Position>();
                var position = _wordkScope.GetAll<Position>()
                 .Where(s => s.Id == positionToDelete.Id);

                allPositions.Count().ShouldBe(expectTotalCount);
                position.ShouldBeEmpty();
                return Task.CompletedTask;
            });
        }

        [Fact]
        // Delete Position with UserFriendlyException id not found
        public async Task DeleteTest2()
        {
            var positionToDelete = new EntityDto<long>
            {
                Id = 9999,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Delete(positionToDelete);
                });
                Assert.Equal($"There is no entity Position with id = {positionToDelete.Id}!", exception.Message);
            });
        }

        [Fact]
        // Delete Position with UserFriendlyException position already in retro result
        public async Task DeleteTest3()
        {
            var positionToDelete = new EntityDto<long>
            {
                Id = 4,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var hasUserPosition = _wordkScope.GetAsync<RetroResult>(1);
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Delete(positionToDelete);
                });
                Assert.Equal("This position is already in the record of retro results", exception.Message);
            });
        }

        [Fact]
        // Delete Position with UserFriendlyException position id has user
        public async Task DeleteTest4()
        {
            var positionToDelete = new EntityDto<long>
            {
                Id = 5,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _positionAppService.Delete(positionToDelete);
                });
                Assert.Equal($"Position Id {positionToDelete.Id} has user", exception.Message);
            });
        }

        [Fact]
        //Get All Position DropDownList Test
        public async Task GetAllPositionDropDownListTest1()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _positionAppService.GetAllPositionDropDownList();

                result.Count.ShouldBeGreaterThanOrEqualTo(1);

                result.First().Name.ShouldBe("Dev");
                result.First().ShortName.ShouldBe("Dev");
                result.First().Code.ShouldBe("Dev\t");
                result.First().Color.ShouldBe("#c81919");
            });
        }

    }
}
