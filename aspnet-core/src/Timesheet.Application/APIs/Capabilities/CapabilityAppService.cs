using Abp.Application.Services.Dto;
using Abp.Authorization;
using Ncc;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Timesheet.APIs.Capabilities.Dto;
using Timesheet.APIs.RetroDetails.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Extension;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Ncc.Authorization.Users;
using Ncc.IoC;
using DocumentFormat.OpenXml.VariantTypes;

namespace Timesheet.APIs.Capabilities
{
    public class CapabilityAppService : AppServiceBase
    {
        public CapabilityAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Capability_View)]
        public async Task<PagedResultDto<GetCapabilityDto>> GetAllPaging(GridParam input)
        {
            var query = WorkScope.GetAll<Capability>().
                Select(s => new GetCapabilityDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Type = s.Type,
                    Note = s.Note,
                    ApplySetting = WorkScope.GetAll<CapabilitySetting>().Where(x => x.CapabilityId == s.Id).OrderBy(x => x.UserType)
                    .Select(k => new GetCapabilitySettingByCapabilityIdDto
                    {
                        Usertype = k.UserType,
                        PositionName = k.Position.ShortName,
                        Coefficient = k.Coefficient
                    }).ToList()
                });
            var temp = await query.GetGridResult(query, input);
            return new PagedResultDto<GetCapabilityDto>(temp.TotalCount, temp.Items);
        }
        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Capability_View)]
        public async Task<List<GetCapabilityDto>> GetAll()
        {
            var query = WorkScope.GetAll<Capability>().
                Select(s => new GetCapabilityDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Type = s.Type,
                    Note = s.Note,
                    ApplySetting = WorkScope.GetAll<CapabilitySetting>().Where(x => x.CapabilityId == s.Id)
                    .Select(k => new GetCapabilitySettingByCapabilityIdDto
                    {
                        Usertype = k.UserType,
                        PositionName = k.Position.ShortName,
                        Coefficient = k.Coefficient
                    }).ToList()
                });
            return await query.ToListAsync();
        }
        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Capability_AddNew)]
        public async Task<GetCapabilityDto> Create(GetCapabilityDto input)
        {
            var isExist = await WorkScope.GetAll<Capability>()
                .Where(s => s.Name == input.Name).Where(s => s.Id != input.Id).AnyAsync();
            if (isExist)
            {
                throw new UserFriendlyException(string
                    .Format("This Capability already exist"));
            }
            if (input.Id <= 0)
            {
                var item = ObjectMapper.Map<Capability>(input);
                input.Id = await WorkScope.InsertAndGetIdAsync(item);
            }
            return input;
        }
        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Capability_Edit)]
        public async Task<GetCapabilityDto> Update(GetCapabilityDto input)
        {
            var isExist = await WorkScope.GetAll<Capability>()
                 .Where(s => s.Name == input.Name).Where(s => s.Id != input.Id).AnyAsync();
            if (isExist)
            {
                throw new UserFriendlyException(string
                    .Format("This Capability already exist"));
            }
            if (input.Id >= 0)
            {
                var item = await WorkScope.GetAsync<Capability>(input.Id);
                ObjectMapper.Map<GetCapabilityDto, Capability>(input, item);
                await WorkScope.UpdateAsync(item);
            }
            return input;
        }
        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Capability_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var item = await WorkScope.GetAsync<Capability>(input.Id);
            var hasCapabilitySetting = await WorkScope.GetAll<CapabilitySetting>().AnyAsync(s => s.CapabilityId == input.Id);
            if (hasCapabilitySetting)
            {
                throw new UserFriendlyException(String.Format("This Capability Id = {0} is already in somewhere", input.Id));
            }
            if (item == null)
            {
                throw new UserFriendlyException(string.Format("There is no entity Capability with id = {0}!", input.Id));
            }
            await WorkScope.GetRepo<Capability>().DeleteAsync(input.Id);
        }
    }
}