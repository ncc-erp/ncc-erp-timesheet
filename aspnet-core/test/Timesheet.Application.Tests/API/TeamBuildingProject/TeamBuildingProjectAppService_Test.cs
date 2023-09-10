using Abp.Application.Services.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Office.Interop.Word;
using Ncc.Entities;
using Ncc.IoC;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.TeamBuildingDetails.Dto;
using Timesheet.APIs.TeamBuildingProject;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Services.Project;
using Xunit;
namespace Timesheet.Application.Tests.API.TeamBuildingProject
{
    public class TeamBuildingProjectAppService_Test : TimesheetApplicationTestBase
    {
        private readonly ProjectService _projectService;
        private readonly IWorkScope _workScope;
        private TeamBuildingProjectAppService _teamBuildingProjectAppService;
        public TeamBuildingProjectAppService_Test()
        {
            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
            configuration.GetValue<string>("ProjectService:BaseAddress").Returns("http://localhost/");
            configuration.GetValue<string>("ProjectService:SecurityCode").Returns("SecurityCode");

            var httpClient = Resolve<HttpClient>();
            var loggerProjectService = Resolve<ILogger<ProjectService>>();
            _projectService = new ProjectService(httpClient, configuration, loggerProjectService);
            _workScope = Resolve<IWorkScope>();
            _teamBuildingProjectAppService = new TeamBuildingProjectAppService(_projectService, _workScope);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetAllPagging_OK()
        {
            var input = new GridParam { };
           
            await WithUnitOfWorkAsync(async () =>
            {
               var result= await _teamBuildingProjectAppService.GetAllPagging(input);
                result.Items.Count.ShouldBe(7);
            });
         
        }
    }
}