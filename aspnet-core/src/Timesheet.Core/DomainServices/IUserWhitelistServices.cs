using Abp.Domain.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DataExport;
using Timesheet.DomainServices.Dto;

namespace Timesheet.DomainServices
{
    public interface IUserWhitelistServices : IDomainService
    {
        Task<GetUserWhitelistDto> Add(AddUserWhitelistDto input);
        Task<List<GetUserWhitelistDto>> GetAll();
        Task<bool> Delete(long id);
        Task<FileBase64Dto> DownloadTemplate();
        Task<ImportUserWhitelistResultDto> ImportFromExcel(IFormFile file);
    }
}
