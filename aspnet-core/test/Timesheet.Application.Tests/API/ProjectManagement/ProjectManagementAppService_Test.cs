using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.IoC;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Timesheet.APIs.ProjectManagement;
using Timesheet.APIs.ProjectManagement.Dto;
using Timesheet.DomainServices;
using Timesheet.Services.Komu;
using Timesheet.Timesheets.Customers.Dto;
using Xunit;

namespace Timesheet.Application.Tests.API.ProjectManagement
{
    /// <summary>
    /// 9/9 function
    /// 19/19 test cases passed
    /// update day 18/01/2023
    /// </summary>
    public class ProjectManagementAppService_Test : TimesheetApplicationTestBase
    {
        private readonly UserManager _userManager;
        private readonly ProjectManagementAppService _project;
        private readonly IWorkScope _work;
        public ProjectManagementAppService_Test()
        {
            _work = Resolve<IWorkScope>();
            var httpClient = Resolve<HttpClient>();
            var configuration = Substitute.For<IConfiguration>();
            var settingManager = Substitute.For<ISettingManager>();    
            configuration.GetValue<string>("HRMv2Service:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("HRMv2Service:SecurityCode").Returns("secretCode");
            var userManager = Resolve<UserManager>();
            var logger = Resolve<ILogger<KomuService>>();
            configuration.GetValue<string>("KomuService:BaseAddress").Returns("http://www.myserver.com");
            configuration.GetValue<string>("KomuService:SecurityCode").Returns("secretCode");
            configuration.GetValue<string>("KomuService:DevModeChannelId").Returns("_channelIdDevMode");
            configuration.GetValue<string>("KomuService:EnableKomuNotification").Returns("_isNotifyToKomu");
            var komuService = Substitute.For<KomuService>(httpClient, logger, configuration, settingManager);
            var abpSession = Resolve<IAbpSession>();
            var roleManager = Resolve<RoleManager>();
            var objectMapper = Resolve<IObjectMapper>();
            var userServices = Substitute.For<UserServices>(userManager, komuService, abpSession, roleManager, objectMapper, _work);
            /* == 1. ROLE MANAGER == */
            var _roleManager = Resolve<RoleManager>();
            /* == 2. USER MANAGER == */
            _userManager = Resolve<UserManager>();
            _project = new ProjectManagementAppService(userServices, _userManager, _roleManager, _work);
            _project.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
        }

        [Fact]
        public async void GetTotalWorkingTime()
        {
            var projectCode = "Project Training";
            var startDate = new DateTime(2020, 1, 1);
            var endDate = DateTime.Now;
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.GetTotalWorkingTime(projectCode, startDate, endDate);
                Assert.Equal(0, result.NormalWorkingMinute);
                Assert.Equal(0, result.OTMinute);
                Assert.Equal(0, result.OTNoChargeMinute);
            });
        }

        [Fact]
        public async void GetTimesheetByListProjectCode()
        {
            // Arrange
            var listProjectCode = new List<string>() { "Project Training", "Project Trudi" };
            var startDate = new DateTime(2022, 1, 1);
            var endDate = new DateTime(2022, 1, 31);

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.GetTimesheetByListProjectCode(listProjectCode, startDate, endDate);
                Assert.IsType<List<TotalWorkingTimeDto>>(result);
                Assert.Empty(result);
                //Assert.Contains(result, r => r.ProjectCode == "Project Training");
                //Assert.Contains(result, r => r.ProjectCode == "Project Trudi");
            });
        }

        [Fact]
        public async void CreateProject()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Name = "Project 9",
                Code = "Project 9",
                CustomerCode = "Client 1",
                ProjectType = 0,
                TimeStart = new DateTime(2022, 1, 1),
                TimeEnd = new DateTime(2022, 12, 31),
                EmailPM = "testemail4@gmail.com"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateProject(input);
                Assert.Null(result);
            });
        }

        [Fact]
        public async void CreateProject_ExistingName()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Name = "Project 1",
                Code = "Project 1",
                CustomerCode = "Client 1",
                ProjectType = 0,
                TimeStart = new DateTime(2022, 1, 1),
                TimeEnd = new DateTime(2022, 12, 31),
                EmailPM = "testemail4@gmail.com"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateProject(input);
                Assert.Equal("Fail! Project name <b>Project 1</b> already exist in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void CreateProject_ExistingCode()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Name = "Project 9",
                Code = "Project 1",
                CustomerCode = "Client 1",
                ProjectType = 0,
                TimeStart = new DateTime(2022, 1, 1),
                TimeEnd = new DateTime(2022, 12, 31),
                EmailPM = "testemail4@gmail.com"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateProject(input);
                Assert.Equal("Fail! Project code <b>Project 1</b> already exist in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void CreateProject_InvalidTime()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Name = "Project 9",
                Code = "Project 9",
                CustomerCode = "client 9",
                ProjectType = 0,
                TimeStart = new DateTime(2022, 1, 1),
                TimeEnd = new DateTime(2021, 12, 31),
                EmailPM = "testemail4@gmail.com"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateProject(input);
                Assert.Equal("Fail! Start time cannot be greater than end time in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void CreateProject_InvalidCustomerCode()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Name = "Project 9",
                Code = "Project 9",
                CustomerCode = "InvalidCode",
                ProjectType = 0,
                TimeStart = new DateTime(2022, 1, 1),
                TimeEnd = new DateTime(2022, 12, 31),
                EmailPM = "testemail4@gmail.com"
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateProject(input);
                input.CustomerCode = "InvalidCode";
                Assert.Equal("Fail! Not found Customer code <b>InvalidCode</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void CreateProject_InvalidPMEmail()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Name = "Project 9",
                Code = "Project 9",
                CustomerCode = "Client 2",
                ProjectType = 0,
                TimeStart = new DateTime(2022, 1, 1),
                TimeEnd = new DateTime(2022, 12, 31),
                EmailPM = "invalid@test.com"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateProject(input);
                Assert.Equal("Fail! Not found PM with email <b>invalid@test.com</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void ChangePmOfProject()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Code = "Project Training",
                EmailPM = "testemail4@gmail.com"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.ChangePmOfProject(input);
                Assert.Null(result);
            });
        }

        [Fact]
        public async void ChangePmOfProject_InvalidCode()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Code = "InvalidCode",
                EmailPM = "testemail4@gmail.com"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.ChangePmOfProject(input);
                Assert.Equal("Fail! Not found project code <b>InvalidCode</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void ChangePmOfProject_EmptyMail()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Code = "Project Training",
                EmailPM = ""
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.ChangePmOfProject(input);
                Assert.Equal("Fail! Email are not allowed to be empty <b>{0}</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void ChangePmOfProject_NonExistentPMEmail()
        {
            // Arrange
            var input = new SpecialProjectDto
            {
                Code = "Project Training",
                EmailPM = "invalid@test.com"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.ChangePmOfProject(input);
                Assert.Equal("Fail! Not found PM with email <b>invalid@test.com</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        // bug key = 1
        //[Fact]
        //public async void UserJoinProject()
        //{
        //    // Arrange
        //    var input = new UserJoinProjectDto
        //    {
        //        ProjectCode = "Project Training",
        //        EmailAddress = "testemail6@gmail.com",
        //        PMEmail = "testemail4@gmail.com",
        //        Role = 1,
        //        IsPool = false,
        //        StartDate = DateTime.Now
        //    };
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        var result = await _project.UserJoinProject(input);
        //        Assert.IsType<string>(result);
        //    });
        //}

        [Fact]
        public async void UserJoinProject_InvalidCode()
        {
            // Arrange
            var input = new UserJoinProjectDto
            {
                ProjectCode = "InvalidCode",
                EmailAddress = "testemail6@gmail.com",
                PMEmail = "testemail4@gmail.com",
                Role = 1,
                IsPool = false,
                StartDate = DateTime.Now
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.UserJoinProject(input);
                Assert.Equal("Fail! Not found project code <b>InvalidCode</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        [Fact]
        public async void UserJoinProject_InvalidEmail()
        {
            // Arrange
            var input = new UserJoinProjectDto
            {
                ProjectCode = "Project Training",
                EmailAddress = "invalid@test.com",
                PMEmail = "testemail4@gmail.com",
                Role = 1,
                IsPool = false,
                StartDate = DateTime.Now
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.UserJoinProject(input);
                Assert.Equal("Fail! Not found user with email <b>invalid@test.com</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        // bug key = 1
        //[Fact]
        //public async void UserJoinProject_InvalidPMEmail()
        //{
        //    // Arrange
        //    var input = new UserJoinProjectDto
        //    {
        //        ProjectCode = "Project Training",
        //        EmailAddress = "testemail6@gmail.com",
        //        PMEmail = "invalid@test.com",
        //        Role = 1,
        //        IsPool = false,
        //        StartDate = DateTime.Now
        //    };
        //    await WithUnitOfWorkAsync(async () =>
        //    {
        //        var result = await _project.UserJoinProject(input);
        //        Assert.Equal("Fail! Not found user with email <b>invalid@test.com</b> in <b>TIMESHEET TOOL</b>", result);
        //    });
        //}

        [Fact]
        public async void CloseProject()
        {
            var code = "Project Training";
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CloseProject(code);
                Assert.Null(result);
            });
        }

        [Fact]
        public async void CloseProject_InvalidCode()
        {
            var code = "InvalidCode";
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CloseProject(code);
                Assert.Equal("Fail! Not found project code <b>InvalidCode</b> in <b>TIMESHEET TOOL</b>", result);
            });
        }

        //Abp.ObjectMapping.IObjectMapper should be implemented
/*        [Fact]
        public async void CreateCustomer()
        {
            var input = new CustomerDto
            {
                Name = "Acme Corp",
                Code = "ACME",
                Address = "Corp"
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateCustomer(input);
                Assert.Equal("<p style='color:#28a745'>Create client name <b>Acme Corp</b> in <b>TIMESHEET TOOL</b> successful!</p>", result);
            });
        }*/

        [Fact]
        public async void CreateCustomer_ExistingName()
        {
            // Arrange
            var input = new CustomerDto
            {
                Name = "Client 1",
                Code = "Client 1",
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateCustomer(input);
                Assert.Equal("<p style='color:#dc3545'>Fail! Customer name <b>Client 1</b> already exist in <b>TIMESHEET TOOL</b></p>", result);
            });
        }

        [Fact]
        public async void CreateCustomer_ExistingCode()
        {
            // Arrange
            var input = new CustomerDto
            {
                Name = "Client 1",
                Code = "Client 1",
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _project.CreateCustomer(input);
                Assert.Equal("<p style='color:#dc3545'>Fail! Customer name <b>Client 1</b> already exist in <b>TIMESHEET TOOL</b></p>", result);
            });
        }

        [Fact]
        public async void GetRetroReviewInternHistories()
        {
            // Arrange
            var input = new InputRetroReviewInternHistoriesDto
            {
                Emails = new List<string> { "testemail9@gmail.com", "testemail17@gmail.com" },
                MaxCountHistory = 10
            };

            await WithUnitOfWorkAsync(async () =>
            {
                // Act
                var result = await _project.GetRetroReviewInternHistories(input);

                // Assert
                Assert.Equal(2, result.Count);
                Assert.Equal(3, result[0].PointHistories.Count);
                Assert.Equal("testemail9@gmail.com", result[0].Email);
                Assert.Equal(2, result[1].PointHistories.Count);
                Assert.Equal("testemail17@gmail.com", result[1].Email);
            });
        }
    }
}
