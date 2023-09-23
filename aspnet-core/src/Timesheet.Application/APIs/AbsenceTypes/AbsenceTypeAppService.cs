using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.UI;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.DayOffs.Dto;
using Timesheet.Entities;

namespace Timesheet.APIs.AbsenceTypes
{
    [AbpAuthorize]
    public class AbsenceTypeAppService : AppServiceBase
    {
        public AbsenceTypeAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_LeaveTypes_AddNew, Ncc.Authorization.PermissionNames.Admin_LeaveTypes_Edit)]
        public async Task<AbsenceTypeDto> Save(AbsenceTypeDto input)
        {
            var isExist = await WorkScope.GetAll<DayOffType>().AnyAsync(s => s.Name == input.Name && s.Id != input.Id);
            if (isExist)
                throw new UserFriendlyException(string.Format("Absence type - {0} already existed", input.Name));

            if (input.Id <= 0) //insert
            {
                var item = ObjectMapper.Map<DayOffType>(input);
                input.Id = await WorkScope.InsertAndGetIdAsync(item);
            }
            else //update abc
            {
                var item = await WorkScope.GetAsync<DayOffType>(input.Id);
                ObjectMapper.Map<AbsenceTypeDto, DayOffType>(input, item);
                await WorkScope.UpdateAsync(item);
            }
            return input;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_LeaveTypes_View, Ncc.Authorization.PermissionNames.MyAbsenceDay)]
        public async Task<List<AbsenceTypeDto>> GetAll()
        {
            return await WorkScope.GetAll<DayOffType>().ProjectTo<AbsenceTypeDto>().ToListAsync();
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_LeaveTypes_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var item = await WorkScope.GetAsync<DayOffType>(input.Id);
            if (item == null)
                throw new UserFriendlyException(String.Format("Absence type  Id {0} is not exist", input.Id));
            await WorkScope.GetRepo<DayOffType>().DeleteAsync(input.Id);
        }
    }
}
