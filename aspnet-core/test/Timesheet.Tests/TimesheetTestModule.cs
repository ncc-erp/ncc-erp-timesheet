using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ncc.Authorization;
using Ncc.Configuration;
using Ncc.EntityFrameworkCore;
using Ncc.IoC;
using Ncc.MultiTenancy;
using Ncc.Tests.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Timesheet.APIs.Public;
using Timesheet.DomainServices;
using Timesheet.Services.HRM;
using Timesheet.Services.Project;
using Timesheet.UploadFilesService;

namespace Ncc.Tests
{
    public class FakeMezonService : Timesheet.Services.Mezon.MezonService
    {
        public FakeMezonService(
            HttpClient httpClient,
            ISettingManager settingManager,
            ILogger<Timesheet.Services.Mezon.MezonService> logger,
            IConfiguration configuration,
            IWorkScope workScope)
            : base(httpClient, settingManager, logger, configuration, workScope)
        {
        }
    }

    [DependsOn(
        typeof(TimesheetApplicationModule),
        typeof(TimesheetEntityFrameworkModule),
        typeof(AbpTestBaseModule)
        )]
    public class TimesheetTestModule : AbpModule
    {
        public TimesheetTestModule(TimesheetEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
            abpProjectNameEntityFrameworkModule.SkipDbSeed = true;
        }

        public override void PreInitialize()
        {
            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
            Configuration.UnitOfWork.IsTransactional = false;
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            RegisterFakeService<AbpZeroDbMigrator<TimesheetDbContext>>();
            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
            Configuration.Settings.Providers.Add<AppSettingProvider>();

            RegisterFakeConfiguration();

            Configuration.ReplaceService(
                typeof(Microsoft.AspNetCore.Hosting.IHostingEnvironment),
                () => {
                    IocManager.IocContainer.Register(
                        Component.For<Microsoft.AspNetCore.Hosting.IHostingEnvironment>()
                            .Instance(CreateFakeHostingEnvironment())
                            .IsDefault()
                    );
                }
            );

            IocManager.IocContainer.Register(
                Component.For<HttpClient>()
                    .UsingFactoryMethod(() => new HttpClient())
                    .LifestyleTransient()
            );

            IocManager.IocContainer.Register(
                Component.For<ILogger<Timesheet.Services.Mezon.MezonService>>()
                    .Instance(NullLogger<Timesheet.Services.Mezon.MezonService>.Instance)
            );

            IocManager.IocContainer.Register(
                Component.For<Timesheet.Services.Mezon.MezonService>()
                    .ImplementedBy<FakeMezonService>()
                    .LifestyleSingleton()
            );

            IocManager.IocContainer.Register(
                Component.For<LogInManager>()
                    .LifestyleTransient()
            );

            RegisterFakeService<Timesheet.Services.Komu.KomuService>();

            IocManager.IocContainer.Register(
                Component.For<Ncc.Users.UserAppService>()
                    .LifestyleTransient()
            );
        }

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment CreateFakeHostingEnvironment()
        {
            var mock = Substitute.For<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
            mock.EnvironmentName.Returns("Test");
            mock.ApplicationName.Returns("Ncc.Tests");
            mock.ContentRootPath.Returns(AppContext.BaseDirectory);
            mock.WebRootPath.Returns(AppContext.BaseDirectory);
            return mock;
        }

        private void RegisterFakeConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
            {
                { "ConnectionStrings:Default", "fake-connection-string" },
                { "App:ServerRootAddress", "http://localhost" },
                { "Authentication:JwtBearer:SecurityKey", "Timesheet_C421AAEE0D114E9C" },
                { "Authentication:JwtBearer:Issuer", "Timesheet" },
                { "Authentication:JwtBearer:Audience", "Timesheet" },
                { "MezonService:ClientId", "test-client-id" },
                { "MezonService:ClientSecret", "test-client-secret" },
                { "MezonService:RedirectUri", "http://localhost/callback" },
                { "MezonService:BaseAddress", "http://localhost" }
            });

            var configuration = configBuilder.Build();

            IocManager.IocContainer.Register(
                Component.For<IConfiguration>()
                    .Instance(configuration)
                    .LifestyleSingleton()
            );

        }

        public override void Initialize()
        {
            var mezonService = NSubstitute.Substitute.For<Timesheet.Services.Mezon.IMezonService>();
            var projectServiceInterface = NSubstitute.Substitute.For<Timesheet.Services.Project.IProjectService>();
            var uploadFileService = NSubstitute.Substitute.For<Timesheet.UploadFilesService.IUploadFileService>();
            var userServices = NSubstitute.Substitute.For<IUserServices>();
            var hrmServiceInterface = NSubstitute.Substitute.For<Timesheet.Services.HRM.IHRMService>();
            var hrmv2Service = NSubstitute.Substitute.For<Timesheet.Services.HRMv2.IHRMv2Service>();

            IocManager.IocContainer.Register(
                Component.For<Timesheet.Services.Mezon.IMezonService>()
                    .Instance(mezonService),

                Component.For<Timesheet.Services.Project.IProjectService>()
                    .Instance(projectServiceInterface),

                Component.For<Timesheet.UploadFilesService.IUploadFileService>()
                    .Instance(uploadFileService),

                Component.For<IUserServices>()
                    .Instance(userServices),

                Component.For<Timesheet.Services.HRM.IHRMService>()
                    .Instance(hrmServiceInterface),

                Component.For<Timesheet.Services.HRMv2.IHRMv2Service>()
                    .Instance(hrmv2Service)
            );

            IocManager.IocContainer.Register(
                Component.For<UploadAvatarService>()
                    .UsingFactoryMethod(kernel =>
                    {
                        var fileService = kernel.Resolve<Timesheet.UploadFilesService.IUploadFileService>();
                        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<UploadAvatarService>>();
                        var tenantManager = kernel.Resolve<Ncc.MultiTenancy.TenantManager>();
                        var session = kernel.Resolve<Abp.Runtime.Session.IAbpSession>();
                        return new UploadAvatarService(fileService, logger, tenantManager, session);
                    })
                    .LifestyleTransient()
            );

            IocManager.IocContainer.Register(
                Component.For<ProjectService>()
                    .UsingFactoryMethod(kernel =>
                    {
                        var httpClient = kernel.Resolve<HttpClient>();
                        var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string>
                            {
                        {"ProjectService:BaseAddress","http://localhost/"},
                        {"ProjectService:SecurityCode","fake"}
                            }).Build();
                        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<ProjectService>>();
                        return NSubstitute.Substitute.For<ProjectService>(httpClient, config, logger);
                    })
                    .LifestyleTransient(),

                Component.For<HRMService>()
                    .UsingFactoryMethod(kernel =>
                    {
                        var httpClient = kernel.Resolve<HttpClient>();
                        var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string>
                            {
                        {"HRMService:BaseAddress","http://localhost/"},
                        {"HRMService:SecurityCode","fake"}
                            }).Build();
                        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<HRMService>>();
                        return NSubstitute.Substitute.For<HRMService>(httpClient, config, logger);
                    })
                    .LifestyleTransient(),

                Component.For<PublicAppService>()
                    .UsingFactoryMethod(kernel =>
                    {
                        var overTime = NSubstitute.Substitute.For<Timesheet.APIs.OverTimeHours.OverTimeHourAppService>();
                        var httpAccessor = NSubstitute.Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
                        var workScope = kernel.Resolve<IWorkScope>();
                        return NSubstitute.Substitute.For<PublicAppService>(overTime, httpAccessor, workScope);
                    })
                    .LifestyleTransient()
            );

            IocManager.RegisterAssemblyByConvention(typeof(TimesheetApplicationModule).Assembly);

            ServiceCollectionRegistrar.Register(IocManager);
        }

        private void RegisterFakeService<TService>() where TService : class
        {
            IocManager.IocContainer.Register(
                Component.For<TService>()
                    .UsingFactoryMethod(() => Substitute.For<TService>())
                    .LifestyleSingleton()
            );
        }
    }
}