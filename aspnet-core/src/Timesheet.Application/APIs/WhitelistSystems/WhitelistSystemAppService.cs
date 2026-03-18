using Abp.Authorization;
using Ncc;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;

namespace Timesheet.APIs.WhitelistSystems
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_WhitelistSystem)]
    public class WhitelistSystemAppService : AppServiceBase
    {
        private readonly IWhitelistSystemServices _whitelistSystemServices;
        public WhitelistSystemAppService(IWhitelistSystemServices whitelistSystemServices, IWorkScope workScope) : base(workScope)
        {
            _whitelistSystemServices = whitelistSystemServices;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_WhitelistSystem_Add)]
        public async Task<WhitelistSystem> Add(AddWhitelistTypeDto input)
        {
            return await _whitelistSystemServices.Add(input);
        }

        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_WhitelistSystem_Edit)]
        public async Task<WhitelistSystem> Update(UpdateWhitelistTypeDto input)
        {
            return await _whitelistSystemServices.Update(input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_WhitelistSystem_View)]
        public List<WhitelistTypeDto> GetWhitelistTypes()
        {
            return _whitelistSystemServices.GetWhitelistTypes();
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_WhitelistSystem_View)]
        public async Task<List<GetWhitelistSystemDto>> GetAll()
        {
            return await _whitelistSystemServices.GetAll();
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_WhitelistSystem_Delete)]
        public async Task<bool> Delete(long id)
        {
            return await _whitelistSystemServices.Delete(id);
        }
    }
}
