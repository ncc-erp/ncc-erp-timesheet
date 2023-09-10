using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Word;
using Ncc;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Positions.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Extension;
using Castle.MicroKernel;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Abp.Application.Services.Dto;
using Ncc.IoC;
using Ncc.Authorization.Users;

namespace Timesheet.APIs.Positions
{
    public class PositionAppService : AppServiceBase
    {
        public PositionAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Position_View)]
        public async Task<GridResult<PositionDto>> GetAllPagging(GridParam input)
        {
            var query = WorkScope.GetAll<Position>()
                 .Select(s => new PositionDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     ShortName = s.ShortName,
                     Code = s.Code,
                     Color = s.Color,
                 });
            return await query.GetGridResult(query, input);
        }


        [HttpGet]
        public async Task<GridResult<PositionDto>> GetAllPosition(GridParam input)
        {
            var query = WorkScope.GetAll<Position>()
                 .Select(s => new PositionDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     ShortName = s.ShortName,
                     Code = s.Code,
                     Color = s.Color,
                 });
            return await query.GetGridResult(query, input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Position_View)]
        public async Task<List<PositionDto>> GetAll()
        {
            var query = await WorkScope.GetAll<Position>()
                 .Select(s => new PositionDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     ShortName = s.ShortName,
                     Code = s.Code,
                     Color = s.Color,
                 }).ToListAsync();
            return query;
        }

        private async System.Threading.Tasks.Task ValPosition(PositionCreateEditDto input)
        {
            var isExistName = await WorkScope.GetAll<Position>()
                 .Where(s => s.Name == input.Name).Where(s => s.Id != input.Id).AnyAsync();
            if (isExistName)
                throw new UserFriendlyException(string
                    .Format("Position name {0} already existed", input.Name));

            var isExistShortName = await WorkScope.GetAll<Position>()
                 .Where(s => s.ShortName == input.ShortName).Where(s => s.Id != input.Id).AnyAsync();
            if (isExistShortName)
                throw new UserFriendlyException(string
                    .Format("Short name {0} already existed", input.ShortName));

            var isExistCode = await WorkScope.GetAll<Position>()
                 .Where(s => s.Code == input.Code).Where(s => s.Id != input.Id).AnyAsync();
            if (isExistCode)
                throw new UserFriendlyException(string
                    .Format("Code {0} already existed", input.Code));
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Position_AddNew)]
        public async Task<PositionCreateEditDto> Create(PositionCreateEditDto input)
        {
            await ValPosition(input);
            var item = ObjectMapper.Map<Position>(input);
            await WorkScope.InsertAsync(item);

            return input;
        }

        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Position_Edit)]
        public async Task<PositionCreateEditDto> Update(PositionCreateEditDto input)
        {
            await ValPosition(input);
            var item = await WorkScope.GetAsync<Position>(input.Id);
            ObjectMapper.Map<PositionCreateEditDto, Position>(input, item);
            await WorkScope.UpdateAsync(item);

            return input;
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Position_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var hasUserPosition = await WorkScope.GetAll<RetroResult>().Where(x => x.PositionId == input.Id).AnyAsync();
            var hasRecord = await WorkScope.GetAll<Position>().Where(x => x.Id == input.Id).AnyAsync();
            var hasUser = await WorkScope.GetAll<User>().AnyAsync(s => s.PositionId == input.Id);
            if (!hasRecord)
                throw new UserFriendlyException(string.Format("There is no entity Position with id = {0}!", input.Id));

            if (hasUserPosition)
                throw new UserFriendlyException(string.Format("This position is already in the record of retro results"));

            if (hasUser)
                throw new UserFriendlyException(String.Format("Position Id {0} has user", input.Id));
            await WorkScope.GetRepo<Position>().DeleteAsync(input.Id);
        }
        public async Task<List<PositionDto>> GetAllPositionDropDownList()
        {
            return await WorkScope.GetAll<Position>()
                 .Select(s => new PositionDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     ShortName = s.ShortName,
                     Code = s.Code,
                     Color = s.Color,
                 }).ToListAsync();
        }
    }
}