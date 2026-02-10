using Abp.Application.Services.Dto;
using Abp.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DataExport;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RemoteBlacklists
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist)]
    public class RemoteBlacklistAppService : AppServiceBase
    {
        private readonly IRemoteBlacklistServices _remoteBlacklistServices;
        public RemoteBlacklistAppService(IRemoteBlacklistServices remoteBlacklistServices, IWorkScope workScope) : base(workScope)
        {
            _remoteBlacklistServices = remoteBlacklistServices;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist_Add)]
        public async Task<GetRemoteBlacklistDto> AddNewUser(AddNewUserToRemoteBlacklistDto input)
        {
            return await _remoteBlacklistServices.AddNewUser(input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist_View)]
        public async Task<int> GetMaxRemoteDays()
        {
            return await _remoteBlacklistServices.GetMaxRemoteDaysAsync();
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist_View)]
        public async Task<PagedResultDto<GetRemoteBlacklistDto>> GetAll(GridParam param)
        {
            return await _remoteBlacklistServices.GetAll(param);
        }

        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist_Edit)]
        public async Task<GetRemoteBlacklistDto> Update(UpdatePenaltyDaysDto input)
        {
            return await _remoteBlacklistServices.Update(input);
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist_Delete)]
        public async Task<bool> Delete(long id)
        {
            return await _remoteBlacklistServices.Delete(id);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist_DownloadTemplate)]
        public async Task<FileBase64Dto> DownloadTemplate()
        {
            return await _remoteBlacklistServices.DownloadTemplate();
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Remote_Blacklist_Import)]
        public async Task<ImportRemoteBlacklistResultDto> ImportFromExcel([FromForm] IFormFile file)
        {
            return await _remoteBlacklistServices.ImportFromExcel(file);
        }
    }
}
