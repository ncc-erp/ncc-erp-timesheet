using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.EntityFrameworkCore;
using Ncc.Users;
using Ncc.Users.Dto;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Tests.Users
{
    public class UserAppService_Tests : UserAppServiceTestBase
    {
        private readonly UserAppService _userAppService;

        public UserAppService_Tests()
        {
            _userAppService = InstanceUserAppService();
        }

        //[Fact]
        //public async Task GetUsers_Test()
        //{
        //    // Act
        //    var output = await _userAppService.GetAll(new PagedUserResultRequestDto{MaxResultCount=20, SkipCount=0} );

        //    // Assert
        //    output.Items.Count.ShouldBeGreaterThan(0);
        //}

        //[Fact]
        //public async Task CreateUser_Test()
        //{
        //    // Act
        //    await _userAppService.Create(
        //        new CreateUserDto
        //        {
        //            EmailAddress = "john@volosoft.com",
        //            IsActive = true,
        //            Name = "John",
        //            Surname = "Nash",
        //            Password = "123qwe",
        //            UserName = "john.nash"
        //        });

        //    await UsingDbContextAsync(async context =>
        //    {
        //        var johnNashUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "john.nash");
        //        johnNashUser.ShouldNotBeNull();   
        //    });
        //}
        [Fact]
        public async void DeactiveUser_Should_Deactive_Success()
        {
            // Arrange
            await WithUnitOfWorkAsync(async () =>
            {
                var testUserId = await CreateTestUserAsync();
                long testProjectId = 0;

                await UsingDbContextAsync(async context =>
                {
                    var project = new Project
                    {
                        Name = "Test Project",
                        Status = ProjectStatus.Active
                    };
                    context.Projects.Add(project);
                    await context.SaveChangesAsync();
                    testProjectId = project.Id;

                    context.ProjectUsers.Add(new ProjectUser
                    {
                        UserId = testUserId,
                        ProjectId = testProjectId,
                        Type = ProjectUserType.Member
                    });

                    await context.SaveChangesAsync();
                });
                await _userAppService.DeactiveUser(new EntityDto<long>(testUserId));
                await UsingDbContextAsync(async context =>
                {
                    var user = await context.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
                    user.ShouldNotBeNull();
                    user.IsActive.ShouldBeFalse();
                    user.EndDateAt.ShouldNotBeNull();

                    var projectUsers = await context.ProjectUsers
                        .Where(pu => pu.UserId == testUserId)
                        .ToListAsync();

                    projectUsers.ShouldAllBe(pu => pu.Type == ProjectUserType.DeActive);
                });
            });
        }

        [Fact]
        public async void DeactiveUser_Should_Throw_When_User_Not_Exist()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _userAppService.DeactiveUser(new EntityDto<long>(9999));
                });
            });
        }

        [Fact]
        public async void DeactiveUser_Should_Throw_When_PM_Is_Only_PM_In_Active_Project()
        {
            var userId = await CreateTestUserAsync();

            long projectId = await UsingDbContextAsync(async context =>
            {
                var project = new Project
                {
                    Name = "PM Project",
                    Status = ProjectStatus.Active,
                    Code = "TEST001"
                };

                context.Projects.Add(project);
                await context.SaveChangesAsync();

                context.ProjectUsers.Add(new ProjectUser
                {
                    UserId = userId,
                    ProjectId = project.Id,
                    Type = ProjectUserType.PM
                });
                await context.SaveChangesAsync();

                return project.Id;
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    await _userAppService.DeactiveUser(new EntityDto<long>(userId)));

                exception.Message.ShouldContain("Cannot deactivate the only PM");
                exception.Message.ShouldContain("active project");
            });
        }


        [Fact]
        public async void DeactiveUser_Should_Allow_When_Project_Is_Deactive()
        {
            var userId = await CreateTestUserAsync();

            var projectId = await UsingDbContextAsync(async context =>
            {
                var project = new Project
                {
                    Name = "Deactive Project",
                    Status = ProjectStatus.Deactive,
                    Code = "TEST002"
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();

                context.ProjectUsers.Add(new ProjectUser
                {
                    UserId = userId,
                    ProjectId = project.Id,
                    Type = ProjectUserType.PM
                });

                await context.SaveChangesAsync();
                return project.Id;
            });
            await WithUnitOfWorkAsync(async () =>
                await _userAppService.DeactiveUser(new EntityDto<long>(userId))
            );
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstAsync(u => u.Id == userId);
                user.IsActive.ShouldBeFalse();
                user.EndDateAt.ShouldNotBeNull();

                var projectUser = await context.ProjectUsers
                    .FirstAsync(pu => pu.UserId == userId && pu.ProjectId == projectId);

                projectUser.Type.ShouldBe(ProjectUserType.DeActive);
            });
        }


        [Fact]
        public async void DeactiveUser_Should_Allow_When_Multiple_PMs_In_Active_Project()
        {
            var pm1Id = await CreateTestUserAsync();
            var pm2Id = await CreateTestUserAsync();

            var projectId = await UsingDbContextAsync(async context =>
            {
                var project = new Project
                {
                    Name = "Multi PM Project",
                    Status = ProjectStatus.Active,
                    Code = "TEST003"
                };

                context.Projects.Add(project);
                await context.SaveChangesAsync();

                context.ProjectUsers.AddRange(
                    new ProjectUser { UserId = pm1Id, ProjectId = project.Id, Type = ProjectUserType.PM },
                    new ProjectUser { UserId = pm2Id, ProjectId = project.Id, Type = ProjectUserType.PM }
                );

                await context.SaveChangesAsync();
                return project.Id;
            });
            await WithUnitOfWorkAsync(async () =>
                await _userAppService.DeactiveUser(new EntityDto<long>(pm1Id))
            );
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstAsync(u => u.Id == pm1Id);
                user.IsActive.ShouldBeFalse();
                user.EndDateAt.ShouldNotBeNull();

                var projectUser = await context.ProjectUsers
                    .FirstAsync(pu => pu.UserId == pm1Id && pu.ProjectId == projectId);
                projectUser.Type.ShouldBe(ProjectUserType.DeActive);
            });
        }


    }
}