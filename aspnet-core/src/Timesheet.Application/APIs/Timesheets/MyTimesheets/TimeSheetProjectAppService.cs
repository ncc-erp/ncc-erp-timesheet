using Microsoft.AspNetCore.Mvc;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Entities;
using Timesheet.Timesheets.MyTimesheets.Dto;
using System.Linq;
using Ncc;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Abp.UI;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.Authorization;
using Timesheet.DomainServices;
using Ncc.Authorization.Roles;
using Ncc.IoC;

namespace Timesheet.Timesheets.MyTimesheets
{

    [AbpAuthorize]
    public class TimeSheetProjectAppService : AppServiceBase
    {
        public TimeSheetProjectAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpGet]
        public async Task<List<TimeStatisticTaskDto>> GetTimeSheetStatisticTasks(long projectId, DateTime? startDate, DateTime? endDate)
        {
            var timesheetTasks = await (WorkScope.GetRepo<MyTimesheet>().GetAllIncluding(s => s.ProjectTask, s => s.ProjectTask.Task)
                .Where(s => s.Status == TimesheetStatus.Approve)
                .Where(s => s.ProjectTask.ProjectId == projectId)
                .Where(s => !startDate.HasValue || s.DateAt >= startDate.Value)
                .Where(s => !endDate.HasValue || s.DateAt <= endDate.Value)
                .GroupBy(s => new { s.ProjectTask.TaskId, s.ProjectTask.Task.Name, s.ProjectTask.Billable })
                .Select(s => new
                {
                    s.Key.TaskId,
                    TotalWorkingTime = s.Sum(x => x.WorkingTime),
                    BillableWorkingTime = s.Sum(x => (x.ProjectTask.Billable
                                                     && (x.TypeOfWork == TypeOfWork.NormalWorkingHours
                                                     || (x.TypeOfWork == TypeOfWork.OverTime && x.IsCharged)))
                                                       ? x.WorkingTime : 0)
                })).ToListAsync();


            var projectTasks = await WorkScope.GetRepo<ProjectTask>().GetAllIncluding(s => s.Task)
                                        .Where(s => s.ProjectId == projectId)
                                        .Select(s => new
                                        {
                                            s.TaskId,
                                            s.Task.Name,
                                            s.Billable
                                        }).ToListAsync();

            return (from pt in projectTasks
                    join ts in timesheetTasks on pt.TaskId equals ts.TaskId into tss
                    from t in tss.DefaultIfEmpty()
                    select new TimeStatisticTaskDto
                    {
                        TaskId = pt.TaskId,
                        TaskName = pt.Name,
                        Billable = pt.Billable,
                        BillableWorkingTime = t != null ? t.BillableWorkingTime : 0,
                        TotalWorkingTime = t != null ? t.TotalWorkingTime : 0
                    }).OrderByDescending(s => s.TotalWorkingTime).ToList();
        }

        [HttpGet]
        public async Task<List<TimeStatisticMemberDto>> GetTimeSheetStatisticTeams(long projectId, DateTime? startDate, DateTime? endDate)
        {
            var timesheetUsers = await (WorkScope.GetRepo<MyTimesheet>().GetAllIncluding(s => s.User, s => s.ProjectTask))
                .Where(s => s.Status == TimesheetStatus.Approve)
                .Where(s => s.ProjectTask.ProjectId == projectId)
                .Where(s => !startDate.HasValue || s.DateAt >= startDate.Value)
                .Where(s => !endDate.HasValue || s.DateAt <= endDate.Value)
                .GroupBy(s => new { s.UserId, s.User.FullName })
                .Select(s => new
                {
                    UserID = s.Key.UserId,   
                    TotalWorkingTime = s.Sum(x => x.WorkingTime),
                    BillableWorkingTime = s.Sum(x => (x.ProjectTask.Billable
                                                    && (x.TypeOfWork == TypeOfWork.NormalWorkingHours
                                                        || (x.TypeOfWork == TypeOfWork.OverTime && x.IsCharged)))
                                                        ? x.WorkingTime : 0)
                }).ToListAsync();

            var projectUsers = await (from pu in WorkScope.GetRepo<ProjectUser>()
                                                          .GetAllIncluding(s => s.User)
                                                          .Where(s => s.ProjectId == projectId)
                                      select new
                                      {
                                          UserID = pu.UserId,
                                          pu.User.FullName,
                                          projectUserType = pu.Type
                                      }).ToListAsync();

            return (from pu in projectUsers
                    join tu in timesheetUsers
                    on pu.UserID equals tu.UserID into tusers
                    from user in tusers.DefaultIfEmpty()
                    select new TimeStatisticMemberDto
                    {
                        UserID = pu.UserID,
                        UserName = pu.FullName,
                        projectUserType = pu.projectUserType,
                        TotalWorkingTime = user == null ? 0 : user.TotalWorkingTime,
                        BillableWorkingTime = user == null ? 0 : user.BillableWorkingTime
                    }).OrderByDescending(s => s.TotalWorkingTime).ToList();
        }

        [HttpGet]
        public async Task<List<ReportTimesheetDto>> ExportBillableTimesheets(long projectId, DateTime? startDate, DateTime? endDate)
        {
            return await (from mts in
                              from mtss in WorkScope.GetAll<MyTimesheet>()

                              join u in WorkScope.GetAll<User>() on mtss.UserId equals u.Id
                              join pt in WorkScope.GetAll<ProjectTask>() on mtss.ProjectTaskId equals pt.Id
                              join t in WorkScope.GetAll<Ncc.Entities.Task>() on pt.TaskId equals t.Id

                              where (mtss.Status == TimesheetStatus.Approve)
                              where (!startDate.HasValue || mtss.DateAt >= startDate.Value)
                              where (!endDate.HasValue || mtss.DateAt <= endDate.Value)
                              where (pt.Billable && pt.ProjectId == projectId)
                              where (mtss.TypeOfWork == TypeOfWork.NormalWorkingHours || (mtss.TypeOfWork == TypeOfWork.OverTime && mtss.IsCharged))

                              select new {
                                  mtss.Id,
                                  UserName = u.FullName,
                                  mtss.ProjectTargetUserId,
                                  mtss.Status,
                                  mtss.DateAt,
                                  TaskName = t.Name,
                                  mtss.TypeOfWork,
                                  mtss.WorkingTime,
                                  mtss.TargetUserWorkingTime,
                                  mtss.Note
                              }

                          join ptu1 in
                                from ptuu in WorkScope.GetAll<ProjectTargetUser>()
                                join u in WorkScope.GetAll<User>() on ptuu.UserId equals u.Id
                                select new { TargetUserName = u.FullName, ProjectTargetUserId = ptuu.Id, ptuu.RoleName }
                          on mts.ProjectTargetUserId equals ptu1.ProjectTargetUserId into ptus
                          from ptu in ptus.DefaultIfEmpty().Take(1)


                          select new ReportTimesheetDto
                          {
                              Id = mts.Id,
                              UserName = mts.UserName,
                              DateAt = mts.DateAt,
                              TaskName = mts.TaskName,
                              Note = mts.Note,
                              TypeOfWork = mts.TypeOfWork,
                              WorkingTime = mts.WorkingTime,
                              TargetUserWorkingTime = mts.TargetUserWorkingTime,
                              RoleName = ptu != null ? ptu.RoleName : "",
                              TargetUserName = ptu != null ? ptu.TargetUserName : mts.UserName,
                              IsShadow = ptu != null
                          }).ToListAsync();
        }
    }
}

