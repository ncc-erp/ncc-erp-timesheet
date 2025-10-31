using Abp.Application.Services.Dto;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Project.Dto;

namespace Timesheet.DomainServices
{
    public interface IBotReportDailyService : IDomainService
    {
        Task<PagedProjectTimelogDto> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportRequestDto request);
        Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportByBranchCodesInput input);
        Task<bool> SendDailyProjectTimelogToMezon();
    }
}
