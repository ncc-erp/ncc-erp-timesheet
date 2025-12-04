using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Ncc.Authorization;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using Ncc.Users;
using NSubstitute;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Timesheet.APIs.Public;
using Timesheet.DomainServices;
using Timesheet.Services.HRM;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Project;
using Timesheet.UploadFilesService;

namespace Ncc.Tests.Users
{
    public abstract class UserAppServiceTestBase : TimesheetTestBase
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

            var uploadAvatarService = Resolve<UploadAvatarService>();
            var projectService = Resolve<ProjectService>();
            var hrmService = Resolve<HRMService>();
            var hrmv2Service = Resolve<Timesheet.Services.HRMv2.IHRMv2Service>(); 
            var publicAppService = Resolve<PublicAppService>();

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
                hrmService,
                null, 
                publicAppService
            );

            userAppService.ObjectMapper = objectMapper;
            userAppService.UnitOfWorkManager = unitOfWorkManager;
            userAppService.AbpSession = abpSession;

            return userAppService;
        }
        protected T Resolve<T>()
        {
            return LocalIocManager.Resolve<T>();
        }
    }
}