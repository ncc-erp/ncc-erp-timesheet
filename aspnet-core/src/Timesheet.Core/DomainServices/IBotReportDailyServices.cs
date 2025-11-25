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
        Task<List<ProjectTimelogDto>> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportInput input);
        Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportByBranchCodesInput input);
        Task<bool> SendDailyProjectTimelogToMezon();
    }
}
