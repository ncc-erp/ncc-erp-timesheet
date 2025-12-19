using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using Ncc.Users;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.APIs.Public;
using Timesheet.Application.Tests;
using Timesheet.DomainServices;
using Timesheet.Services.HRM;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Project;
using Timesheet.UploadFilesService;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.Application.Tests.API.Users
{
    public abstract class UserAppServiceTestBase : TimesheetApplicationTestBase
    {
        protected UserAppService InstanceUserAppService()
        {
            var workScope = Resolve<IWorkScope>();
            var repository = Resolve<IRepository<User, long>>();
            var userManager = Resolve<UserManager>();
            var roleManager = Resolve<RoleManager>();
            var roleRepository = Resolve<IRepository<Role>>();
            var passwordHasher = Resolve<IPasswordHasher<User>>();
            var abpSession = Resolve<IAbpSession>();
            var objectMapper = Resolve<IObjectMapper>();
            var unitOfWorkManager = Resolve<IUnitOfWorkManager>();

            var uploadFileService = Substitute.For<IUploadFileService>();

            if (!LocalIocManager.IsRegistered<IUploadFileService>())
            {
                LocalIocManager.IocContainer.Register(
                    Component.For<IUploadFileService>()
                        .Instance(uploadFileService)
                        .LifestyleSingleton()
                );
            }
            var uploadAvatarService = Resolve<UploadAvatarService>();
            var projectServiceConfigOptions = new Dictionary<string, string>
                {
                    {"ProjectService:BaseAddress", "http://www.myserver.com"},
                    {"ProjectService:SecurityCode", "SecurityCode"},
                };
            var projectServiceConfiguration = new ConfigurationBuilder()
              .AddInMemoryCollection(projectServiceConfigOptions)
              .Build();

            var httpClient = Resolve<HttpClient>();
            var projectServiceLogger = Resolve<ILogger<ProjectService>>();
            var projectService = Substitute.For<ProjectService>(
                httpClient,
                projectServiceConfiguration,
                projectServiceLogger);

            var emailSender = Substitute.For<IEmailSender>();
            var backgroundJobManager = Substitute.For<IBackgroundJobManager>();
            var hostingEnvironment = Substitute.For<IHostingEnvironment>();
            var userServices = Substitute.For<IUserServices>();
            var userAppService = new UserAppService(
                repository,
                userManager,
                roleManager,
                roleRepository,
                passwordHasher,
                abpSession,
                null,
                emailSender,
                workScope,
                backgroundJobManager,
                hostingEnvironment,
                userServices,
                uploadAvatarService,
                projectService,
                null,
                null,
                null
            );
            userAppService.ObjectMapper = objectMapper;
            userAppService.UnitOfWorkManager = unitOfWorkManager;
            userAppService.AbpSession = abpSession;
            return userAppService;
        }
        public async Task<long> CreateUser(string userName, bool isActive = true)
        {
            long userId = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var workScope = Resolve<IWorkScope>();
                var user = new User
                {
                    UserName = userName,
                    EmailAddress = userName + "@ncc.asia",
                    IsActive = isActive,
                    Name = "Test",
                    Surname = "User",
                    Password = "DefaultPassword123"
                };

                userId = await workScope.InsertAndGetIdAsync(user);
            });

            return userId;
        }

        public async Task<long> SetupUserAsOnlyActivePM(long userId, string projectName)
        {
            long projectId = 0;

            await WithUnitOfWorkAsync(async () =>
            {
                var workScope = Resolve<IWorkScope>();

                var id = await workScope.InsertAndGetIdAsync(new Project
                {
                    Name = projectName,
                    Code = Guid.NewGuid().ToString().Substring(0, 8),
                    Status = ProjectStatus.Active,
                    CustomerId = 1,
                    TimeStart = DateTime.Now
                });

                await workScope.InsertAsync(new ProjectUser
                {
                    ProjectId = id,
                    UserId = userId,
                    Type = ProjectUserType.PM
                });

                projectId = id;
            });

            return projectId;
        }

        public async System.Threading.Tasks.Task AddAnotherActivePMToProject(long projectId, string otherPmName)
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var workScope = Resolve<IWorkScope>();

                var otherUserId = await workScope.InsertAndGetIdAsync(new User
                {
                    UserName = otherPmName,
                    EmailAddress = otherPmName + "@ncc.asia",
                    IsActive = true,
                    Name = "Other",
                    Surname = "PM"
                });

                await workScope.InsertAsync(new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = otherUserId,
                    Type = ProjectUserType.PM
                });
            });
        }
    }
}