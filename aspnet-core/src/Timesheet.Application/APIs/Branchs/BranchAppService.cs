using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.Branchs.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Ncc.Configuration;
using Timesheet.Uitls;
using System.Collections.Generic;
using Ncc.Authorization.Users;
using Ncc.IoC;

namespace Timesheet.APIs.Branchs
{
    public class BranchAppService : AppServiceBase
    {
        public BranchAppService(IWorkScope workScope) : base(workScope)
        {
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Branchs_View)]
        public async Task<GridResult<BranchCreateEditDto>> GetAllPagging(GridParam input)
        {
            var query = WorkScope.GetAll<Branch>()
                 .Select(s => new BranchCreateEditDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     DisplayName = s.DisplayName,
                     AfternoonWorking = s.AfternoonWorking,
                     AfternoonEndAt = s.AfternoonEndAt,
                     AfternoonStartAt = s.AfternoonStartAt,
                     MorningWorking = s.MorningWorking,
                     MorningStartAt = s.MorningStartAt,
                     MorningEndAt = s.MorningEndAt,
                     Color = s.Color,
                     Code = s.Code,
                 });

            return await query.GetGridResult(query, input);
        }

        [HttpGet]
        public async Task<List<BranchCreateEditDto>> GetAllNotPagging()
        {
            return await WorkScope.GetAll<Branch>()
                 .Select(s => new BranchCreateEditDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     DisplayName = s.DisplayName,
                     AfternoonWorking = s.AfternoonWorking,
                     AfternoonEndAt = s.AfternoonEndAt,
                     AfternoonStartAt = s.AfternoonStartAt,
                     MorningWorking = s.MorningWorking,
                     MorningStartAt = s.MorningStartAt,
                     MorningEndAt = s.MorningEndAt
                 }).ToListAsync();

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Branchs_AddNew, Ncc.Authorization.PermissionNames.Admin_Branchs_Edit)]
        public async Task<BranchCreateEditDto> Save(BranchCreateEditDto input)
        {
            var isExist = await WorkScope.GetAll<Branch>().Where(s => s.Name == input.Name || s.DisplayName == input.DisplayName || s.Code == input.Code).Where(s => s.Id != input.Id).AnyAsync();
            if (isExist)
                throw new UserFriendlyException(string.Format("Branch name {0} or display name {1} or code {2} already existed ", input.Name, input.DisplayName, input.Code));

            if (input.Id <= 0) //insert
            {
                var item = ObjectMapper.Map<Branch>(input);
                await WorkScope.InsertAndGetIdAsync(item);
            }
            else //update
            {
                var item = await WorkScope.GetAsync<Branch>(input.Id);
                ObjectMapper.Map<BranchCreateEditDto, Branch>(input, item);
                await WorkScope.UpdateAsync(item);
            }

            return input;
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Branchs_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var hasUser = await WorkScope.GetAll<User>().AnyAsync(s => s.BranchId == input.Id);
            if (hasUser)
                throw new UserFriendlyException(String.Format("Branch Id {0} has user", input.Id));
            await WorkScope.GetRepo<Branch>().DeleteAsync(input.Id);
        }

        [HttpGet]
        public async Task<List<BranchDto>> GetAllBranchFilter(bool isAll = false)
        {
            var branchId = await WorkScope.GetAll<Timesheet.Entities.Branch>().Select(s => s.Id).FirstOrDefaultAsync();
            var query = await WorkScope.GetAll<Branch>()
                 .Select(s => new BranchDto
                 {
                     Id = s.Id,
                     Name = s.Name,
                     DisplayName = s.DisplayName
                 }).ToListAsync();
            if (isAll)
            {
                query.Add(new BranchDto
                {
                    Name = "All",
                    DisplayName = "All",
                    Id = 0,
                });
            }
            return query.OrderBy(s => s.Id).ToList();
        }
    }
}