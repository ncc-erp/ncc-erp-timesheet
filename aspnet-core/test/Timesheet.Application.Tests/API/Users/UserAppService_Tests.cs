using Abp.Application.Services.Dto;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Entities;
using Ncc.IoC;
using Ncc.Users;
using Shouldly;
using Timesheet.DomainServices.Dto;
using Timesheet.Timesheets.Projects;
using Xunit;

namespace Timesheet.Application.Tests.API.Users
{
    public class UserAppService_Tests : UserAppServiceTestBase
    {
        private readonly UserAppService _userAppService;

        public UserAppService_Tests()
        {
            _userAppService = InstanceUserAppService();
        }

        [Fact]
        public async System.Threading.Tasks.Task DeactiveUser_Should_Throw_Exception_When_User_Is_Only_PM()
        {
            var userId = await CreateUser("only_pm");
            var projectName = "Only_PM_Project";
            await SetupUserAsOnlyActivePM(userId, projectName);

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    await _userAppService.DeactiveUser(new EntityDto<long>(userId)));

                exception.Message.ShouldBe($"Cannot deactivate because this user is the only active PM in project(s): \"{projectName}\".");
            });
        }

        [Fact]
        public async System.Threading.Tasks.Task DeactiveUser_Should_Succeed_When_Project_Has_Other_Active_PM()
        {
            var userId = await CreateUser("pm_test");
            var projectId = await SetupUserAsOnlyActivePM(userId, "Project X");
            await AddAnotherActivePMToProject(projectId, "reserved_pm");

            await WithUnitOfWorkAsync(async () =>
            {
                await _userAppService.DeactiveUser(new EntityDto<long>(userId));
            });

            UsingDbContext(context =>
            {
                var user = context.Users.Find(userId);
                user.IsActive.ShouldBeFalse();
                user.EndDateAt.ShouldNotBeNull();
            });
        }

        [Fact]
        public async System.Threading.Tasks.Task DeactiveUser_Should_Throw_Exception_When_Other_PM_Is_Inactive()
        {
            var projectName = "Project X";

            var userAId = await CreateUser("user_A_active", true);

            var userBId = await CreateUser("user_B_inactive", false);

            var projectId = await SetupUserAsOnlyActivePM(userAId, projectName);

            await WithUnitOfWorkAsync(async () =>
            {
                var workScope = Resolve<IWorkScope>();
                await workScope.InsertAsync(new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = userBId,
                    Type = ProjectUserType.PM
                });
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    await _userAppService.DeactiveUser(new EntityDto<long>(userAId)));

                exception.Message.ShouldBe($"Cannot deactivate because this user is the only active PM in project(s): \"{projectName}\".");
            });
        }
    }
}
