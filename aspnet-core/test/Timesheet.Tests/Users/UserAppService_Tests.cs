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

        [Fact]
        public async void DeactiveUser_Should_Deactive_Success()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                long testUserId = 0;
                long testProjectId = 0;

                await UsingDbContextAsync(async context =>
                {
                    var existingUser = await context.Users
                        .Where(u => u.IsActive)
                        .FirstOrDefaultAsync();

                    existingUser.ShouldNotBeNull("Không tìm thấy user active trong DB");
                    testUserId = existingUser.Id;

                    var project = new Project
                    {
                        Name = "Test Project",
                        Status = ProjectStatus.Active,
                        Code = "TEST_" + Guid.NewGuid().ToString().Substring(0, 8)
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
                        .Where(pu => pu.UserId == testUserId && pu.ProjectId == testProjectId)
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
                long nonExistentUserId = 0;
                await UsingDbContextAsync(async context =>
                {
                    var maxId = await context.Users.MaxAsync(u => (long?)u.Id) ?? 0;
                    nonExistentUserId = maxId + 1000;
                });

                await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _userAppService.DeactiveUser(new EntityDto<long>(nonExistentUserId));
                });
            });
        }

        [Fact]
        public async void DeactiveUser_Should_Throw_When_PM_Is_Only_PM_In_Active_Project()
        {
            long testUserId = 0;
            long testProjectId = 0;

            await UsingDbContextAsync(async context =>
            {
                var existingUser = await context.Users
                    .Where(u => u.IsActive)
                    .FirstOrDefaultAsync();

                existingUser.ShouldNotBeNull("Không tìm thấy user active trong DB");
                testUserId = existingUser.Id;

                var project = new Project
                {
                    Name = "PM Project",
                    Status = ProjectStatus.Active,
                    Code = "PM_TEST_" + Guid.NewGuid().ToString().Substring(0, 8)
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();
                testProjectId = project.Id;

                context.ProjectUsers.Add(new ProjectUser
                {
                    UserId = testUserId,
                    ProjectId = testProjectId,
                    Type = ProjectUserType.PM
                });
                await context.SaveChangesAsync();
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _userAppService.DeactiveUser(new EntityDto<long>(testUserId));
                });

                exception.Message.ShouldContain("Cannot deactivate the only PM");
                exception.Message.ShouldContain("active project");
            });

            await UsingDbContextAsync(async context =>
            {
                var projectUser = await context.ProjectUsers
                    .FirstOrDefaultAsync(pu => pu.UserId == testUserId && pu.ProjectId == testProjectId);
                if (projectUser != null)
                {
                    context.ProjectUsers.Remove(projectUser);
                }

                var project = await context.Projects.FindAsync(testProjectId);
                if (project != null)
                {
                    context.Projects.Remove(project);
                }

                await context.SaveChangesAsync();
            });
        }

        [Fact]
        public async void DeactiveUser_Should_Allow_When_Project_Is_Deactive()
        {
            long testUserId = 0;
            long testProjectId = 0;

            await UsingDbContextAsync(async context =>
            {
                var existingUser = await context.Users
                    .Where(u => u.IsActive)
                    .FirstOrDefaultAsync();

                existingUser.ShouldNotBeNull("Không tìm thấy user active trong DB");
                testUserId = existingUser.Id;

                var project = new Project
                {
                    Name = "Deactive Project",
                    Status = ProjectStatus.Deactive,
                    Code = "DEACT_TEST_" + Guid.NewGuid().ToString().Substring(0, 8)
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();
                testProjectId = project.Id;

                context.ProjectUsers.Add(new ProjectUser
                {
                    UserId = testUserId,
                    ProjectId = testProjectId,
                    Type = ProjectUserType.PM
                });
                await context.SaveChangesAsync();
            });

            // Act
            await WithUnitOfWorkAsync(async () =>
            {
                await _userAppService.DeactiveUser(new EntityDto<long>(testUserId));
            });

            // Assert
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
                user.ShouldNotBeNull();
                user.IsActive.ShouldBeFalse();
                user.EndDateAt.ShouldNotBeNull();

                var projectUser = await context.ProjectUsers
                    .FirstOrDefaultAsync(pu => pu.UserId == testUserId && pu.ProjectId == testProjectId);
                projectUser.Type.ShouldBe(ProjectUserType.DeActive);
            });
        }

        [Fact]
        public async void DeactiveUser_Should_Allow_When_Multiple_PMs_In_Active_Project()
        {
            long testUserId1 = 0;
            long testUserId2 = 0;
            long testProjectId = 0;

            await UsingDbContextAsync(async context =>
            {
                var existingUsers = await context.Users
                    .Where(u => u.IsActive)
                    .Take(2)
                    .ToListAsync();

                existingUsers.Count.ShouldBeGreaterThanOrEqualTo(2, "Cần ít nhất 2 users active trong DB");
                testUserId1 = existingUsers[0].Id;
                testUserId2 = existingUsers[1].Id;

                var project = new Project
                {
                    Name = "Multi PM Project",
                    Status = ProjectStatus.Active,
                    Code = "MULTI_PM_" + Guid.NewGuid().ToString().Substring(0, 8)
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();
                testProjectId = project.Id;

                context.ProjectUsers.AddRange(
                    new ProjectUser
                    {
                        UserId = testUserId1,
                        ProjectId = testProjectId,
                        Type = ProjectUserType.PM
                    },
                    new ProjectUser
                    {
                        UserId = testUserId2,
                        ProjectId = testProjectId,
                        Type = ProjectUserType.PM
                    }
                );
                await context.SaveChangesAsync();
            });

            // Act
            await WithUnitOfWorkAsync(async () =>
            {
                await _userAppService.DeactiveUser(new EntityDto<long>(testUserId1));
            });

            // Assert
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == testUserId1);
                user.ShouldNotBeNull();
                user.IsActive.ShouldBeFalse();
                user.EndDateAt.ShouldNotBeNull();

                var projectUser = await context.ProjectUsers
                    .FirstOrDefaultAsync(pu => pu.UserId == testUserId1 && pu.ProjectId == testProjectId);
                projectUser.Type.ShouldBe(ProjectUserType.DeActive);
            });
        }
    }
}