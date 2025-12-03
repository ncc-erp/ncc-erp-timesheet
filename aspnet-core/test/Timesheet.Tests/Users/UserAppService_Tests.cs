using Abp.Application.Services.Dto;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.Users;
using Ncc.Users.Dto;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Tests.Users
{
    public class UserAppService_Tests : TimesheetTestBase
    {
        private readonly IUserAppService _userAppService;

        public UserAppService_Tests()
        {
            _userAppService = Resolve<IUserAppService>();


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
            long testUserId = 0;
            long testProjectId = 0;

            await UsingDbContextAsync(async context =>
            {
                var user = new User
                {
                    UserName = "test.user",
                    Name = "Test",
                    Surname = "User",
                    EmailAddress = "test@example.com",
                    IsActive = true
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                testUserId = user.Id;

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

            // Act
            await _userAppService.DeactiveUser(new EntityDto((int)testUserId)); 

            // Assert
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
                user.IsActive.ShouldBeFalse();
                user.EndDateAt.ShouldNotBeNull();

                var projectUsers = await context.ProjectUsers
                    .Where(pu => pu.UserId == testUserId)
                    .ToListAsync();

                projectUsers.ShouldAllBe(pu => pu.Type == ProjectUserType.DeActive);
            });
        }

        [Fact]
        public async void DeactiveUser_Should_Throw_When_User_Not_Exist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _userAppService.DeactiveUser(new EntityDto<long>(9999));
            });
        }

        [Fact]
        public async void DeactiveUser_Should_Throw_When_PM_Is_Only_PM()
        {
            long testUserId = 0;
            long testProjectId = 0;

            await UsingDbContextAsync(async context =>
            {
                var user = new User
                {
                    UserName = "pm.only",
                    Name = "PM",
                    Surname = "Only",
                    EmailAddress = "pm@example.com",
                    IsActive = true
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                testUserId = user.Id;

                var project = new Project
                {
                    Name = "PM Project",
                    Status = ProjectStatus.Active 
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

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
            {
                await _userAppService.DeactiveUser(new EntityDto<long>(testUserId));
            });

            exception.Message.ShouldContain("Cannot deactivate the only PM");
        }
        [Fact]
        public async void DeactiveUser_Should_Allow_When_Project_InActive()
        {
            long testUserId = 0;

            await UsingDbContextAsync(async context =>
            {
                var user = new User
                {
                    UserName = "pm.inactive",
                    Name = "PM",
                    Surname = "Inactive",
                    EmailAddress = "pm.inactive@example.com",
                    IsActive = true
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
                testUserId = user.Id;

                var project = new Project
                {
                    Name = "Inactive Project",
                    Status = ProjectStatus.Deactive  
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();

                context.ProjectUsers.Add(new ProjectUser
                {
                    UserId = testUserId,
                    ProjectId = project.Id,
                    Type = ProjectUserType.PM
                });

                await context.SaveChangesAsync();
            });

            // Act - Should NOT throw
            await _userAppService.DeactiveUser(new EntityDto<long>(testUserId));

            // Assert
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
                user.IsActive.ShouldBeFalse();
            });
        }
        [Fact]
        public async void DeactiveUser_Should_Allow_When_Multiple_PMs()
        {
            long testUserId = 0;

            await UsingDbContextAsync(async context =>
            {
                var user1 = new User
                {
                    UserName = "pm1",
                    Name = "PM1",
                    Surname = "User",
                    EmailAddress = "pm1@example.com",
                    IsActive = true
                };
                var user2 = new User
                {
                    UserName = "pm2",
                    Name = "PM2",
                    Surname = "User",
                    EmailAddress = "pm2@example.com",
                    IsActive = true
                };
                context.Users.AddRange(user1, user2);
                await context.SaveChangesAsync();
                testUserId = user1.Id;

                var project = new Project
                {
                    Name = "Multi PM Project",
                    Status = ProjectStatus.Active
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();

                context.ProjectUsers.AddRange(
                    new ProjectUser
                    {
                        UserId = user1.Id,
                        ProjectId = project.Id,
                        Type = ProjectUserType.PM
                    },
                    new ProjectUser
                    {
                        UserId = user2.Id,
                        ProjectId = project.Id,
                        Type = ProjectUserType.PM
                    }
                );

                await context.SaveChangesAsync();
            });

            await _userAppService.DeactiveUser(new EntityDto<long>(testUserId));

            // Assert
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == testUserId);
                user.IsActive.ShouldBeFalse();
            });
        }

    }
}