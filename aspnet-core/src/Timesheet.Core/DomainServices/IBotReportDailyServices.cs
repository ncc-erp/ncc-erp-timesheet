using Abp.Application.Services.Dto;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Services.Project.Dto;

namespace Timesheet.DomainServices
{
    public interface IBotReportDailyService : IDomainService
    {
        Task<PagedResultDto<TotalTimelogProjectDto>> GetPagedDailyProjectTimelogReport(GridParam param, GetDailyProjectTimelogReportInput input);
        Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportByBranchCodesInput input);
        Task<bool> SendDailyProjectTimelogToMezon();
    }
}
