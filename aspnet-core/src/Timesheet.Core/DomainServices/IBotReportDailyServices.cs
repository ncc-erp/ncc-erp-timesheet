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
        Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportInput input);
        Task<bool> SendDailyProjectTimelogToMezon();
    }
}
