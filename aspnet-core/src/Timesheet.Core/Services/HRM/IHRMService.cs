using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Services.HRM.Dto;
using static Timesheet.Services.HRM.Dto.UpdateToHrmDto;

namespace Timesheet.Services.HRM
{
    public interface IHRMService
    {
        Task<string> UpdateLevel(CreateRequestFromTSDto input);
        Task<UpdateLevelHRMDto> UpdateLevelAfterRejectEmail(UpdateLevelHRMDto input);
        Task<UserContactDto> GetUserContract(long id);
    }
}
