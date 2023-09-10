using Abp.Configuration;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Configuration;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.APIs.HRM;
using Timesheet.APIs.TeamBuildingDetails;
using Timesheet.APIs.TeamBuildingDetails.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Services.Project;
using Xunit;

namespace Timesheet.Application.Tests.API.TeamBuildingDetailsHR
{
    public class TeamBuildingDetailsHRAppService_Test : TimesheetApplicationTestBase
    {
        private readonly IWorkScope _workScope;
        private readonly TeamBuildingDetailsAppService teamBuildingDetailsAppService;
        public static ProjectService _projectService;
        public static HRMAppService _hRMAppService;
        private readonly ILogger<TeamBuildingDetailsAppService> _logger;
        private readonly GenerateDataTeamBuildingServices _generateDataTeamBuildingServices;

        public TeamBuildingDetailsHRAppService_Test()
        {
            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
            configuration.GetValue<string>("ProjectService:BaseAddress").Returns("http://localhost/");
            configuration.GetValue<string>("ProjectService:SecurityCode").Returns("SecurityCode");
            var httpClient = Resolve<HttpClient>();
            var loggerProjectService = Resolve<ILogger<ProjectService>>();
            _projectService = new ProjectService(httpClient, configuration, loggerProjectService);

            

            var loggerTeamBuildingDetailsAppService = Resolve<ILogger<TeamBuildingDetailsAppService>>();

            _workScope = Resolve<IWorkScope>();

            var settingManager = Substitute.For<ISettingManager>();
            settingManager.GetSettingValueForApplicationAsync(AppSettingNames.TeamBuildingMoney).Returns(Task.FromResult("100000"));

            var loggerGenerateDataTeamBuildingServices = Resolve<ILogger<GenerateDataTeamBuildingServices>>();
            _generateDataTeamBuildingServices = new GenerateDataTeamBuildingServices(_projectService, loggerGenerateDataTeamBuildingServices, _workScope);
            _generateDataTeamBuildingServices.SettingManager = settingManager;

            teamBuildingDetailsAppService = new TeamBuildingDetailsAppService(_projectService, loggerTeamBuildingDetailsAppService, _generateDataTeamBuildingServices, _workScope);
            teamBuildingDetailsAppService.UnitOfWorkManager = Resolve<IUnitOfWorkManager>();
            teamBuildingDetailsAppService.AbpSession = AbpSession;
            teamBuildingDetailsAppService.ObjectMapper = Resolve<IObjectMapper>();
            teamBuildingDetailsAppService.SettingManager = settingManager;
        }
        [Fact]
        public async Task AddDataToTeamBuildingDetailByMonth_Should_Add()
        {
            var date = new InputGenerateDataTeamBuildingDto
            {
                Month = 01,
                Year = 2023
            };

            var expectId = 6;

            WithUnitOfWork( () =>
            {
                teamBuildingDetailsAppService.AddDataToTeamBuildingDetail(date);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var addedTeamBuilding = await _workScope.GetAsync<TeamBuildingDetail>(expectId);

                addedTeamBuilding.Id.ShouldBe(expectId);
                addedTeamBuilding.EmployeeId.ShouldBe(1);
                addedTeamBuilding.ProjectId.ShouldBe(1);
                addedTeamBuilding.Money.ShouldBe(20000);
                addedTeamBuilding.Status.ShouldBe(Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Open);
            });
        }

        [Fact]
        public async Task AddDataToTeamBuildingDetailByMonth_Should_Not_Add_Greater_Month_Than_Current()
        {
            var date = new InputGenerateDataTeamBuildingDto
            {
                Month = 12,
                Year = 2024
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    teamBuildingDetailsAppService.AddDataToTeamBuildingDetail(date);
                });
                Assert.Equal("The selected month cannot greater than the current month!", exception.Message);
            });
        }

        [Fact]
        public async Task AddDataToTeamBuildingDetailByMonth_Should_Not_Add_Empty_Value()
        {
            var date = new InputGenerateDataTeamBuildingDto
            {
                Month = default,
                Year = default
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    teamBuildingDetailsAppService.AddDataToTeamBuildingDetail(date);
                });
                Assert.Equal("Selected month or year is empty!", exception.Message);
            });
        }

        [Fact]
        public async Task GetAllPagging_Test()
        {
            var expectTotalCount = 25;
            var expectItemCount = 10;

            var gridParam = new GridParam
            {
                MaxResultCount = 10,
            };

            var date = new InputFilterTeamBuildingDetailPagingDto
            {
                GridParam = gridParam,
                Year = 2022,
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await teamBuildingDetailsAppService.GetAllPagging(date);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.First().EmployeeId.ShouldBe(1);
                result.Items.First().Money.ShouldBe(50000);
                result.Items.First().ProjectId.ShouldBe(1);
            });
        }

        [Fact]
        public async Task GetAllPagging_Test_Filter_By_Status()
        {
            var expectTotalCount = 5;
            var expectItemCount = 3;

            var gridParam = new GridParam
            {
                MaxResultCount = 3,
            };

            var date = new InputFilterTeamBuildingDetailPagingDto
            {
                GridParam = gridParam,
                Year = 2022,
                Status = Ncc.Entities.Enum.StatusEnum.TeamBuildingStatus.Done
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await teamBuildingDetailsAppService.GetAllPagging(date);
                Assert.Equal(expectTotalCount, result.TotalCount);
                Assert.Equal(expectItemCount, result.Items.Count);

                result.Items.First().EmployeeId.ShouldBe(15);
                result.Items.First().Money.ShouldBe(20000);
                result.Items.First().ProjectId.ShouldBe(2);

            });
        }
        [Fact]
        public async Task AddNew_Test()
        {

            var teambuildingDetail = new CreateTeamBuildingDetailDto
            {
                 EmployeeId = 1,
                 Month = 6,
                 Year = 2022
            };
            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await teamBuildingDetailsAppService.AddNew(teambuildingDetail);
                });
                Assert.Equal("User does not have working time this month!", exception.Message);
            });
        }

        [Fact]
        public async Task AddNew_But_Not_Found_EmployeeId()
        {
            var teambuildingDetail = new CreateTeamBuildingDetailDto
            {
                EmployeeId = default,
                Month = 1,
                Year = 2022
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await teamBuildingDetailsAppService.AddNew(teambuildingDetail);
                });
                Assert.Equal("Not found employee by employee id", exception.Message);
            });
        }

        [Fact]
        public async Task AddNew_But_Employee_Selected_Have_Current_Level_Less_Intern2()
        {
            var teambuildingDetail = new CreateTeamBuildingDetailDto
            {
                EmployeeId = 8,
                Month = 1,
                Year = 2022
            };

            await WithUnitOfWorkAsync(async () =>
            {
                var exception = await Assert.ThrowsAsync<UserFriendlyException>(async () =>
                {
                    await teamBuildingDetailsAppService.AddNew(teambuildingDetail);
                });
                Assert.Equal("Cannot add employee because the employee's level is less than intern 2", exception.Message);
            });
        }

        [Fact]
        public async Task GetAllEmployeeTeamBuilding()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await teamBuildingDetailsAppService.GetAllEmployeeTeamBuilding();

                result.First().Id.ShouldBe(3);
                result.First().FullName.ShouldBe("Tiến Nguyễn Hữu");
            });
        }

        [Fact]
        public async Task GetAllProjectTeamBuilding()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await teamBuildingDetailsAppService.GetAllProjectTeamBuilding();

                result.First().Id.ShouldBe(4);
                result.First().Name.ShouldBe("Project Product");
            });
        }
    }
}
