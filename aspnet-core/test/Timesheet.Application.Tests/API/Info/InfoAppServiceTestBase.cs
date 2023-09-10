using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.ObjectMapping;
using Abp.Runtime.Caching;
using Abp.Runtime.Session;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.IoC;
using NSubstitute;
using System.Collections.Generic;
using System.Net.Http;
using Timesheet.APIs.Info;
using Timesheet.DomainServices;
using Timesheet.Services.Komu;

namespace Timesheet.Application.Tests.API.Info
{
    public class InfoAppServiceTestBase : TimesheetApplicationTestBase
    {
        public InfoAppService InfoAppServiceInstance() 
        {
            var configOptions = new Dictionary<string, string>
                {
                    {"KomuService:ChannelIdDevMode", ""},
                    {"KomuService:EnableKomuNotification", "true"},
                    {"KomuService:BaseAddress", "http://www.myserver.com"},
                    {"KomuService:SecurityCode", "secretCode"}
                };

            var configuration = new ConfigurationBuilder()
              .AddInMemoryCollection(configOptions)
              .Build();

            var mockIUnitOfWorkManager = Resolve<IUnitOfWorkManager>();
            var mockRoleRepository = Resolve<IRepository<Role>>();
            var mockRolePermissionSettingRepository = Resolve<IRepository<RolePermissionSetting, long>>();

            var mockRoleStore = Substitute.For<RoleStore>(
                mockIUnitOfWorkManager,
                mockRoleRepository,
                mockRolePermissionSettingRepository);
            var mockError = Substitute.For<IdentityErrorDescriber>();
            var mockroleValidators = Substitute.For<IEnumerable<IRoleValidator<Role>>>();
            var mockILookupNormalizer = Resolve<ILookupNormalizer>();
            var mockILogger = Resolve<ILogger<AbpRoleManager<Role, User>>>();
            var mockIPermissionManager = Resolve<IPermissionManager>();
            var mockICacheManager = Resolve<ICacheManager>();
            var mockIRoleManagementConfig = Resolve<IRoleManagementConfig>();
            var mockIObjectMapper = Resolve<IObjectMapper>();

            var mockRoleManager = Substitute.For<RoleManager>(
                mockRoleStore,
                mockroleValidators,
                mockILookupNormalizer,
                mockError,
                mockILogger,
                mockIPermissionManager,
                mockICacheManager,
                mockIUnitOfWorkManager,
                mockIRoleManagementConfig);

            var mockUserManager = Resolve<UserManager>();
            var mockIAbpSession = Resolve<IAbpSession>();
            var mockHttpClient = Substitute.For<HttpClient>();
            var logger = Resolve<ILogger<KomuService>>();
            var workScope = Resolve<IWorkScope>();
            var settingManager = Substitute.For<ISettingManager>();
            var mockKomuSerive = Substitute.For<KomuService>(
                mockHttpClient,
                logger,
                configuration,
                settingManager);

            var mockUserService = Substitute.For<UserServices>(
                mockUserManager,
                mockKomuSerive,
                mockIAbpSession,
                mockRoleManager,
                mockIObjectMapper,
                workScope);
            mockUserService.WorkScope= Resolve<IWorkScope>();

            var httpContextAccessor = Resolve<IHttpContextAccessor>();
            var commonServices = Resolve<ICommonServices>();

            var inforAppService = new InfoAppService(
                httpContextAccessor,
                commonServices,
                mockUserService,
                workScope);
            inforAppService.SettingManager = Resolve<ISettingManager>();

            return inforAppService;
        }
    }
}
