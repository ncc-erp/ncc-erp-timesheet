using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Timesheet.DomainServices;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Timesheets.Projects;
using Timesheet.Timesheets.Projects.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Application.Tests.API.TimeSheets.Projects
{
    public class ProjectAppServiceTestBase : TimesheetApplicationTestBase
    {
        public ProjectAppService InstanceProjectAppService() 
        {
            var workScope = Resolve<IWorkScope>();
            var userManager = Resolve<UserManager>();
            var abpSession = Resolve<IAbpSession>();
            var objectMapper = Resolve<IObjectMapper>();
            var unitOfManager = Resolve<IUnitOfWorkManager>();

            var projectServiceConfigOptions = new Dictionary<string, string>
                {
                    {"ProjectService:BaseAddress", "http://www.myserver.com"},
                    {"ProjectService:SecurityCode", "SecurityCode"},
                };

            var projectServiceConfiguration = new ConfigurationBuilder()
              .AddInMemoryCollection(projectServiceConfigOptions)
              .Build();

            var komuServiceConfigOptions = new Dictionary<string, string>
                {
                    {"KomuService:BaseAddress", "http://www.myserver.com"},
                    {"KomuService:SecurityCode", "SecurityCode"},
                    {"KomuService:DevModeChannelId", ""},
                    {"KomuService:EnableKomuNotification", "true"},
                };

            var komuServiceConfiguration = new ConfigurationBuilder()
              .AddInMemoryCollection(komuServiceConfigOptions)
              .Build();
            var httpClient = Resolve<HttpClient>();
            var projectServiceLogger = Resolve<ILogger<ProjectService>>();
            var komuServiceLogger = Resolve<ILogger<KomuService>>();
            var projectService = Substitute.For<ProjectService>(
                httpClient, 
                projectServiceConfiguration, 
                projectServiceLogger);

            var settingManager = Substitute.For<ISettingManager>();    
            var komuSerive = Substitute.For<KomuService>(
                httpClient,
                komuServiceLogger,
                komuServiceConfiguration,
                settingManager);
            var roleRepository = Resolve<IRepository<Role>>();
            var rolePermissionSettingRepository = Resolve<IRepository<RolePermissionSetting, long>>();
            var roleValidators = Substitute.For<IEnumerable<IRoleValidator<Role>>>();
            var lookupNormalizer = Resolve<ILookupNormalizer>();
            var roleLogger = Resolve<ILogger<AbpRoleManager<Role, User>>>();
            var permissionManager = Resolve<IPermissionManager>();
            var cacheManager = Resolve<ICacheManager>();
            var roleManagementConfig = Resolve<IRoleManagementConfig>();
            var error = Substitute.For<IdentityErrorDescriber>();

            var roleStore = Substitute.For<RoleStore>(
                unitOfManager,
                roleRepository,
                rolePermissionSettingRepository);

            var mockRoleManager = Substitute.For<RoleManager>(
                roleStore,
                roleValidators,
                lookupNormalizer,
                error,
                roleLogger,
                permissionManager,
                cacheManager,
                unitOfManager,
                roleManagementConfig);

            var userService = Substitute.For<UserServices>(
                userManager,
                komuSerive,
                abpSession,
                mockRoleManager,
                objectMapper,
                workScope);

            var projectAppService = new ProjectAppService(
                userService, 
                projectService, 
                userManager, 
                workScope);
            projectAppService.ObjectMapper= objectMapper;
            projectAppService.UnitOfWorkManager= unitOfManager;
            projectAppService.AbpSession= abpSession;

            return projectAppService;
        }

        public ProjectTaskDto ProjectTaskDto()
        {
            return new ProjectTaskDto
            {
                TaskId = 1,
                Billable = true,
            };
        }

        public ProjectUsersDto ProjectUsersDto()
        {
            return new ProjectUsersDto
            {
                UserId = 4,
                Type = ProjectUserType.Member,
                IsTemp = false
            };
        }
        
        public ProjectUsersDto ProjectUsersDtoPM()
        {
            return new ProjectUsersDto
            {
                UserId = 3,
                Type = ProjectUserType.PM,
                IsTemp = false
            };
        }
        
        public ProjectTargetUserDto ProjectTargetUserDto()
        {
            return new ProjectTargetUserDto
            {
                UserId = 4,
                RoleName = "Leader",
            };
        }

        public ProjectDto ProjectDto()
        {
            var projectTaskDto = ProjectTaskDto();
            var projectUserDto = ProjectUsersDto();
            var projectUserDtoPM = ProjectUsersDtoPM();
            var projectTargetUserDto = ProjectTargetUserDto();
            var listProjectTaskDto = new List<ProjectTaskDto> { projectTaskDto };
            var listProjectUsersDto = new List<ProjectUsersDto> { projectUserDto, projectUserDtoPM };
            var listProjectTargetUserDto = new List<ProjectTargetUserDto> { projectTargetUserDto };

            return new ProjectDto
            {
                Name = "Project Unit Test",
                Code = "Unit Test",
                Status = ProjectStatus.Active,
                TimeStart = new DateTime(2020, 1, 1),
                TimeEnd = new DateTime(2024, 1, 1),
                Note = "Test",
                ProjectType = ProjectType.TimeAndMaterials,
                CustomerId = 1,
                Tasks = listProjectTaskDto,
                Users = listProjectUsersDto,
                ProjectTargetUsers = listProjectTargetUserDto,
                IsNotifyToKomu = false,
                IsNoticeKMSubmitTS = false,
                IsNoticeKMRequestOffDate = false,
                IsNoticeKMApproveRequestOffDate = false,
                IsNoticeKMRequestChangeWorkingTime = false,
                IsNoticeKMApproveChangeWorkingTime = false,
                isAllUserBelongTo = false,
            };
        }
    }
}
