using Abp.Application.Services.Dto;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.Timesheets.Projects.Dto;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Entities;
using Abp.Linq.Extensions;
using Timesheet.DomainServices;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Abp.Authorization;
using Timesheet.APIs.Timesheets.Projects.Dto;
using Timesheet.Extension;
using System.Text;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;
using Ncc.IoC;
using Timesheet.Services.Project;

namespace Timesheet.Timesheets.Projects
{
    [AbpAuthorize(
        Ncc.Authorization.PermissionNames.Project,
        Ncc.Authorization.PermissionNames.MyTimesheet,
        Ncc.Authorization.PermissionNames.AbsenceDayByProject,
        Ncc.Authorization.PermissionNames.Report_OverTime,
        Ncc.Authorization.PermissionNames.AbsenceDayOfTeam,
        Ncc.Authorization.PermissionNames.ManageWorkingTime
    )]
    public class ProjectAppService : AppServiceBase
    {
        private readonly IUserServices _userServices;
        private readonly UserManager _userManager;
        private readonly ProjectService _projectService;

        public ProjectAppService(IUserServices userService, ProjectService projectService,
            UserManager userManager, IWorkScope workScope) : base(workScope)
        {
            this._userServices = userService;
            this._userManager = userManager;
            _projectService = projectService;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_AddNew, Ncc.Authorization.PermissionNames.Project_Edit)]
        public async Task<ProjectDto> Save(ProjectDto input)
        {
            var isExistsName = await WorkScope.GetAll<Project>().AnyAsync(s => s.Name == input.Name && s.Id != input.Id);
            if (isExistsName)
            {
                throw new UserFriendlyException(string.Format("Project name:{0} is exists", input.Name));
            }

            var isExistsCode = await WorkScope.GetAll<Project>().AnyAsync(s => s.Code == input.Code && s.Id != input.Id);
            if (isExistsCode)
            {
                throw new UserFriendlyException(string.Format("Project code:{0} is exists", input.Code));
            }

            var hasProjectAdmin = input.Users.Any(s => s.Type == ProjectUserType.PM);
            if (!hasProjectAdmin)
            {
                throw new Exception(string.Format("Project must have at least one project manager"));
            }

            if (input.TimeEnd.HasValue && input.TimeStart.Date > input.TimeEnd.Value.Date)
            {
                throw new UserFriendlyException("Start time cannot be greater than end time !");
            }

            if (input.Id <= 0)//insert 3 bang project, projectTask, projectUser
            {
                var project = ObjectMapper.Map<Project>(input);
                input.Id = await WorkScope.GetRepo<Project, long>().InsertAndGetIdAsync(project);
                CurrentUnitOfWork.SaveChanges();

                //if (project.isAllUserBelongTo)
                //{
                //    var remainUserId = WorkScope.GetAll<User>().Select(u => u.Id).Except(input.Users.Select(u => u.Id));
                //    foreach (var id in remainUserId)
                //    {
                //        var projectUser = new ProjectUser
                //        {
                //            ProjectId = input.Id,
                //            UserId = id,
                //            Type = ProjectUserType.Member
                //        };
                //        await WorkScope.GetRepo<ProjectUser, long>().InsertAsync(projectUser);
                //    }
                //}

                //insert ProjectTask
                foreach (var ptaskDto in input.Tasks)
                {
                    var projectTask = new ProjectTask
                    {
                        ProjectId = input.Id,
                        TaskId = ptaskDto.TaskId,
                        Billable = ptaskDto.Billable
                    };
                    await WorkScope.GetRepo<ProjectTask, long>().InsertAsync(projectTask);
                }

                //insert projectUser
                foreach (var pUserDto in input.Users)
                {
                    var projectUser = new ProjectUser
                    {
                        ProjectId = input.Id,
                        UserId = pUserDto.UserId,
                        Type = pUserDto.Type,
                        IsTemp = pUserDto.IsTemp
                    };
                    await WorkScope.GetRepo<ProjectUser, long>().InsertAsync(projectUser);

                    if (projectUser.Type == ProjectUserType.PM)
                    {
                        var userRoles = await _userServices.UserAllRoles(projectUser.UserId).ToListAsync();
                        if (!userRoles.Any(r => r == StaticRoleNames.Host.Admin || r == StaticRoleNames.Host.ProjectAdmin))
                        {
                            userRoles.Add(StaticRoleNames.Host.ProjectAdmin);
                            var user = await WorkScope.GetAsync<User>(projectUser.UserId);
                            CheckErrors(await _userManager.SetRoles(user, userRoles.ToArray()));
                        }
                    }
                }

                if (input.ProjectTargetUsers != null)
                {
                    foreach (var pTargetUserDto in input.ProjectTargetUsers)
                    {
                        var projectTargetUser = new ProjectTargetUser
                        {
                            ProjectId = input.Id,
                            UserId = pTargetUserDto.UserId,
                            RoleName = pTargetUserDto.RoleName
                        };
                        await WorkScope.GetRepo<ProjectTargetUser, long>().InsertAsync(projectTargetUser);
                    }
                }
            }
            else //edit
            {
                var project = await WorkScope.GetAsync<Project>(input.Id);
                ObjectMapper.Map<ProjectDto, Project>(input, project);
                await WorkScope.GetRepo<Project, long>().UpdateAsync(project);

                //ProjectTask
                var currentProjectTasks = await WorkScope.GetAll<ProjectTask>()
                    .Where(s => s.ProjectId == input.Id).ToListAsync();

                var currentTaskIds = currentProjectTasks.Select(s => s.TaskId).ToList();
                var newTaskIds = input.Tasks.Select(s => s.TaskId).ToList();

                var insertTasks = input.Tasks.Where(x => !currentTaskIds.Contains(x.TaskId));
                var deleteProjectTaskIds = currentProjectTasks.Where(s => !newTaskIds.Contains(s.TaskId)).Select(s => s.Id).ToList();

                var isDeleteProjectTasksInTimeSheet = await WorkScope.GetAll<MyTimesheet>()
                    .AnyAsync(s => deleteProjectTaskIds.Contains(s.ProjectTaskId));
                if (isDeleteProjectTasksInTimeSheet)
                    throw new UserFriendlyException(String.Format("Tasks are logged timesheet so you can't remove"));

                var updateTasks = (from cpt in currentProjectTasks
                                   join ct in input.Tasks on cpt.TaskId equals ct.TaskId
                                   select new
                                   {
                                       ProjectTask = cpt,
                                       Dto = ct
                                   }).ToList();

                foreach (var id in deleteProjectTaskIds)
                {
                    await WorkScope.DeleteAsync<ProjectTask>(id);
                }

                foreach (var ptaskDto in insertTasks)
                {
                    var projectTask = ObjectMapper.Map<ProjectTask>(ptaskDto);
                    projectTask.ProjectId = input.Id;
                    await WorkScope.InsertAsync<ProjectTask>(projectTask);
                }

                foreach (var item in updateTasks)
                {
                    if (item.Dto.Billable != item.ProjectTask.Billable)
                    {
                        item.ProjectTask.Billable = item.Dto.Billable;
                        await WorkScope.UpdateAsync<ProjectTask>(item.ProjectTask);
                    }
                }

                //ProjectUser
                var currentProjectUsers = await WorkScope.GetAll<ProjectUser>()
                    .Where(s => s.ProjectId == input.Id).ToListAsync();
                var currentUserIds = currentProjectUsers.Select(s => s.UserId).ToList();
                var newUserIds = input.Users.Select(s => s.UserId).ToList();

                var deleteUserIds = currentUserIds.Except(newUserIds);
                var isDeleteUsersInTimesheet = await WorkScope.GetRepo<MyTimesheet>()
                    .GetAllIncluding(s => s.ProjectTask)
                    .AnyAsync(s => deleteUserIds.Contains(s.UserId) && s.ProjectTask.ProjectId == project.Id);
                if (isDeleteUsersInTimesheet)
                    throw new UserFriendlyException(String.Format("Users are logged timesheet so you can't remove"));

                var insertUsers = input.Users.Where(x => !currentUserIds.Contains(x.UserId));
                var deleteProjectUserIds = currentProjectUsers.Where(s => !newUserIds.Contains(s.UserId)).Select(s => s.Id).ToList();

                var updateUsers = (from cpu in currentProjectUsers
                                   join cu in input.Users on cpu.UserId equals cu.UserId
                                   select new
                                   {
                                       ProjectUser = cpu,
                                       Dto = cu
                                   }).ToList();

                foreach (var id in deleteProjectUserIds)
                {
                    await WorkScope.DeleteAsync<ProjectUser>(id);
                }

                foreach (var pUserDto in insertUsers)
                {
                    var projectUser = ObjectMapper.Map<ProjectUser>(pUserDto);
                    projectUser.ProjectId = input.Id;
                    await WorkScope.InsertAsync<ProjectUser>(projectUser);

                    if (pUserDto.Type == ProjectUserType.PM)
                    {
                        var userRoles = await _userServices.UserAllRoles(pUserDto.UserId).ToListAsync();
                        if (!userRoles.Any(r => r == StaticRoleNames.Host.Admin || r == StaticRoleNames.Host.ProjectAdmin))
                        {
                            userRoles.Add(StaticRoleNames.Host.ProjectAdmin);
                            var user = await WorkScope.GetAsync<User>(pUserDto.UserId);
                            CheckErrors(await _userManager.SetRoles(user, userRoles.ToArray()));
                        }
                    }
                }

                foreach (var item in updateUsers)
                {
                    if (item.Dto.Type != item.ProjectUser.Type || item.Dto.IsTemp != item.ProjectUser.IsTemp)
                    {
                        item.ProjectUser.Type = item.Dto.Type;
                        var isEditTypeWord = await IsGrantedAsync(Ncc.Authorization.PermissionNames.Project_Edit_Team_WorkType);
                        if (isEditTypeWord)
                        {
                            item.ProjectUser.IsTemp = item.Dto.IsTemp;
                        }

                        await WorkScope.UpdateAsync<ProjectUser>(item.ProjectUser);
                    }
                    if (item.ProjectUser.Type == ProjectUserType.PM)
                    {
                        var userRoles = await _userServices.UserAllRoles(item.ProjectUser.UserId).ToListAsync();
                        if (!userRoles.Any(r => r == StaticRoleNames.Host.Admin || r == StaticRoleNames.Host.ProjectAdmin))
                        {
                            userRoles.Add(StaticRoleNames.Host.ProjectAdmin);
                            var user = await WorkScope.GetAsync<User>(item.ProjectUser.UserId);
                            CheckErrors(await _userManager.SetRoles(user, userRoles.ToArray()));
                        }
                    }
                }

                //ProjectTargetUser
                if (!input.Users.Any(s => s.Type == ProjectUserType.Shadow))
                {//no shadow user => remove projectTargetUser
                    var ptus = await WorkScope.GetAll<ProjectTargetUser>().Where(s => s.ProjectId == input.Id)
                        .Select(s => s.Id).ToListAsync();
                    foreach (var ptuId in ptus)
                    {
                        await WorkScope.DeleteAsync<ProjectTargetUser>(ptuId);
                    }
                }
                else
                {
                    var currentProjectTargetUsers = await WorkScope.GetAll<ProjectTargetUser>()
                   .Where(s => s.ProjectId == input.Id).ToListAsync();
                    var currentTargetUserIds = currentProjectTargetUsers.Select(s => s.UserId).ToList();
                    var newTargetUserIds = input.ProjectTargetUsers.Select(s => s.UserId).ToList();

                    var deleteTargetUserIds = currentTargetUserIds.Except(newTargetUserIds);
                    var isDeleteTargetUsersInTimesheet = await WorkScope.GetRepo<MyTimesheet>()
                        .GetAllIncluding(s => s.ProjectTargetUser)
                        .AnyAsync(s => deleteTargetUserIds.Contains(s.ProjectTargetUser.UserId) && s.ProjectTargetUser.ProjectId == project.Id);
                    if (isDeleteTargetUsersInTimesheet)
                        throw new UserFriendlyException(String.Format("Users are logged timesheet so you can't remove"));

                    var insertProjectTargetUsers = input.ProjectTargetUsers.Where(x => !currentTargetUserIds.Contains(x.UserId));
                    var deleteProjectTargetUserIds = currentProjectTargetUsers.Where(s => !newTargetUserIds.Contains(s.UserId)).Select(s => s.Id).ToList();

                    var updateProjectTargetUsers = (from cpu in currentProjectTargetUsers
                                                    join cu in input.ProjectTargetUsers on cpu.UserId equals cu.UserId
                                                    select new
                                                    {
                                                        ProjectTargetUser = cpu,
                                                        Dto = cu
                                                    }).ToList();

                    foreach (var id in deleteProjectTargetUserIds)
                    {
                        await WorkScope.DeleteAsync<ProjectTargetUser>(id);
                    }

                    foreach (var pTargetUserDto in insertProjectTargetUsers)
                    {
                        var projectTargetUser = ObjectMapper.Map<ProjectTargetUser>(pTargetUserDto);
                        projectTargetUser.ProjectId = input.Id;
                        await WorkScope.InsertAsync<ProjectTargetUser>(projectTargetUser);
                    }

                    foreach (var item in updateProjectTargetUsers)
                    {
                        if (item.Dto.RoleName != item.ProjectTargetUser.RoleName)
                        {
                            item.ProjectTargetUser.RoleName = item.Dto.RoleName;
                            await WorkScope.UpdateAsync<ProjectTargetUser>(item.ProjectTargetUser);
                        }
                    }
                }
            }
            return input;
        }

        //show list Projects
        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_View, Ncc.Authorization.PermissionNames.Project_View_All)]
        public async Task<List<GetProjectDto>> GetAll(ProjectStatus? status, string search)
        {
            var isViewAll = await IsGrantedAsync(Ncc.Authorization.PermissionNames.Project_View_All);

            search = !string.IsNullOrEmpty(search) ? search.Trim().ToLower() : "";

            var qproject = WorkScope.GetAll<Project>()
                                    .Select(s => new
                                    {
                                        CustomerName = s.Customer.Name.ToLower(),
                                        s.Id,
                                        s.Name,
                                        s.Code,
                                        s.Status,
                                        s.ProjectType,
                                        s.TimeStart,
                                        s.TimeEnd
                                    })
                                    .Where(s => !status.HasValue || s.Status == status)
                                    .WhereIf(!string.IsNullOrWhiteSpace(search), s => s.Name.Contains(search)
                                           || s.CustomerName.Contains(search)
                                           || s.Code.Contains(search));

            var qprojectIds = qproject.Select(s => s.Id);

            List<long> projectIds = null;
            if (isViewAll)
            {
                projectIds = qprojectIds.ToList();
            }
            else
            {
                var myProjectIds = WorkScope.GetAll<ProjectUser>()
                                            .Where(s => s.UserId == AbpSession.UserId.Value)
                                            .Where(s => s.Type == ProjectUserType.PM)
                                            .Select(s => s.ProjectId)
                                            .Distinct();
                projectIds = qprojectIds.Where(s => myProjectIds.Contains(s)).ToList();
            }

            var PMs = WorkScope.GetAll<ProjectUser>()
                               .Where(x => x.Type == ProjectUserType.PM)
                               .Where(s => projectIds.Contains(s.ProjectId))
                               .Select(x => new { x.ProjectId, x.User.FullName })
                               .OrderBy(x => x.FullName)
                               .GroupBy(s => s.ProjectId)
                               .Select(s => new
                               {
                                   ProjectId = s.Key,
                                   PMs = s.Select(x => x.FullName).ToList()
                               }).AsNoTracking().AsEnumerable();

            var projectMembers = WorkScope.GetAll<ProjectUser>()
                                          .Where(s => projectIds.Contains(s.ProjectId))
                                          .Where(s => s.Type != ProjectUserType.DeActive)
                                          .Select(x => new { x.ProjectId })
                                          .GroupBy(s => s.ProjectId)
                                          .Select(s => new
                                          {
                                              ProjectId = s.Key,
                                              Count = s.Count()
                                          }).AsNoTracking().AsEnumerable();

            var projects = qproject.Where(s => projectIds.Contains(s.Id)).AsNoTracking().AsEnumerable();

            var results = (from p in projects
                           join members in projectMembers on p.Id equals members.ProjectId
                           join pm in PMs on p.Id equals pm.ProjectId
                           select new GetProjectDto
                           {
                               CustomerName = p.CustomerName,
                               Id = p.Id,
                               Name = p.Name,
                               Code = p.Code,
                               Status = p.Status,
                               ProjectType = p.ProjectType,
                               Pms = pm.PMs,
                               ActiveMember = members.Count,
                               TimeStart = p.TimeStart,
                               TimeEnd = p.TimeEnd
                           }).ToList();
            return results;
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_ChangeStatus)]
        public async System.Threading.Tasks.Task Inactive(EntityDto<long> input)
        {
            var project = await WorkScope.GetAsync<Project>(input.Id);
            if (project != null)
            {
                project.Status = ProjectStatus.Deactive;
                await WorkScope.GetRepo<Project, long>().UpdateAsync(project);
            }
            else
            {
                throw new UserFriendlyException(string.Format("Project is not exist"));
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_ChangeStatus)]
        public async System.Threading.Tasks.Task Active(EntityDto<long> input)
        {
            var project = await WorkScope.GetAsync<Project>(input.Id);
            if (project != null)
            {
                project.Status = ProjectStatus.Active;
                await WorkScope.UpdateAsync<Project>(project);
            }
            else
            {
                throw new UserFriendlyException(string.Format("Project is not exist"));
            }
        }

        [HttpDelete]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_Delete)]
        public async System.Threading.Tasks.Task Delete(EntityDto<long> input)
        {
            //check projectId exist

            //check mystimesheet exist
            var hasTimeSheet = await WorkScope.GetRepo<MyTimesheet, long>()
                                    .GetAllIncluding(s => s.ProjectTask)
                                    .AnyAsync(s => s.ProjectTask.ProjectId == input.Id);
            if (hasTimeSheet)
                throw new UserFriendlyException(string.Format("MyTimesheet is exist, you cann't delete Project"));

            await WorkScope.GetRepo<Project, long>().DeleteAsync(input.Id);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_ViewDetail)]
        public async Task<ProjectDto> Get(long input)
        {
            var hasProject = await WorkScope.GetAll<Project>().AnyAsync(s => s.Id == input);
            if (hasProject)
            {
                var result = (from p in WorkScope.GetAll<Project>()
                              join pt in WorkScope.GetAll<ProjectTask>() on p.Id equals pt.ProjectId into Tasks
                              join pu in WorkScope.GetAll<ProjectUser>() on p.Id equals pu.ProjectId into Users
                              join ptu in WorkScope.GetAll<ProjectTargetUser>() on p.Id equals ptu.ProjectId into ptus
                              where (p.Id == input)
                              select new ProjectDto
                              {
                                  Id = p.Id,
                                  Name = p.Name,
                                  TimeStart = p.TimeStart,
                                  TimeEnd = p.TimeEnd,
                                  Code = p.Code,
                                  Note = p.Note,
                                  Status = p.Status,
                                  ProjectType = p.ProjectType,
                                  CustomerId = p.CustomerId,
                                  KomuChannelId = p.KomuChannelId,
                                  IsNotifyToKomu = p.IsNotifyToKomu,
                                  IsNoticeKMSubmitTS = p.IsNoticeKMSubmitTS,
                                  IsNoticeKMRequestOffDate = p.IsNoticeKMRequestOffDate,
                                  IsNoticeKMApproveRequestOffDate = p.IsNoticeKMApproveRequestOffDate,
                                  IsNoticeKMRequestChangeWorkingTime = p.IsNoticeKMRequestChangeWorkingTime,
                                  IsNoticeKMApproveChangeWorkingTime = p.IsNoticeKMApproveChangeWorkingTime,
                                  isAllUserBelongTo = p.isAllUserBelongTo,
                                  Users = Users.Select(s => new ProjectUsersDto
                                  {
                                      Id = s.Id,
                                      UserId = s.UserId,
                                      Type = s.Type,
                                      IsTemp = s.IsTemp,
                                  }).ToList(),
                                  Tasks = Tasks.Select(s => new ProjectTaskDto
                                  {
                                      Id = s.Id,
                                      TaskId = s.TaskId,
                                      Billable = s.Billable
                                  }).ToList(),
                                  ProjectTargetUsers = ptus.Select(s => new ProjectTargetUserDto
                                  {
                                      Id = s.Id,
                                      RoleName = s.RoleName,
                                      UserId = s.UserId
                                  }).ToList()
                              }
                              ).FirstOrDefault();

                return result;
            }
            else
            {
                throw new UserFriendlyException(string.Format("Project isn't exist"));
            }
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_UpdateDefaultProjectTask)]
        public async System.Threading.Tasks.Task ClearDefaultProjectTask()
        {
            var userId = AbpSession.UserId.Value;
            var user = await WorkScope.GetAsync<User>(userId);
            user.DefaultProjectTaskId = null;
            await WorkScope.UpdateAsync(user);
        }

        [HttpPost]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Project_UpdateDefaultProjectTask)]
        public async System.Threading.Tasks.Task UpdateDefaultProjectTask(EntityDto<long> input)
        {
            var userId = AbpSession.UserId.Value;

            //var projectTask = (from pt in WorkScope.GetAll<ProjectTask>()
            //                   join pu in WorkScope.GetAll<ProjectUser>() on pt.ProjectId equals pu.ProjectId
            //                   where pu.UserId == userId && pu.Type != ProjectUserType.DeActive
            //                   select new
            //                   {
            //                       Id = pt.Id
            //                   }).ToList();

            //if (!projectTask.Any(cus => cus.Id == input.Id))
            //{
            //    throw new UserFriendlyException(string.Format("Project task id not exist " + input.Id));
            //}

            var user = await WorkScope.GetAsync<User>(userId);

            if (user.DefaultProjectTaskId != input.Id)
            {
                user.DefaultProjectTaskId = input.Id;
                await WorkScope.UpdateAsync(user);
            }
        }

        public async Task<List<ProjectIncludingTaskDto>> GetProjectsIncludingTasks()
        {
            var qProjectUserType = WorkScope.GetAll<ProjectUser>()
                  .Where(s => s.UserId == AbpSession.UserId.Value && s.Type != ProjectUserType.DeActive)
                  .Select(s => new
                  {
                      projectId = s.ProjectId,
                      s.Type
                  });

            var userId = AbpSession.UserId.Value;

            var defaultProjectTaskId = await WorkScope.GetAll<User>()
                    .Where(s => s.Id == userId)
                    .Select(s => s.DefaultProjectTaskId).FirstOrDefaultAsync();

            return await (from p in
                              from pp in WorkScope.GetAll<Project>().Where(s => s.Status == ProjectStatus.Active)
                              join c in WorkScope.GetAll<Customer>() on pp.CustomerId equals c.Id
                              select new { pp.Name, pp.Code, CustomerName = c.Name, pp.Id }

                          join put in qProjectUserType on p.Id equals put.projectId

                          join pt in
                                 from ptt in WorkScope.GetAll<ProjectTask>()
                                 join t in WorkScope.GetAll<Ncc.Entities.Task>() on ptt.TaskId equals t.Id
                                 select new { ptt.ProjectId, ProjectTaskId = ptt.Id, TaskName = t.Name, ptt.Billable }
                          on p.Id equals pt.ProjectId into pts

                          join ptu in
                                 from ptuu in WorkScope.GetAll<ProjectTargetUser>()
                                 join u in WorkScope.GetAll<User>() on ptuu.UserId equals u.Id
                                 select new { ProjectTargetUserId = ptuu.Id, ptuu.ProjectId, UserName = u.FullName }
                          on p.Id equals ptu.ProjectId into ptus

                          join pu in
                                from puu in WorkScope.GetAll<ProjectUser>()
                                    .Where(s => s.Project.Status == ProjectStatus.Active)
                                    .Where(s => s.Type == ProjectUserType.PM)
                                join u in WorkScope.GetAll<User>()
                                on puu.UserId equals u.Id
                                select new { PM = u.FullName, puu.ProjectId }
                          on p.Id equals pu.ProjectId into pus

                          select new ProjectIncludingTaskDto
                          {
                              Id = p.Id,
                              CustomerName = p.CustomerName,
                              ProjectName = p.Name,
                              ProjectCode = p.Code,
                              ProjectUserType = put.Type,
                              ListPM = pus.Select(pm => pm.PM).ToList(),
                              Tasks = pts.Select(s => new PTaskDto
                              {
                                  TaskName = s.TaskName,
                                  ProjectTaskId = s.ProjectTaskId,
                                  Billable = s.Billable,
                                  IsDefault = s.ProjectTaskId == defaultProjectTaskId ? true : false
                              }).ToList(),
                              TargetUsers = ptus.Select(s => new PTargetUserDto
                              {
                                  ProjectTargetUserId = s.ProjectTargetUserId,
                                  UserName = s.UserName
                              }).ToList()
                          }).ToListAsync();
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<List<GetProjectFilterDto>> GetFilter()
        {
            return await WorkScope.GetAll<Project>()
                .Where(s => s.Status == ProjectStatus.Active)
                .Select(s => new GetProjectFilterDto
                {
                    Id = s.Id,
                    Name = s.Name.Normalize(),
                    Code = s.Code
                }).ToListAsync();
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<List<GetProjectFilterDto>> GetProjectPM()
        {
            var userId = AbpSession.UserId.Value;
            return await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userId)
                .Where(s => s.Project.Status == ProjectStatus.Active)
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => new GetProjectFilterDto
                {
                    Id = s.ProjectId,
                    Name = s.Project.Name,
                    Code = s.Project.Customer.Name
                }).ToListAsync();
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<List<GetProjectFilterDto>> GetProjectWorkingTimePM()
        {
            var userId = AbpSession.UserId.Value;
            var isViewAllUserWorkingTime = await this.IsGrantedAsync(Ncc.Authorization.PermissionNames.ManageWorkingTime_ViewAll);

            return await WorkScope.GetAll<ProjectUser>()
                .Where(s => isViewAllUserWorkingTime || s.UserId == userId)
                .Where(s => s.Project.Status == ProjectStatus.Active)
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => new GetProjectFilterDto
                {
                    Id = s.ProjectId,
                    Name = s.Project.Name,
                    Code = s.Project.Customer.Name
                }).Distinct().OrderBy(s => s.Name).ToListAsync();
        }

        [HttpGet]
        [AbpAuthorize]
        public async Task<List<GetProjectFilterDto>> GetProjectUser()
        {
            var userId = AbpSession.UserId.Value;
            return await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userId)
                .Where(s => s.Project.Status == ProjectStatus.Active && s.Type != ProjectUserType.DeActive)
                .Select(s => new GetProjectFilterDto
                {
                    Id = s.ProjectId,
                    Name = s.Project.Name,
                    Code = s.Project.Customer.Name
                }).ToListAsync();
        }

        [HttpGet]
        public async Task<object> GetQuantityProject()
        {
            return await WorkScope.GetAll<Project>()
                                  .GroupBy(x => x.Status)
                                  .Select(x => new
                                  {
                                      Status = x.Key,
                                      Quantity = x.Count(),
                                  })
                                  .ToListAsync();
        }

        [HttpGet]
        public List<MyTimesheet> ProcessTempTimesheet(string projectCode)
        {
            var project = WorkScope.GetAll<Project>()
                .Any(x => x.Code == projectCode);

            if (project == default)
                throw new UserFriendlyException("Project not exist !");

            var listTempUsersInOut = _projectService.GetTempUsersInOutProjectHistory(projectCode);

            if (listTempUsersInOut == default)
                throw new UserFriendlyException("listTempUsersInOut null");

            var emails = listTempUsersInOut.Select(s => s.EmailAddress);
            var myTSs = WorkScope.GetAll<MyTimesheet>()
                .Select(s => new { TS = s, s.User.EmailAddress, s.ProjectTask.Project.Code })
                .Where(s => emails.Contains(s.EmailAddress))
                .Where(s => s.Code == projectCode)
                .ToList();

            var results = new List<MyTimesheet>();

            foreach (var item in listTempUsersInOut)
            {
                var startEndDates = DateTimeUtils.GetStartEndDates(item.ListTimeInOut);
                if (startEndDates == null || startEndDates.Count == 0)
                {
                    continue;
                }

                foreach (var startEndDate in startEndDates)
                {
                    var tempTSs = myTSs.Where(s => s.EmailAddress == item.EmailAddress)
                        .Where(s => s.TS.DateAt >= startEndDate.StartDate)
                        .Where(s => s.TS.DateAt.Date <= startEndDate.EndDate)
                        .Select(s => s.TS)
                        .ToList();

                    tempTSs.ForEach(s => s.IsTemp = true);

                    results.AddRange(tempTSs);
                }
            }

            CurrentUnitOfWork.SaveChanges();

            return results;
        }

        [HttpGet]
        public List<ProjectUser> ProcessCurrentTempProjectUser(string projectCode)
        {
            var projectId = WorkScope.GetAll<Project>()
                .Where(x => x.Code == projectCode)
                .Select(x => x.Id)
                .FirstOrDefault();

            if (projectId == default)
                throw new UserFriendlyException("Project not exist !");

            var listEmail = _projectService.GetCurrentTempEmailsInProject(projectCode);

            if (listEmail == null)
                throw new UserFriendlyException("listEmail null");

            var PUs = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.ProjectId == projectId)
                .Select(s => new { ProjectUser = s, s.User.EmailAddress })
                .Where(s => listEmail.Contains(s.EmailAddress))
                .Select(s => s.ProjectUser)
                .ToList();

            PUs.ForEach(s =>
            {
                s.IsTemp = true;
                s.LastModificationTime = DateTimeUtils.GetNow();
            });

            CurrentUnitOfWork.SaveChanges();

            return PUs;
        }

        [HttpGet]
        public List<ProjectUser> ProcessAllCurrentTempProjectUser()
        {
            var listCurrentTempPU = _projectService.GetAllCurrentTempProjectUser();

            if (listCurrentTempPU == null)
                throw new UserFriendlyException("listEmail null");

            var listEmail = listCurrentTempPU.Select(s => s.EmailAddress).ToList();

            var PUs = WorkScope.GetAll<ProjectUser>()
                .Select(s => new { ProjectUser = s, s.User.EmailAddress, s.Project.Code })
                .Where(s => listEmail.Contains(s.EmailAddress))
                .ToList();

            var q = from pu in PUs
                    from t in listCurrentTempPU
                    where (pu.EmailAddress == t.EmailAddress && pu.Code == t.ProjectCode)
                    select pu.ProjectUser;

            var tempPUs = q.ToList();

            tempPUs.ForEach(s =>
            {
                s.IsTemp = true;
                s.LastModificationTime = DateTimeUtils.GetNow();
            });

            CurrentUnitOfWork.SaveChanges();

            return tempPUs;
        }
    }

    public class StartEndDate
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}