using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Timesheet.Services.Project.Dto;
using Timesheet.Services.Project;
using Timesheet.Tests;
using Xunit;
using NSubstitute;

namespace Timesheet.Application.Tests
{
    [Collection("Sequential")]
    public abstract class TimesheetApplicationTestBase : TesBase<TimesheetApplicationTestModule>
    {
        public readonly ProjectService _projectService;

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .Build();

        public TimesheetApplicationTestBase() {
            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
            configuration.GetValue<string>("ProjectService:BaseAddress").Returns("http://example.com/");
            configuration.GetValue<string>("ProjectService:SecurityCode").Returns("12345678");
            var httpClient = Resolve<HttpClient>();
            var loggerProjectService = Resolve<ILogger<ProjectService>>();
            var projectService = Substitute.For<ProjectService>(httpClient, configuration, loggerProjectService);

            List<GetProjectPMNameDto> listPms = new List<GetProjectPMNameDto>();
            string path1 = Path.Combine(Directory.GetCurrentDirectory().Replace("\\bin\\Debug\\netcoreapp3.1", ""), "JsonData", "TimekeepingsAppService_Tests", "get_list_PM_by_project_code_result.json").Replace("/bin/Debug/netcoreapp3.1", "");
            string getListPmResultStringJson = System.IO.File.ReadAllText(path1);
            listPms = JsonConvert.DeserializeObject<List<GetProjectPMNameDto>>(getListPmResultStringJson);
            projectService.GetProjectPMName().Returns(listPms);

            _projectService = projectService;
        }
    }
}
