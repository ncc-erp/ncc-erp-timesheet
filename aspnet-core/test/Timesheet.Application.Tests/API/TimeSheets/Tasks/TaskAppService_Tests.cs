using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Uow;
using Abp.UI;
using Ncc.IoC;
using Shouldly;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.Timesheets.Tasks;
using Timesheet.Timesheets.Tasks.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.Timesheets.Tasks
{
    /// <summary>
    /// 5/5 functions
    /// 9/9 test cases passed
    /// update day 17/01/2023
    /// </summary>

    public class TaskAppService_Tests : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;
        private readonly TaskAppService _taskAppService;

        public TaskAppService_Tests()
        {
            _workScope = Resolve<IWorkScope>();
            _taskAppService = new TaskAppService(_workScope)
            {
                ObjectMapper = Resolve<Abp.ObjectMapping.IObjectMapper>(),
                UnitOfWorkManager = Resolve<IUnitOfWorkManager>(),
            };

            _workScope.Insert<Ncc.Entities.Task>(new Ncc.Entities.Task
            {
                Name = "Test Task",
                Type = Ncc.Entities.Enum.StatusEnum.TaskType.OrtherTask,
                IsDeleted = true,
            });
        }

        [Fact]
        public async Task GetAll_Should_Get_All()
        {
            var expectTotalCount = 11;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _taskAppService.GetAll();

                result.Count.ShouldBeGreaterThanOrEqualTo(expectTotalCount);

                result.First().Id.ShouldBe(1);
                result.First().IsDeleted.ShouldBe(false);
                result.First().Name.ShouldBe("Coding");
                result.First().Type.ShouldBe(Ncc.Entities.Enum.StatusEnum.TaskType.CommonTask);
            });
        }

        [Fact]
        public async Task Delete_Should_Delete()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _taskAppService.Delete(new EntityDto<long>
                {
                    Id = 11,
                });
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<Ncc.Entities.Task>();
                result.Count().ShouldBe(10);
            });
        }

        [Fact]
        public async Task Delete_Should_Not_Delete_Task_Is_In_Project()
        {
            long id = 3;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _taskAppService.Delete(new EntityDto<long>
                    {
                        Id = id,
                    });
                });
                exception.Message.ShouldBe(string.Format("This taskId {0} is in a project ,You can't delete task", id));
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = _workScope.GetAll<Ncc.Entities.Task>();
                result.Count().ShouldBe(10);
            });
        }

        [Fact]
        public async Task Save_Should_Save()
        {
            var expectTask = new TaskDto
            {
                Name = "Test8686",
                Type = 0,
                IsDeleted = false,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _taskAppService.Save(expectTask);

                result.ShouldNotBeNull();
                expectTask.Id = result.Id;
                result.Name.ShouldBe(expectTask.Name);
                result.Type.ShouldBe(expectTask.Type);
                result.IsDeleted.ShouldBeFalse();
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var allTask = _workScope.GetAll<Ncc.Entities.Task>();
                var task = await _workScope.GetAsync<Ncc.Entities.Task>(expectTask.Id);

                allTask.Count().ShouldBe(11);
                allTask.ToList().Find(item => item.Name == expectTask.Name).ShouldNotBeNull();
                task.Id.ShouldBe(expectTask.Id);
                task.Name.ShouldBe(expectTask.Name);
                task.Type.ShouldBe(expectTask.Type);
                task.IsDeleted.ShouldBeFalse();
            });
        }

        [Fact]
        public async Task Save_Should_Not_Save_Task_Exist()
        {

            var expectTask = new TaskDto
            {
                Name = "Coding",
                Type = 0,
                IsDeleted = false,
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _taskAppService.Save(expectTask);
                });
                Assert.Equal(string.Format("Task {0} is already exist", expectTask.Name), exception.Message);
            });
        }

        [Fact]
        public async Task Archive_Should_Archive()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _taskAppService.Archive(new EntityDto<long>
                {
                    Id = 11,
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = _workScope.GetAll<Ncc.Entities.Task>();
                result.Count().ShouldBe(10);
            });
        }

        [Fact]
        public async Task Archive_Should_Not_Archive_Task_Is_In_Project()
        {
            long id = 3;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _taskAppService.Archive(new EntityDto<long>
                    {
                        Id = id,
                    });
                });
                exception.Message.ShouldBe(string.Format("This taskId {0} is in a project ,You can't delete task", id));
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<Ncc.Entities.Task>();
                result.Count().ShouldBe(10);
            });
        }

        [Fact]
        public async Task DeArchive_Should_DeArchive()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _taskAppService.DeArchive(new EntityDto<long>
                {
                    Id = 11,
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _workScope.GetAsync<Ncc.Entities.Task>(11);
                result.IsDeleted.ShouldBeFalse();
                _workScope.GetAll<Ncc.Entities.Task>().Count().ShouldBe(11);
            });
        }

        [Fact]
        public async Task DeArchive_Should_Not_DeArchive_Task_Exist()
        {
            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<Ncc.Entities.Task>();
                result.Count().ShouldBe(10);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                {
                    await _taskAppService.DeArchive(new EntityDto<long>
                    {
                        Id = 99999,
                    });
                });
            });

            WithUnitOfWork(() =>
            {
                var result = _workScope.GetAll<Ncc.Entities.Task>();
                result.Count().ShouldBe(10);
            });
        }
    }
}
