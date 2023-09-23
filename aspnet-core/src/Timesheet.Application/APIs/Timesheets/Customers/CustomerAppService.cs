using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Ncc.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Timesheets.Customers.Dto;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Ncc.IoC;

namespace Ncc.Timesheets.Customers
{
    [AbpAuthorize(
        Ncc.Authorization.PermissionNames.Admin_Clients, 
        Authorization.PermissionNames.Project, 
        Authorization.PermissionNames.Admin_Tasks
    )]
    public class CustomerAppService : AppServiceBase
    {
        public CustomerAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Authorization.PermissionNames.Admin_Clients_View, Authorization.PermissionNames.Admin_Tasks_View)]
        [HttpPost]
        public async Task<GridResult<CustomerDto>> GetAllPagging(GridParam input)
        {
            var query = WorkScope.GetAll<Customer>().OrderByDescending(nameof(Customer.CreationTime)).ProjectTo<CustomerDto>();
            return await query.GetGridResult(query, input);
        }

        [AbpAuthorize]
        public async Task<List<SelectCustomerDto>> GetAll()
        {
            return await WorkScope.GetAll<Customer>().ProjectTo<SelectCustomerDto>().ToListAsync();
        }

        [AbpAuthorize(Authorization.PermissionNames.Admin_Clients_AddNew, Authorization.PermissionNames.Admin_Clients_Edit)]
        public async Task<CustomerDto> Save(CustomerDto input)
        {
            var isExistName = await WorkScope.GetAll<Customer>().AnyAsync(s => s.Name == input.Name && s.Id != input.Id);
            if (isExistName)
                throw new UserFriendlyException(string.Format("Customer name {0} already exist", input.Name));

            var isExistCode = await WorkScope.GetAll<Customer>().AnyAsync(s => s.Code == input.Code && s.Id != input.Id);
            if (isExistCode)
                throw new UserFriendlyException(string.Format("Customer code {0} already exist", input.Code));
            if (input.Id <= 0) //insert
            {
                var item = ObjectMapper.Map<Customer>(input);
                input.Id = await WorkScope.InsertAndGetIdAsync(item);
            }
            else //update
            {
                var item = await WorkScope.GetAsync<Customer>(input.Id);
                ObjectMapper.Map<CustomerDto, Customer>(input, item);
                await WorkScope.UpdateAsync(item);                
            }
            return input;
        }

        [AbpAuthorize(Authorization.PermissionNames.Admin_Clients_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            var hasProject = await WorkScope.GetAll<Project>().AnyAsync(s => s.CustomerId == input.Id);
            if (hasProject)
                throw new UserFriendlyException(String.Format("Client Id {0} has project", input.Id));
            await WorkScope.DeleteAsync<Customer>(input.Id);
        }
    }
}
