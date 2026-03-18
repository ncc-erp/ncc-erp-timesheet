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

namespace Timesheet.APIs.UserWhitelists
{
    public class UserWhitelistAppService : AppServiceBase
    {
        private readonly IUserWhitelistServices _userWhitelistServices;
        public UserWhitelistAppService(IUserWhitelistServices userWhitelistServices, IWorkScope workScope) : base(workScope)
        {
            _userWhitelistServices = userWhitelistServices;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserWhitelist_Add)]
        public async Task<GetUserWhitelistDto> Add(AddUserWhitelistDto input)
        {
            return await _userWhitelistServices.Add(input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserWhitelist_View)]
        public async Task<List<GetUserWhitelistDto>> GetAll()
        {
            return await _userWhitelistServices.GetAll();
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserWhitelist_Delete)]
        public async Task<bool> Delete(long id)
        {
            return await _userWhitelistServices.Delete(id);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserWhitelist_DownloadTemplate)]
        public async Task<FileBase64Dto> DownloadTemplate()
        {
            return await _userWhitelistServices.DownloadTemplate();
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_UserWhitelist_Import)]
        public async Task<ImportUserWhitelistResultDto> ImportFromExcel([FromForm] IFormFile file)
        {
            return await _userWhitelistServices.ImportFromExcel(file);
        }
    }
}
