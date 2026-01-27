using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;
using System.Linq;
using Timesheet.Timesheets.Timesheets.Dto;
using Microsoft.EntityFrameworkCore;
using Abp.Linq.Extensions;
using Ncc.Entities;
using Ncc.Authorization.Users;
using Ncc.IoC;
using Timesheet.Timesheets.Projects.Dto;
using Ncc.Configuration;
using Timesheet.APIs.Timesheets.Timesheets.Dto;

namespace Timesheet.Timesheets.TimesheetsSupervisor
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.TimesheetSupervision)]
    public class TimesheetsSupervisor : AppServiceBase
    {
        public TimesheetsSupervisor(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TimesheetSupervision_View)]
        public async Task<List<MyTimeSheetDto>> GetAll(GetTimesheetsInputDto input)
        {
            var OpenTalkID = Convert.ToInt64(await SettingManager.GetSettingValueAsync(AppSettingNames.ProjectTaskId));
            var qUsers = WorkScope.GetAll<User>()
             .Select(x => new
             {
                 x.Id,
                 x.EmailAddress
             });
            return await (from a in WorkScope.GetRepo<MyTimesheet>().GetAllIncluding(s => s.ProjectTask,
                                                                        s => s.ProjectTask.Task,
                                                                        s => s.ProjectTask.Project,
                                                                        s => s.ProjectTask.Project.Customer)
                          join pu in
                               from puu in WorkScope.GetAll<ProjectUser>()
                                   .Where(s => s.Type == ProjectUserType.PM)
                               join u in WorkScope.GetAll<User>()
                               on puu.UserId equals u.Id
                               select new { PM = u.FullName, puu.ProjectId }
                          on a.ProjectTask.ProjectId equals pu.ProjectId into pus
                          select new MyTimeSheetDto
                          {
                              Id = a.Id,
                              Status = a.Status,
                              WorkingTime = a.WorkingTime,
                              DateAt = a.DateAt,
                              User = a.User.Name + " " + a.User.Surname,
                              UserId = a.User.Id,
                              TaskName = a.ProjectTask.Task.Name,
                              TaskId = a.ProjectTask.TaskId,
                              ProjectTaskId = a.ProjectTaskId,
                              CustomerName = a.ProjectTask.Project.Customer.Name,
                              ProjectName = a.ProjectTask.Project.Name,
                              MytimesheetNote = a.Note,
                              ProjectCode = a.ProjectTask.Project.Code,
                              ProjectId = a.ProjectTask.Project.Id,
                              IsCharged = a.IsCharged,
                              TypeOfWork = a.TypeOfWork,
                              IsTemp = a.IsTemp,
                              ListPM = pus.Select(pm => pm.PM).ToList(),
                              LastModificationTime = a.LastModificationTime,
                              IsUnlockedByEmployee = a.IsUnlockedByEmployee,
                              projectTargetUser = a.ProjectTargetUser.User.FullName,
                              workingTimeTargetUser = a.TargetUserWorkingTime,
                              openTalkTime = WorkScope.GetAll<OpenTalk>().Where(s=>s.UserId == a.User.Id && a.DateAt == s.DateAt.Date).Select(s=>s.totalTime).FirstOrDefault(),
                              LastModifierUser = qUsers.Where(x => x.Id == a.LastModifierUserId).Select(x => x.EmailAddress).FirstOrDefault()
                          })
                           .WhereIf(input.Status.HasValue && input.Status >= 0, s => s.Status == input.Status)
                           .WhereIf(input.StartDate != null, s => s.DateAt >= input.StartDate)
                           .WhereIf(input.EndDate != null, s => s.DateAt.Date <= input.EndDate)
                           .WhereIf(input.ProjectId != null, s => s.ProjectId == input.ProjectId)
                           .WhereIf(input.OpentalkTime.HasValue, s => s.ProjectTaskId == OpenTalkID)
                           .WhereIf(input.OpentalkTime.HasValue, s => input.OpentalkTimeType.Value ? s.openTalkTime >= input.OpentalkTime : s.openTalkTime < input.OpentalkTime)
                           .WhereIf(input.TypeOfWork.HasValue, s => s.TypeOfWork == input.TypeOfWork.Value)
                           .WhereIf(input.IsCharged.HasValue, s => s.IsCharged == input.IsCharged.Value)
                           .WhereIf(input.UserId != null, s => s.UserId == input.UserId)
                           .ToListAsync();
        }
        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.TimesheetSupervision_View)]
        public async Task<object> GetQuantityTimesheetSupervisorStatus(GetTimesheetsInputDto input)
        {
            var OpenTalkID = Convert.ToInt64(await SettingManager.GetSettingValueAsync(AppSettingNames.ProjectTaskId));
            var query = WorkScope.GetAll<MyTimesheet>()
                                 .Where(x => !input.StartDate.HasValue || x.DateAt.Date >= input.StartDate)
                                 .Where(x => !input.EndDate.HasValue || x.DateAt.Date <= input.EndDate)
                                 .WhereIf(input.OpentalkTime.HasValue, x => x.ProjectTaskId == OpenTalkID)
                                 .WhereIf(input.ProjectId.HasValue, x => x.ProjectTask.ProjectId == input.ProjectId)
                                 .WhereIf(input.UserId.HasValue, x => x.UserId == input.UserId)
                                 .Select (x => new
                                 {
                                     Status = x.Status,
                                     IsCharged = x.IsCharged,
                                     TypeOfWork = x.TypeOfWork,
                                     openTalkTime = !input.OpentalkTime.HasValue ? 0 : WorkScope.GetAll<OpenTalk>().Where(s => s.UserId == x.UserId && x.DateAt.Date == s.DateAt.Date).Select(s => s.totalTime).FirstOrDefault()
                                 })
                                 .WhereIf(input.OpentalkTime.HasValue, x => input.OpentalkTimeType.Value ? x.openTalkTime >= input.OpentalkTime : x.openTalkTime < input.OpentalkTime)
                                 .WhereIf(input.TypeOfWork.HasValue, x => x.TypeOfWork == input.TypeOfWork.Value)
                                 .WhereIf(input.IsCharged.HasValue, x => x.IsCharged == input.IsCharged.Value)
                                 .GroupBy(x => x.Status).Select(x => new
                                 {
                                     Status = x.Key,
                                     Quantity = x.Count()
                                 });

            var statusQuantityList = await query.ToDictionaryAsync(s => s.Status);
            TimesheetStatus[] statuses = new TimesheetStatus[] {
                TimesheetStatus.All,
                TimesheetStatus.Approve,
                TimesheetStatus.None,
                TimesheetStatus.Pending,
                TimesheetStatus.Reject };

            var result = new List<Object>();
            foreach (TimesheetStatus status in statuses)
            {
                var quantity = statusQuantityList.ContainsKey(status) ? statusQuantityList[status].Quantity : 0;
                result.Add(new { Status = status, Quantity = quantity });
            }
            return result;
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_NormalWorking_LockUnlockTimesheet)]
        public async System.Threading.Tasks.Task UnlockPM(long id)
        {
            var projectPM = await WorkScope.GetAll<ProjectUser>().Where(s => s.ProjectId == id && s.Type == ProjectUserType.PM).Select(s => s.UserId).ToListAsync();
            foreach (var pm in projectPM)
            {
                var isExist = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == pm && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet);
                if (!isExist)
                {
                    var item = new UnlockTimesheet
                    {
                        UserId = pm,
                        Type = LockUnlockTimesheetType.ApproveRejectTimesheet
                    };
                    await WorkScope.GetRepo<UnlockTimesheet, long>().InsertAsync(item);
                }
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_NormalWorking_LockUnlockTimesheet)]
        public async System.Threading.Tasks.Task LockPM(long id)
        {
            var projectPM = await WorkScope.GetAll<ProjectUser>().Where(s => s.ProjectId == id && s.Type == ProjectUserType.PM).Select(s => s.UserId).ToListAsync();
            foreach (var pm in projectPM)
            {
                var isExist = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == pm && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet);
                if (isExist)
                {
                    var pmu = await WorkScope.GetAll<UnlockTimesheet>().Where(s => s.UserId == pm && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet)
                                        .Select(s => s.Id).FirstOrDefaultAsync();
                    await WorkScope.GetRepo<UnlockTimesheet, long>().DeleteAsync(pmu);
                }
            }
        }
    }
}
