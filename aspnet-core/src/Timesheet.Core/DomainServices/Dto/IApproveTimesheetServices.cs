using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public interface IApproveTimesheetServices : IDomainService
    {
        List<NotifyApproveTimesheetDto> GetListPmNotApproveTimesheet();
    }
}
