using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Extensions;
using Abp.UI;
using AutoMapper.QueryableExtensions;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.DayOffs.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.DayOffs
{
    [AbpAuthorize(
        Ncc.Authorization.PermissionNames.DayOff,
        Ncc.Authorization.PermissionNames.Report_NormalWorking,
        Ncc.Authorization.PermissionNames.MyAbsenceDay,
        Ncc.Authorization.PermissionNames.AbsenceDayByProject,
        Ncc.Authorization.PermissionNames.AbsenceDayOfTeam
    )]
    public class DayOffAppService : AppServiceBase
    {
        public DayOffAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpGet]
        [AbpAuthorize(
            Ncc.Authorization.PermissionNames.DayOff_View,
            Ncc.Authorization.PermissionNames.Report_NormalWorking_View,
            Ncc.Authorization.PermissionNames.MyAbsenceDay_View,
            Ncc.Authorization.PermissionNames.AbsenceDayByProject,
            Ncc.Authorization.PermissionNames.AbsenceDayOfTeam
        )]
        public async Task<List<DayOffDto>> GetAll(int year, int month, long? branchId)
        {
            return await WorkScope.GetAll<DayOffSetting>()
                     .Where(s => s.DayOff.Year == year && s.DayOff.Month == month)
                     .Select(s =>
                        new DayOffDto
                        {
                            Id = s.Id,
                            DayOff = s.DayOff,
                            Name = s.Name,
                            Coefficient = s.Coefficient,
                            Branch = s.Branch
                        }
                        ).ToListAsync();
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.DayOff_AddNew, Ncc.Authorization.PermissionNames.DayOff_Edit)]
        public async Task<DayOffDto> Save(DayOffDto input)
        {
            var isExist = await WorkScope.GetAll<DayOffSetting>().AnyAsync(s => s.DayOff == input.DayOff && s.Id != input.Id);
            if (isExist)
                throw new UserFriendlyException(string.Format("DayOff - {0} already existed", input.DayOff.ToString("MM/dd/yyyy")));

            if (input.Id <= 0) //insert
            {
                var item = ObjectMapper.Map<DayOffSetting>(input);
                await WorkScope.InsertAsync(item);
            }
            else //update
            {
                var item = await WorkScope.GetAsync<DayOffSetting>(input.Id);
                ObjectMapper.Map<DayOffDto, DayOffSetting>(input, item);
                await WorkScope.UpdateAsync(item);
            }
            return input;
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.DayOff_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            await WorkScope.GetRepo<DayOffSetting>().DeleteAsync(input.Id);
        }

    }
}

