using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.Retros.Dto;
using Abp.Application.Services.Dto;
using static Ncc.Entities.Enum.StatusEnum;
using System;
using Timesheet.Paging;
using Timesheet.Extension;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Timesheet.Entities;
using Ncc.Entities.Enum;
using Timesheet.APIs.Positions.Dto;
using Ncc.IoC;

namespace Timesheet.APIs.Retros
{
    public class RetroAppService : AppServiceBase
    {
        public RetroAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_View)]
        public async Task<GridResult<RetroDto>> GetAllPagging(GridParam input)
        {
            var query = WorkScope.GetAll<Retro>()
                                 .Select(x => new RetroDto
                                 {
                                     Id = x.Id,
                                     Name = x.Name,
                                     StartDate = x.StartDate,
                                     EndDate = x.EndDate,
                                     Deadline = x.Deadline,
                                     Status = x.Status
                                 }).OrderByDescending(x => x.StartDate);
            return await query.GetGridResult(query, input);
        }

        private async System.Threading.Tasks.Task ValRetro(RetroCreateDto input)
        {
            var isExist = await WorkScope.GetAll<Retro>()
                .Where(s => (s.Name == input.Name && s.Id != input.Id)
                            || s.StartDate.ToString("MM/yyyy") == input.StartDate.ToString("MM/yyyy")).AnyAsync();
            if (input.StartDate > input.EndDate) throw new UserFriendlyException("Start date > end date");
            if (input.StartDate > input.Deadline) throw new UserFriendlyException("Start date > deadline");
            if (isExist)
                throw new UserFriendlyException(string.Format("{0} ({1}) already existed", input.Name, input.StartDate.ToString("MM/yyyy")));
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_AddNew)]
        public async Task<RetroCreateDto> Create(RetroCreateDto input)
        {
            await ValRetro(input);
            var retro = ObjectMapper.Map<Retro>(input);
            await WorkScope.InsertAsync(retro);
            return input;
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var retro = await GetRetroById(input.Id);
            if (retro == default || retro == null)
                throw new UserFriendlyException(string.Format("There is no entity retro with id = {0}!", input.Id));

            var hasRetroDetail = await WorkScope.GetAll<RetroResult>().AnyAsync(s => s.RetroId == input.Id);
            if (hasRetroDetail)
                throw new UserFriendlyException(String.Format("Retro Id {0} has retro detail", input.Id));

            if (retro.Status == RetroStatus.Close)
            {
                throw new UserFriendlyException("Cannot be deleted because the retro status close");
            }
            await WorkScope.GetRepo<Retro>().DeleteAsync(input.Id);
        }

        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_ChangeStatus)]
        public async System.Threading.Tasks.Task ChangeStatus(EntityDto<long> input)
        {
            var retro = await GetRetroById(input.Id);
            var statusNew = StatusEnum.RetroStatus.Public;
            if (retro.Status == StatusEnum.RetroStatus.Public)
            {
                statusNew = StatusEnum.RetroStatus.Close;
            }
            retro.Status = statusNew;
            await WorkScope.UpdateAsync<Retro>(retro);
        }

        [HttpPut]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Retro_Edit)]
        public async Task<RetroEditDto> Update(RetroEditDto input)
        {
            var isExist = await WorkScope.GetAll<Retro>()
                .Where(s => s.Name == input.Name && s.Id != input.Id).AnyAsync();
            if (isExist)
                throw new UserFriendlyException(string.Format("Retro {0} already existed", input.Name));

            var item = await WorkScope.GetAsync<Retro>(input.Id);
            ObjectMapper.Map<RetroEditDto, Retro>(input, item);
            await WorkScope.UpdateAsync(item);

            return input;
        }

        private async Task<Retro> GetRetroById(long retroId)
        {
            return await WorkScope.GetAsync<Retro>(retroId);
        }
    }
}