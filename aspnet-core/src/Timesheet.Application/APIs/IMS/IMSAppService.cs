using Abp.Authorization;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.DayOffs.Dto;
using Timesheet.APIs.MyAbsenceDays.Dto;
using Timesheet.APIs.OverTimeHours;
using Timesheet.APIs.RequestDays;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.NCCAuthen;
using Timesheet.Services.HRMv2;
using Timesheet.Timesheets.Projects.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.IMS
{
    public class IMSAppService : AppServiceBase
    {
        private readonly RequestDayAppService _requestDayAppService;
        public IMSAppService(RequestDayAppService requestDayAppService, IWorkScope workScope) : base(workScope)
        {
            _requestDayAppService = requestDayAppService;
        }

        [AbpAllowAnonymous]
        [NccAuthentication]
        public async Task<List<ProjectIncludingTaskDto>> GetProjectsIncludingTasksByUserEmailIMS(string email)
        {
            var qProjectUserType = WorkScope.GetAll<ProjectUser>()
                  .Where(s => s.User.EmailAddress == email && s.Type != ProjectUserType.DeActive)
                  .Select(s => new
                  {
                      projectId = s.ProjectId,
                      s.Type
                  });

            var defaultProjectTaskId = await WorkScope.GetAll<User>()
                    .Where(s => s.EmailAddress == email)
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

        [AbpAllowAnonymous]
        [NccAuthentication]
        public async Task<MyRequestDto> SubmitToPendingNewByIMS(MyRequestDto input, string email)
        {
            var userId = WorkScope.GetAll<User>()
                .Where(s => s.EmailAddress == email)
                .Select(s => s.Id).FirstOrDefault();

            var requester = GetSessionUserInfoDtoByUserId(userId);

            return await _requestDayAppService.ProcessSubmitToPendingNew(input, userId, requester);
        }

        [AbpAllowAnonymous]
        [NccAuthentication]
        public async Task<List<AbsenceTypeDto>> GetAllAbsenceTypeByIMS()
        {
            return await WorkScope.GetAll<DayOffType>().ProjectTo<AbsenceTypeDto>().ToListAsync();
        }
    }
}
