using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.UI;
using Microsoft.Office.Interop.Word;
using Ncc.Authorization.Users;
using Ncc.Entities.Enum;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using Timesheet.APIs.Retros;
using Timesheet.APIs.Retros.Dto;
using Timesheet.Constants;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Project.Dto;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.HRMV2
{
    public class RetroAppService_Test : TimesheetApplicationTestBase
    {
        /// <summary>
        /// 5/5 function public
        /// 18/18 test cases passed 
        /// update day 16/01/2023
        /// </summary>
        private readonly RetroAppService _retroAppService;
        private readonly IWorkScope _workScope;

        public RetroAppService_Test()
        {
            _workScope = Resolve<IWorkScope>();
            _retroAppService = Resolve<RetroAppService>();

        }


        [Fact]
        //get all paging no skip, no take
        //if no MaxResultCount then take = 10
        public async void GetAllPagingTest1()
        {
            var expectTotalCount = 4;
            var expectItemCount = 4;

            var input = new GridParam { };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.GetAllPagging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
            });
        }

        [Fact]
        //get all paging skip = 1, no take
        public async void GetAllPagingTest2()
        {
            var expectTotalCount = 4;
            var expectItemCount = 3;
            var skipCount = 1;

            var input = new GridParam
            {
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.GetAllPagging(input);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);
                Assert.NotEqual(expectTotalCount, result.Items.Count);
                Assert.Equal(expectItemCount, result.TotalCount - skipCount);

                result.Items.First().Id.ShouldBe(1);
                result.Items.First().Name.ShouldBe("Retro tháng 09 - 2022");
                result.Items.First().StartDate.ShouldBe(new DateTime(2022, 09, 01));
                result.Items.First().EndDate.ShouldBe(new DateTime(2022, 09, 30));
                result.Items.First().Deadline.ShouldBe(new DateTime(2022, 09, 30));
                result.Items.First().Status.ShouldBe(RetroStatus.Public);
                result.Items.First().Id.ShouldNotBe(2);
            });
        }

        [Fact]
        //get all paging with skip > 4 (max = 4 records) no take
        public async void GetAllPagingTest3()
        {
            var expectItemCount = 0;
            var skipCount = 5;

            var input = new GridParam
            {
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.GetAllPagging(input);
                Assert.Equal(expectItemCount, result.Items.Count);
                result.Items.ShouldNotContain(Retro => Retro.Name == "Retro tháng 10 - 2022");
            });
        }

        [Fact]
        //get all paging with take = 1  
        public async void GetAllPagingTest4()
        {
            var takeCount = 1;

            var input = new GridParam
            {
                MaxResultCount = takeCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.GetAllPagging(input);

                Assert.Equal(takeCount, result.Items.Count);
                result.Items.ShouldContain(Retro => Retro.Name == "Retro tháng 10 - 2022");
                result.Items.ShouldNotContain(Retro => Retro.Name == "Retro tháng 09 - 2022");
            });
        }

        [Fact]
        //get all paging with skip = 1, take = 1  
        public async void GetAllPagingTest5()
        {
            var skipCount = 1;
            var takeCount = 1;

            var input = new GridParam
            {
                MaxResultCount = takeCount,
                SkipCount = skipCount,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.GetAllPagging(input);

                Assert.Equal(takeCount, result.Items.Count);
                Assert.Matches(result.Items[0].Name, "Retro tháng 09 - 2022");
                result.Items.ShouldContain(Retro => Retro.Name == "Retro tháng 09 - 2022");
                result.Items.ShouldContain(Retro => Retro.StartDate == new DateTime(2022, 09, 01));
                result.Items.ShouldContain(Retro => Retro.EndDate == new DateTime(2022, 09, 30));
                result.Items.ShouldContain(Retro => Retro.Deadline == new DateTime(2022, 09, 30));
                result.Items.ShouldContain(Retro => Retro.Status == RetroStatus.Public);
                result.Items.ShouldNotContain(Retro => Retro.Id == 2);
                result.Items.ShouldNotContain(Retro => Retro.Name == "Retro tháng 10 - 2022");
            });
        }

        [Fact]
        //Create Retro
        public async void CreateTest1()
        {
            var expectId = 5;
            var input = new RetroCreateDto
            {
                Id = expectId,
                Name = "retro5",
                StartDate = new DateTime(2022, 11, 01),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 25),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.Create(input);
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allretros = _workScope.GetAll<Retro>();
                var createdretro = await _workScope.GetAsync<Retro>(expectId);

                allretros.Count().ShouldBe(5);
                createdretro.ShouldNotBeNull();
                createdretro.Name.ShouldBe(input.Name);
                createdretro.StartDate.ShouldBe(input.StartDate);
                createdretro.EndDate.ShouldBe(input.EndDate);
                createdretro.Deadline.ShouldBe(input.Deadline);
                createdretro.Status.ShouldBe(input.Status);
            });
        }

        [Fact]
        //Create Retro start date > end date
        public async void CreateTest2()
        {
            var input = new RetroCreateDto
            {
                Id = 3,
                Name = "retro1",
                StartDate = new DateTime(2022, 11, 28),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 25),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Create(input);
                });
                Assert.Equal("Start date > end date", exception.Message);
            });
        }

        [Fact]
        //Create Retro start date > dead line
        public async void CreateTest3()
        {
            var input = new RetroCreateDto
            {
                Id = 3,
                Name = "retro1",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 01),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Create(input);
                });
                Assert.Equal("Start date > deadline", exception.Message);
            });
        }

        [Fact]
        //Create Retro name already existed
        public async void CreateTest4()
        {
            var input = new RetroCreateDto
            {
                Id = 3,
                Name = "Retro tháng 09 - 2022",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 25),
                Deadline = new DateTime(2022, 11, 25),
                Status = RetroStatus.Public
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Create(input);
                });
                Assert.Equal($"{input.Name} ({input.StartDate:MM/yyyy}) already existed", exception.Message);
            });
        }

        [Fact]
        public async void DeleteTest1()
        {
            var expectTotalCount = 3;
            var retroToDelete = new EntityDto<long>
            {
                Id = 4,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _retroAppService.Delete(retroToDelete);
            });

            await WithUnitOfWorkAsync(() => {
                var allretros = _workScope.GetAll<Retro>();
                var retro = _workScope.GetAll<Retro>()
                 .Where(s => s.Id == retroToDelete.Id);

                allretros.Count().ShouldBe(expectTotalCount);
                retro.ShouldBeEmpty();
                return System.Threading.Tasks.Task.CompletedTask;
            });
        }

        [Fact]
        // Delete Retro with UserFriendlyException has retro detail
        public async void DeleteTest2()
        {
            var retroToDelete = new EntityDto<long>
            {
                Id = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Delete(retroToDelete);
                });
                Assert.Equal($"Retro Id {retroToDelete.Id} has retro detail", exception.Message);
            });
        }

        [Fact]
        // Delete Retro with UserFriendlyException retro status close
        public async void DeleteTest3()
        {
            var retroToDelete = new EntityDto<long>
            {
                Id = 3,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _retroAppService.Delete(retroToDelete);
                });
                Assert.Equal("Cannot be deleted because the retro status close", exception.Message);
            });
        }


        [Fact]
        // Delete Retro with UserFriendlyException entity not found
        public async void DeleteTest4()
        {
            var retroToDelete = new EntityDto<long>
            {
                Id = 9999,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _retroAppService.Delete(retroToDelete);
                });
            });
        }

        [Fact]
        //Change Status Retro
        public async void ChangeStatusTest1()
        {
            var expectId = 1;

            var retroToChangeStatus = new EntityDto<long>
            {
                Id = 1,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _retroAppService.ChangeStatus(retroToChangeStatus);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var changedStatusRetro = await _workScope.GetAsync<Retro>(expectId);

                changedStatusRetro.ShouldNotBeNull();
                changedStatusRetro.Id.ShouldBe(retroToChangeStatus.Id);
                changedStatusRetro.Status.ShouldBe(StatusEnum.RetroStatus.Close);
            });
        }

        [Fact]
        //Change Status Retro status = close
        public async void ChangeStatusTest2()
        {
            var expectId = 3;

            var retroToChangeStatus = new EntityDto<long>
            {
                Id = 3,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                await _retroAppService.ChangeStatus(retroToChangeStatus);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var changedStatusRetro = await _workScope.GetAsync<Retro>(expectId);

                changedStatusRetro.ShouldNotBeNull();
                changedStatusRetro.Id.ShouldBe(retroToChangeStatus.Id);
                changedStatusRetro.Status.ShouldBe(StatusEnum.RetroStatus.Public);
            });
        }

        [Fact]
        //Change Status Retro id not found
        public async void ChangeStatusTest3()
        {
            var retroToChangeStatus = new EntityDto<long>
            {
                Id = 1000,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _retroAppService.ChangeStatus(retroToChangeStatus);
                });
            });
        }

        [Fact]
        //Update Retro
        public async void UpdateTest1()
        {
            var expectId = 1;

            var input = new RetroEditDto
            {
                Id = expectId,
                Name = "retro2",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 30),
                Deadline = new DateTime(2022, 11, 30),
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _retroAppService.Update(input);
                Assert.NotNull(result);
                result.ShouldBe(input);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var changedStatusRetro = await _workScope.GetAsync<Retro>(expectId);

                changedStatusRetro.ShouldNotBeNull();
                changedStatusRetro.Id.ShouldBe(input.Id);
                changedStatusRetro.StartDate.ShouldBe(input.StartDate);
                changedStatusRetro.EndDate.ShouldBe(input.EndDate);
                changedStatusRetro.Deadline.ShouldBe(input.Deadline);
            });
        }

        [Fact]
        //Update Retro  retro existed
        public async void UpdateTest2()
        {
            var expectId = 2;

            var input = new RetroEditDto
            {
                Id = expectId,
                Name = "Retro tháng 09 - 2022",
                StartDate = new DateTime(2022, 11, 15),
                EndDate = new DateTime(2022, 11, 30),
                Deadline = new DateTime(2022, 11, 30),
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    var result = await _retroAppService.Update(input);
                });
                Assert.Equal($"Retro {input.Name} already existed", exception.Message);
            });

        }
    }
}
