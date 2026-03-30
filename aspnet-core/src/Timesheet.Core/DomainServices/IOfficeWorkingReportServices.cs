using Abp.Application.Services.Dto;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Paging;

namespace Timesheet.DomainServices
{
    public interface IOfficeWorkingReportServices : IDomainService
    {
        Task<PagedResultDto<OfficeWorkingTopLWLMDto>> GetOfficeWorkingTimelogReport(GridParam param, GetOfficeWorkingTimelogReportInputDto input);
        Task<string> SendTopOfficeUsersNotification(SendTopOfficeWorkingTimeNotificationDto input);
    }
}
