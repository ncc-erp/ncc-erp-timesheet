﻿using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Ncc.Authorization;
using Timesheet.BackgroundWorker;

namespace Ncc
{
    [DependsOn(
        typeof(TimesheetCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class TimesheetApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<TimesheetAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(TimesheetApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);
            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddProfiles(thisAssembly)
            );
        }
    }
}
