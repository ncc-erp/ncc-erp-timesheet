using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Ncc.Configuration;
using Abp.Threading.BackgroundWorkers;
using Timesheet.BackgroundWorker;

namespace Ncc.Web.Host.Startup
{
    [DependsOn(
       typeof(TimesheetWebCoreModule))]
    public class TimesheetWebHostModule: AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public TimesheetWebHostModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(TimesheetWebHostModule).GetAssembly());
        }
        public override void PostInitialize()
        {
            var workManager = IocManager.Resolve<IBackgroundWorkerManager>();
            //workManager.Add(IocManager.Resolve<LockTimeSheet>());
            //workManager.Add(IocManager.Resolve<SubmitTimesheet>());
            workManager.Add(IocManager.Resolve<AddDataToTimeKeeping>());
            workManager.Add(IocManager.Resolve<NotifyApproveRequestOffWorker>());
            workManager.Add(IocManager.Resolve<NotifyTeamBuildingWorker>());
            workManager.Add(IocManager.Resolve<NotifyHRTheEmployeeMayHaveLeftWorker>());
            workManager.Add(IocManager.Resolve<SendMessageToUserWorker>());
        }
    }
}
