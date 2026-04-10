using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;

namespace Timesheet.DomainServices
{
    public interface IManageUserProjectForBranchServices : IDomainService
    {
        Task<List<ProjectHistoryDto>> GetUserProjectHistory(long userId, DateTime? startDate, DateTime? endDate);
    }
}
