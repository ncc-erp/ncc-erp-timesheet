using System;
using System.Collections.Generic;
using System.Net.Http;
using Castle.MicroKernel.Registration;
using NSubstitute;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Modules;
using Abp.Configuration.Startup;
using Abp.Net.Mail;
using Abp.TestBase;
using Abp.Zero.Configuration;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.EntityFrameworkCore;
using Ncc.IoC;
using Ncc.Tests.DependencyInjection;
using Ncc.Configuration;
using Timesheet.Services.Komu;
using Timesheet.Services.Mezon;

namespace Ncc.Tests
{
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

            // Disable static mapper usage since it breaks unit tests (see https://github.com/aspnetboilerplate/aspnetboilerplate/issues/2052)
            //Configuration.Modules.AbpAutoMapper().UseStaticMapper = false;

            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            // Use database for language management
            Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();

            RegisterFakeService<AbpZeroDbMigrator<TimesheetDbContext>>();
            var testConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "MezonService:BaseAddress", "http://localhost:4200/" },
                    { "MezonService:ClientId", "test" },
                    { "MezonService:ClientSecret", "test" },
                    { "MezonService:RedirectUri", "http://localhost:4200/" },
                    { "MezonService:AppToken", "test" },
                    { "KomuService:BaseAddress", "http://localhost:4300/" },
                    { "KomuService:SecurityCode", "test" },
                    { "KomuService:DevModeChannelId", "" },
                    { "KomuService:EnableKomuNotification", "false" }
                })
                .Build();
            IocManager.IocContainer.Register(
                Component.For<IConfiguration>()
                    .Instance(testConfig)
                    .LifestyleSingleton()
            );
            IocManager.IocContainer.Register(
                Component.For<KomuService>()
                    .UsingFactoryMethod(() => Substitute.For<KomuService>(
                        new HttpClient(),
                        Substitute.For<ILogger<KomuService>>(),
                        testConfig,
                        Substitute.For<ISettingManager>()))
                    .LifestyleSingleton()
            );
            IocManager.IocContainer.Register(
                Component.For<MezonService>()
                    .UsingFactoryMethod(() => Substitute.For<MezonService>(
                        new HttpClient(),
                        Substitute.For<ISettingManager>(),
                        Substitute.For<ILogger<MezonService>>(),
                        testConfig,
                        Substitute.For<IWorkScope>()))
                    .LifestyleSingleton()
            );

            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);

            Configuration.Settings.Providers.Add<AppSettingProvider>();
        }

        public override void Initialize()
        {
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
