using Abp;
using Abp.UI;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.Timesheets.Projects;
using Timesheet.Timesheets.Projects.Dto;
using Xunit;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.TimeSheets.Projects
{
    // 13 function, 21 test case

    public class ProjectAppServiceTest : ProjectAppServiceTestBase
    {
        private readonly ProjectAppService _projectAppService;

        public ProjectAppServiceTest()
        {
            _projectAppService = InstanceProjectAppService();
        }

        [Fact]
        public async void CanNotInsertProjectWithExistName()
        {
            var expectedMessage = "Project name:Project 1 is exists";
            var projectDto = ProjectDto();
            projectDto.Name = "Project 1";

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _projectAppService.Save(projectDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotInsertProjectWithExistCode()
        {
            var expectedMessage = "Project code:Project 1 is exists";
            var projectDto = ProjectDto();
            projectDto.Code = "Project 1";

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _projectAppService.Save(projectDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotInsertProjectWithoutPM()
        {
            var expectedMessage = "Project must have at least one project manager";
            var projectDto = ProjectDto();
            var projectUserDto = ProjectUsersDto();
            projectDto.Users = new List<ProjectUsersDto> { projectUserDto };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<Exception>(async () =>
                {
                    await _projectAppService.Save(projectDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void CanNotInsertProjectWithInvalidTime()
        {
            var expectedMessage = "Start time cannot be greater than end time !";
            var projectUsersDto = ProjectUsersDto();
            projectUsersDto.Type = ProjectUserType.PM;
            var projectDto = ProjectDto();
            var listUser = new List<ProjectUsersDto> { projectUsersDto };
            projectDto.Users = listUser;
            projectDto.TimeEnd = new DateTime(2019, 1, 1);

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _projectAppService.Save(projectDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void Insert()
        {
            var projectDto = ProjectDto();
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.Save(projectDto);
                Assert.Equal(1, result.CustomerId);
                Assert.Equal(8, result.Id);
                Assert.Equal("Unit Test", result.Code);
                Assert.Equal("Project Unit Test", result.Name);
                Assert.Equal("Test", result.Note);
                Assert.Equal(ProjectType.TimeAndMaterials, result.ProjectType);
                Assert.Equal(ProjectStatus.Active, result.Status);
                Assert.Equal(2, result.Users.Count);
            });
        }

        [Fact]
        public async void CanNotUpdate()
        {
            var projectDto = ProjectDto();
            projectDto.Id = 1;
            projectDto.Name = "Project UCG";
            var expectedMessage = "Tasks are logged timesheet so you can't remove";

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _projectAppService.Save(projectDto);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void GetAllAllowStatus()
        {
            var expectedProjectStatus = ProjectStatus.Active;
            var expectedCount = 6;
            var expectedName = "Project 1";
            var expectedCode = "Project 1";
            var expectedCustomerName = "client 1";
            var expectedProjectType = ProjectType.TimeAndMaterials; ;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetAll(expectedProjectStatus, null);

                Assert.Equal(expectedCount, result.Count);
                Assert.True(result.All(project => project.Status == expectedProjectStatus));
                result.ShouldContain(project => project.Name == expectedName);
                result.ShouldContain(project => project.Code == expectedCode);
                result.ShouldContain(project => project.CustomerName == expectedCustomerName);
                result.ShouldContain(project => project.ProjectType == expectedProjectType);
            });
        }

        [Fact]
        public async void GetAllAllowSearchTerm()
        {
            var searchTerm = "1";
            var expectedCount = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetAll(null, searchTerm);

                Assert.Equal(expectedCount, result.Count);
                Assert.True(result.All(project => project.CustomerName.Contains(searchTerm)));
            });
        }

        [Fact]
        public async void GetAllAllowSearchTermAndStatus()
        {
            var searchTerm = "1";
            var expectedStatus = ProjectStatus.Active;
            var expectedCount = 1;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetAll(expectedStatus, searchTerm);

                Assert.Equal(expectedCount, result.Count);
                Assert.True(result.All(project => project.CustomerName.Contains(searchTerm)));
                Assert.True(result.All(project => project.Status == expectedStatus));
            });
        }

        [Fact]
        public async void CanNotGetWithInvalidId()
        {
            var id = 100;
            var expectedMessage = "Project isn't exist";

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await _projectAppService.Get(id);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public async void GetById()
        {
            var id = 1;
            var expectedCode = "Project 1";
            var expectedName = "Project 1";
            var expectedCustomerId = 1;
            var expectedProjectType = ProjectType.TimeAndMaterials;
            var expectedStatus = ProjectStatus.Active;
            var expectedTaskCount = 2;
            var expectedUserCount = 7;
            var expectedIsAllUsersBelong = false;
            var expectedIsNoticeKMApproveChangeWorkingTime = false;
            var expectedIsNoticeKMApproveRequestOffDate = false;
            var expectedIsNoticeKMRequestChangeWorkingTime = false;
            var expectedIsNoticeKMRequestOffDate = false;
            var expectedIsNoticeKMSubmitTimeSheet = false;
            var expectedIsNotifyToKomu = false;

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.Get(id);

                Assert.Equal(id, result.Id);
                Assert.Equal(expectedName, result.Name);
                Assert.Equal(expectedCode, result.Code);
                Assert.Equal(expectedCustomerId, result.CustomerId);
                Assert.Equal(expectedTaskCount, result.Tasks.Count);
                Assert.Equal(expectedUserCount, result.Users.Count);
                Assert.Equal(expectedProjectType, result.ProjectType);
                Assert.Equal(expectedStatus, result.Status);
                Assert.Equal(expectedIsAllUsersBelong, result.isAllUserBelongTo);
                Assert.Equal(expectedIsNoticeKMApproveChangeWorkingTime, result.IsNoticeKMRequestChangeWorkingTime);
                Assert.Equal(expectedIsNoticeKMApproveRequestOffDate, result.IsNoticeKMApproveRequestOffDate);
                Assert.Equal(expectedIsNoticeKMRequestChangeWorkingTime, result.IsNoticeKMRequestChangeWorkingTime);
                Assert.Equal(expectedIsNoticeKMRequestOffDate, result.IsNoticeKMRequestOffDate);
                Assert.Equal(expectedIsNotifyToKomu, result.IsNotifyToKomu);
                Assert.Equal(expectedIsNoticeKMSubmitTimeSheet, result.IsNoticeKMSubmitTS);
            });
        }

        [Fact]
        public async void ClearDefaultProjectTask()
        {
            var workScope = Resolve<IWorkScope>();


            await WithUnitOfWorkAsync(async () =>
            {
                await _projectAppService.ClearDefaultProjectTask();
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var id = AbpSession.UserId.Value;
                var userAfterAction = await workScope.GetAsync<User>(id);

                Assert.Null(userAfterAction.DefaultProjectTaskId);
            });
        }

        [Fact]
        public async void GetProjectsIncludingTasks()
        {
            var expectedCount = 3;
            var expectedName = "Project 1";
            var expectedCode = "Project 1";
            var expectedCustomerName = "Client 1";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetProjectsIncludingTasks();

                Assert.True(result.All(project => project.Tasks.Count > 0));
                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(project => project.ProjectName == expectedName);
                result.ShouldContain(project => project.ProjectCode == expectedCode);
                result.ShouldContain(project => project.CustomerName == expectedCustomerName);
            });
        }

        [Fact]
        public async void GetFilter()
        {
            var expectedCount = 7;
            var expectedName = "Project 1";
            var expectedCode = "Project 1";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetFilter();

                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(project => project.Name == expectedName);
                result.ShouldContain(project => project.Code == expectedCode);
            });
        }

        [Fact]
        public async void GetProjectPM()
        {
            var expectedCount = 3;
            var expectedName = "Project 1";
            var expectedCode = "Client 1";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetProjectPM();

                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(project => project.Name == expectedName);
                result.ShouldContain(project => project.Code == expectedCode);
            });
        }

        [Fact]
        public async void GetProjectWorkingTimePM()
        {
            var expectedCount = 8;
            var expectedName = "Project 1";
            var expectedCode = "Client 1";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetProjectWorkingTimePM();

                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(project => project.Name == expectedName);
                result.ShouldContain(project => project.Code == expectedCode);
            });
        }

        [Fact]
        public async void GetProjectUser()
        {
            var expectedCount = 3;
            var expectedName = "Project 1";
            var expectedCode = "Client 1";

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _projectAppService.GetProjectUser();

                Assert.Equal(expectedCount, result.Count);
                result.ShouldContain(project => project.Name == expectedName);
                result.ShouldContain(project => project.Code == expectedCode);
            });
        }

        [Fact]
        public void CanNotProcessTempTimesheetWithInvalidProjectCode()
        {
            var expectedCode = "UCG";
            var expectedMessage = "Project not exist !";

            WithUnitOfWork(() =>
            {
                var exception = Assert.Throws<UserFriendlyException>(() =>
                {
                    _projectAppService.ProcessTempTimesheet(expectedCode);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public void CanNotProcessTempTimesheetWithEmptyListTempUser()
        {
            var expectedCode = "Project 1";
            var expectedMessage = "listTempUsersInOut null";

            WithUnitOfWork(() =>
            {
                var exception = Assert.Throws<UserFriendlyException>(() =>
                {
                    _projectAppService.ProcessTempTimesheet(expectedCode);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public void CanNotProcessCurrentTempProjectUserWithInvalidProjectCode()
        {
            var expectedCode = "UCG";
            var expectedMessage = "Project not exist !";

            WithUnitOfWork(() =>
            {
                var exception = Assert.Throws<UserFriendlyException>(() =>
                {
                    _projectAppService.ProcessCurrentTempProjectUser(expectedCode);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }

        [Fact]
        public void CanNotProcessCurrentTempProjectUserWithEmptyListEmail()
        {
            var expectedCode = "Project 1";
            var expectedMessage = "listEmail null";

            WithUnitOfWork(() =>
            {
                var exception = Assert.Throws<UserFriendlyException>(() =>
                {
                    _projectAppService.ProcessCurrentTempProjectUser(expectedCode);
                });

                Assert.Equal(expectedMessage, exception.Message);
            });
        }
        [Fact]
        public async void Should_Throw_Exception_When_User_Not_In_Project()
        {
            long projectId = 20072;
            long nonExistUserId = 9999;

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    await _projectAppService.ReleaseUserFromProject(projectId, nonExistUserId));

                exception.Message.ShouldBe($"User with id {nonExistUserId} is not in the project with Id {projectId}");
            });
        }

        [Fact]
        public async void Should_Throw_Exception_When_Try_To_Deactivate_The_Only_PM()
        {
            long projectId = 0;
            long onlyPmUserId = 0;
            await WithUnitOfWorkAsync(async () =>
            {
                var workScope = Resolve<IWorkScope>();
                var project = new Project
                {
                    Name = "Test Project Only PM",
                    Code = "TEST_ONLY_PM",
                    CustomerId = 1,
                    Status = ProjectStatus.Active,
                    ProjectType = ProjectType.TimeAndMaterials,
                    TimeStart = DateTime.Now,
                    TimeEnd = DateTime.Now.AddMonths(6)
                };

                projectId = await workScope.InsertAndGetIdAsync(project);
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = 1,
                    Type = ProjectUserType.PM
                };

                await workScope.InsertAsync(projectUser);
                onlyPmUserId = 1;
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                    await _projectAppService.ReleaseUserFromProject(projectId, onlyPmUserId));

                exception.Message.ShouldBe("Cannot deactivate the only PM in the project. Please assign another PM first.");
            });
        }
        [Fact]
        public async void Should_Deactivate_Normal_Member_Successfully()
        {
            long projectId = 0;
            long pmUserId = 0;
            long normalUserId = 0;
            await WithUnitOfWorkAsync(async () =>
            {
                var workScope = Resolve<IWorkScope>();

                var project = new Project
                {
                    Name = "Test Project Normal Member",
                    Code = "TEST_NORMAL_MEMBER",
                    CustomerId = 1,
                    Status = ProjectStatus.Active,
                    ProjectType = ProjectType.TimeAndMaterials,
                    TimeStart = DateTime.Now,
                    TimeEnd = DateTime.Now.AddMonths(6)
                };

                projectId = await workScope.InsertAndGetIdAsync(project);

                var pmUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = 1,
                    Type = ProjectUserType.PM
                };
                await workScope.InsertAsync(pmUser);
                pmUserId = 1;

                var normalMember = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = 2,
                    Type = ProjectUserType.Member
                };
                await workScope.InsertAsync(normalMember);
                normalUserId = 2;
            });

            await WithUnitOfWorkAsync(async () =>
            {
                await _projectAppService.ReleaseUserFromProject(projectId, normalUserId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                UsingDbContext(context =>
                {
                    var projectUser = context.ProjectUsers
                        .FirstOrDefault(pu => pu.ProjectId == projectId && pu.UserId == normalUserId);

                    projectUser.ShouldNotBeNull();
                    projectUser.Type.ShouldBe(ProjectUserType.DeActive);
                });
            });
        }

        [Fact]
        public async void Should_Deactivate_One_Of_Many_PMs_Successfully()
        {
            long projectId = 0;
            long pm1UserId = 0;
            long pm2UserId = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var workScope = Resolve<IWorkScope>();

                var project = new Project
                {
                    Name = "Test Project Many PMs",
                    Code = "TEST_MANY_PMS",
                    CustomerId = 1,
                    Status = ProjectStatus.Active,
                    ProjectType = ProjectType.TimeAndMaterials,
                    TimeStart = DateTime.Now,
                    TimeEnd = DateTime.Now.AddMonths(6)
                };

                projectId = await workScope.InsertAndGetIdAsync(project);

                var pm1 = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = 1,
                    Type = ProjectUserType.PM
                };
                await workScope.InsertAsync(pm1);
                pm1UserId = 1;

                var pm2 = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = 2,
                    Type = ProjectUserType.PM
                };
                await workScope.InsertAsync(pm2);
                pm2UserId = 2;
            });

            await WithUnitOfWorkAsync(async () =>
            {
                await _projectAppService.ReleaseUserFromProject(projectId, pm1UserId);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                UsingDbContext(context =>
                {
                    var projectUser = context.ProjectUsers
                        .FirstOrDefault(pu => pu.ProjectId == projectId && pu.UserId == pm1UserId);

                    projectUser.ShouldNotBeNull();
                    projectUser.Type.ShouldBe(ProjectUserType.DeActive);
                    var remainingPMs = context.ProjectUsers
                        .Count(pu => pu.ProjectId == projectId && pu.Type == ProjectUserType.PM);

                    remainingPMs.ShouldBeGreaterThan(0);
                });
            });
        }

    }
}
