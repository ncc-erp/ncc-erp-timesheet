using Microsoft.AspNetCore.Mvc;
using Ncc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.Timesheets.Tasks.Dto;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Task = Ncc.Entities.Task;
using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using Abp.Domain.Repositories;
using Abp.UI;
using Ncc.Entities;
using Abp.Authorization;
using Ncc.IoC;

namespace Timesheet.Timesheets.Tasks
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Tasks, Ncc.Authorization.PermissionNames.Project)]

    public class TaskAppService : AppServiceBase
    {
        public TaskAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Tasks_View, Ncc.Authorization.PermissionNames.Project_View)]
        public async Task<List<TaskDto>> GetAll()
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                return await WorkScope.GetAll<Task>().ProjectTo<TaskDto>().ToListAsync();
            }
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Tasks_ChangeStatus)]
        //delete task by id
        public async System.Threading.Tasks.Task Archive(EntityDto<long> input)
        {
            var hasProjectTask = await WorkScope.GetRepo<ProjectTask>().GetAllIncluding(s => s.Project).AnyAsync(s => s.TaskId == input.Id);
            if (hasProjectTask)
            {
                throw new UserFriendlyException(string.Format("This taskId {0} is in a project ,You can't delete task", input.Id));
            }
            else
            {
                await WorkScope.DeleteAsync<Task>(input.Id);
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Tasks_ChangeStatus)]
        public async System.Threading.Tasks.Task DeArchive(EntityDto<long> input)
        {
            using (CurrentUnitOfWork.DisableFilter(Abp.Domain.Uow.AbpDataFilters.SoftDelete))
            {
                var getTask = await WorkScope.GetAsync<Task>(input.Id);
                if (getTask != null)
                {
                    getTask.IsDeleted = false;
                    await WorkScope.UpdateAsync<Task>(getTask);
                }
                else
                {
                    throw new UserFriendlyException(string.Format("Task is not exist"));
                }
            }
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Tasks_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            using (CurrentUnitOfWork.DisableFilter(Abp.Domain.Uow.AbpDataFilters.SoftDelete))
            {
                var hasProjectTask = await WorkScope.GetRepo<ProjectTask>().GetAllIncluding(s => s.Project).AnyAsync(s => s.TaskId == input.Id && !s.IsDeleted && !s.Project.IsDeleted);               
                if (hasProjectTask)
                {
                    throw new UserFriendlyException(string.Format("This taskId {0} is in a project ,You can't delete task", input.Id));
                }
                else
                {
                    await WorkScope.Repository<Task>().HardDeleteAsync(s => s.Id == input.Id);
                }
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Tasks_AddNew, Ncc.Authorization.PermissionNames.Admin_Tasks_Edit)]
        //insert and update task
        public async Task<TaskDto> Save(TaskDto input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                var isExist = await WorkScope.GetAll<Task>().AnyAsync(s => s.Name == input.Name && s.Id != input.Id);
                if (isExist)
                {
                    throw new UserFriendlyException(string.Format("Task {0} is already exist", input.Name));
                }
                //insert
                /*
                 * var item = new Customer
                    {
                        Name = input.Name,
                        Address = input.Address
                    };
                    input.Id = await WorkScope.InsertAndGetIdAsync(item);
                */

                if (input.Id <= 0)
                {
                    var item = ObjectMapper.Map<Task>(input);
                    input.Id = await WorkScope.InsertAndGetIdAsync(item);
                }
                else // update
                {
                    var item = await WorkScope.GetAsync<Task>(input.Id);

                    ObjectMapper.Map<TaskDto, Task>(input, item);
                    await WorkScope.UpdateAsync(item);
                }

                return input;
            }

        }
    }
}
