using Abp.Dependency;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Ncc.IoC;
using Timesheet.Services.Komu;

namespace Timesheet.DomainServices
{
    public class BaseDomainService : DomainService
    {
        public IWorkScope WorkScope { get; set; }
        public BaseDomainService()
        {
            this.WorkScope = IocManager.Instance.Resolve<IWorkScope>();
        }
        public BaseDomainService(IWorkScope workScope)
        {
            this.WorkScope = workScope;
        }
    }
}
