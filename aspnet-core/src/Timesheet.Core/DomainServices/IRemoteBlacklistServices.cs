using Abp.Application.Services.Dto;
using Abp.Domain.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DataExport;
using Timesheet.DomainServices.Dto;
using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public interface IRemoteBlacklistServices : IDomainService
    {
        Task<GetRemoteBlacklistDto> AddNewUserToRemoteBlacklist(AddNewUserToRemoteBlacklistDto input);
        Task<PagedResultDto<GetRemoteBlacklistDto>> GetAllRemoteBlacklist(GridParam param);
        Task<GetRemoteBlacklistDto> UpdatePenaltyDays(UpdatePenaltyDaysDto input);
        Task<bool> Delete(long id);
        Task<FileBase64Dto> DownloadTemplateImportRemoteBlacklist();
        Task<ImportRemoteBlacklistResultDto> ImportRemoteBlacklist(IFormFile file);
        Task<int> GetMaxRemoteDaysAsync();
    }
}
