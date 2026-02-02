using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.DomainServices
{
    public interface ICommonServices: IDomainService
    {
        DateTime getlockDateUser();
        DateTime getlockDatePM();
        Task checkIsMonthLocked(IEnumerable<DateTime> dates);
    }
}
