using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices
{
    public interface ICommonServices: IDomainService
    {
        DateTime getlockDateUser();
        DateTime getlockDatePM();
    }
}
