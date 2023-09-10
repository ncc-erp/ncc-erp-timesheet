using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.Entities;
using Timesheet.Timesheets.Timesheets.Dto;
using System.Linq;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.Authorization;
using Ncc.Entities;
using Ncc.Authorization.Users;
using Abp.BackgroundJobs;
using Timesheet.BackgroundJob;
using System.Text;
using Abp.UI;
using Timesheet.APIs.Timesheets.Timesheets.Dto;
using Abp.Domain.Entities;
using Abp.Application.Services.Dto;
using Timesheet.Uitls;
using Timesheet.DomainServices;
using System.Globalization;
using Ncc.Configuration;
using Ncc.IoC;
using Abp.Extensions;

namespace Timesheet.Timesheets.Timesheets
{

    [AbpAuthorize(Ncc.Authorization.PermissionNames.Timesheet)]
    public class TimesheetAppService : AppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ICommonServices _commonService;
        public TimesheetAppService(IBackgroundJobManager backgroundJobManager, ICommonServices commonService, IWorkScope workScope) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _commonService = commonService;
        }
        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timesheet_View)]
        public async Task<List<MyTimeSheetDto>> GetAll(DateTime? startDate, DateTime? endDate, TimesheetStatus status, long? projectId, HaveCheckInFilter? checkInFilter, long? branchId = null, string searchText = "")
        {
            var dayOffSettings = await WorkScope.GetAll<DayOffSetting>()
             .Where(s => s.DayOff.Date >= startDate && s.DayOff.Date <= endDate)
             .Select(s => s.DayOff).ToListAsync();

            var projectIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == AbpSession.UserId.Value && s.Type == ProjectUserType.PM)
                .Where(s => !projectId.HasValue || s.ProjectId == projectId)
                .Select(s => s.ProjectId).ToListAsync();

            var userIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => projectIds.Contains(s.ProjectId))
                .Select(s => s.UserId).Distinct().ToListAsync();


            var absencedays = await WorkScope.GetAll<AbsenceDayDetail>()
                 .Where(s => !startDate.HasValue || s.DateAt >= startDate.Value.Date)
                 .Where(s => !endDate.HasValue || s.DateAt.Date <= endDate)
                 .Where(s => userIds.Contains(s.Request.UserId))
                 .Where(s => s.Request.Status == RequestStatus.Approved)
                 .Where(s => s.Request.Type == RequestType.Off)
                 .Select(s => new
                 {
                     UserId = s.Request.UserId,
                     DateAt = s.DateAt,
                     Hour = s.Hour
                 })
                  .ToListAsync();

            var q = from a in WorkScope.GetRepo<MyTimesheet>().GetAllIncluding(
                       s => s.ProjectTask,
                       s => s.ProjectTask.Task,
                       s => s.ProjectTask.Project,
                       s => s.ProjectTask.Project.Customer
                       )
                    where (status == TimesheetStatus.All)
                    || (a.Status == status)
                    where (userIds.Contains(a.UserId))
                    where (!startDate.HasValue || a.DateAt >= startDate)
                    where (!endDate.HasValue || a.DateAt.Date <= endDate)
                    where (projectIds.Contains(a.ProjectTask.ProjectId))
                    where (searchText == null || a.User.EmailAddress.Contains(searchText) || a.User.UserName.Contains(searchText) || a.User.FullName.Contains(searchText))
                    where (branchId == null || a.User.BranchId ==  branchId)
                    select new MyTimeSheetDto
                    {
                        Id = a.Id,
                        Status = a.Status,
                        WorkingTime = a.WorkingTime,
                        DateAt = a.DateAt,
                        User = a.User.Name + " " + a.User.Surname,
                        EmailAddress = a.User.EmailAddress,
                        UserId = a.User.Id,
                        AvatarPath = a.User.AvatarPath,
                        //Level = a.User.Level,
                        Type = a.User.Type,
                        TaskName = a.ProjectTask.Task.Name,
                        TaskId = a.ProjectTask.TaskId,
                        CustomerName = a.ProjectTask.Project.Customer.Name,
                        ProjectName = a.ProjectTask.Project.Name,
                        MytimesheetNote = a.Note,
                        ProjectCode = a.ProjectTask.Project.Code,
                        ProjectId = a.ProjectTask.Project.Id,
                        IsCharged = a.IsCharged,
                        TypeOfWork = a.TypeOfWork,
                        IsTemp = a.IsTemp,
                        IsUserInProject = true, // projectIds.Contains(a.ProjectTask.ProjectId)
                        LastModificationTime = a.LastModificationTime,
                        BranchColor = a.User.Branch.Color,
                        BranchDisplayName = a.User.Branch.DisplayName,
                        OffHour = absencedays.Where(s => s.DateAt.Date == a.DateAt.Date && s.UserId == a.User.Id).Select(h => h.Hour).Sum(),
                        IsOffDay = DateTimeUtils.IsOffDay(dayOffSettings, a.DateAt),
                        IsUnlockedByEmployee = a.IsUnlockedByEmployee
                    };
            var query = await q.OrderBy(i => i.EmailAddress).ThenByDescending(s => s.DateAt).ToListAsync();
            var listTimekeeping = WorkScope.GetAll<Timekeeping>()
            .Select(s => new
            {
                UserId = s.UserId,
                CheckIn = s.CheckIn,
                CheckOut = s.CheckOut,
                DateAt = s.DateAt.Date
            })
            .Where(s => !startDate.HasValue || s.DateAt >= startDate)
            .Where(s => !endDate.HasValue || s.DateAt.Date <= endDate)
            .Where(s => s.UserId.HasValue)
            .ToList();

            foreach (var item in query)
            {
                var timekeepingByUserAtDate = listTimekeeping
                    .Where(x => x.UserId == item.UserId)
                    .Where(s => s.DateAt == item.DateAt.Date)
                    .FirstOrDefault();
                item.CheckIn = timekeepingByUserAtDate?.CheckIn;
                item.CheckOut = timekeepingByUserAtDate?.CheckOut;
            }

            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckIn)
            {
                return query.Where(s => s.CheckIn != null && s.CheckIn != "").ToList();
            }

            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckOut)
            {
                return query.Where(s => s.CheckOut != null && s.CheckOut != "").ToList();
            }

            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckInAndHaveCheckOut)
            {
                return query.Where(s => s.CheckIn != null && s.CheckIn != "" && s.CheckOut != null && s.CheckOut != "").ToList();
            }

            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckInOrHaveCheckOut)
            {
                return query.Where(s => (s.CheckIn != null && s.CheckIn != "") || (s.CheckOut != null && s.CheckOut != "")).ToList();
            }

            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.NoCheckInAndNoCheckOut)
            {
                return query.Where(s => (s.CheckIn == null || s.CheckIn == "") && (s.CheckOut == null || s.CheckOut == "")).ToList();
            }

            return query;
        }

        [HttpPost]
        public async Task<List<TimeSheetWarningDto>> GetTimesheetWarning(long[] myTimesheetIds)
        {
            var listMyTimesheetByIds = WorkScope.GetAll<MyTimesheet>()
                    .Where(s => myTimesheetIds.Contains(s.Id))
                    .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                    .Select(s => new TimeSheetWarningDto
                    {
                        Id = s.Id,
                        WorkingTime = s.WorkingTime,
                        DateAt = s.DateAt,
                        TaskName = s.ProjectTask.Task.Name,
                        MytimesheetNote = s.Note,
                        ProjectName = s.ProjectTask.Project.Name,
                        UserId = s.User.Id,
                        EmailAddress = s.User.EmailAddress,
                        Status = s.Status
                    });

            DateTime startDate = listMyTimesheetByIds.OrderBy(s => s.DateAt).Select(s => s.DateAt).FirstOrDefault();
            DateTime endDate = listMyTimesheetByIds.OrderBy(s => s.DateAt).Select(s => s.DateAt).LastOrDefault();

            var dayOffSettings = await WorkScope.GetAll<DayOffSetting>()
           .Where(s => s.DayOff.Date >= startDate && s.DayOff.Date <= endDate)
           .Select(s => s.DayOff).ToListAsync();

            var listUserId = listMyTimesheetByIds.Select(s => s.UserId).ToList();

            var getAllOff = from t in WorkScope.GetAll<AbsenceDayDetail>()
                            .Where(s => s.DateAt.Date >= startDate)
                            .Where(s => s.DateAt.Date <= endDate)
                            .Where(s => listUserId.Contains(s.Request.UserId))
                            .Where(s => s.Request.Status == RequestStatus.Approved)
                            .Where(s => s.Request.Type == RequestType.Off)
                            select new Dto.RequestDetail
                            {
                                CreatorUserId = t.Request.UserId,
                                DateAt = t.DateAt,
                                Hour = t.Hour,
                            };

            var listMyTimesheetByUserIds = WorkScope.GetAll<MyTimesheet>()
                   .Where(s => listUserId.Contains(s.UserId))
                   .Where(s => !myTimesheetIds.Contains(s.Id))
                   .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                   .Where(s => s.DateAt.Date >= startDate)
                   .Where(s => s.DateAt.Date <= endDate)
                   .Select(s => new TimeSheetWarningDto
                   {
                       Id = s.Id,
                       WorkingTime = s.WorkingTime,
                       DateAt = s.DateAt,
                       TaskName = s.ProjectTask.Task.Name,
                       MytimesheetNote = s.Note,
                       ProjectName = s.ProjectTask.Project.Name,
                       UserId = s.User.Id,
                       EmailAddress = s.User.EmailAddress,
                       Status = s.Status,
                       HourOff = getAllOff.Where(h => h.DateAt == s.DateAt)
                                      .Where(h => h.CreatorUserId == s.User.Id)
                                      .Select(h => h.Hour).Sum()
                   }).ToList();

            var listTimesheetWarningByUserDateAt = listMyTimesheetByIds.GroupBy(s => new
            {
                s.DateAt,
                s.UserId
            }).Select(s => new TotalWorkingTimeUserAtDate
            {
                UserId = s.Key.UserId,
                DateAt = s.Key.DateAt,
                TotalWorkingTime = s.Sum(x => x.WorkingTime),
                TotalWorkingTimeDateAt = listMyTimesheetByUserIds.Where(h => h.DateAt == s.Key.DateAt)
                          .Where(h => h.UserId == s.Key.UserId)
                          .Select(h => h.WorkingTime).Sum(),
                HourOff = getAllOff.Where(h => h.DateAt == s.Key.DateAt)
                          .Where(h => h.CreatorUserId == s.Key.UserId)
                          .Select(h => h.Hour).Sum(),
                IsOffDay = DateTimeUtils.IsOffDay(dayOffSettings, s.Key.DateAt)
            }).ToList();

            listTimesheetWarningByUserDateAt = listTimesheetWarningByUserDateAt
               .Where(s => s.IsThanDefaultWorkingHourPerDay || s.IsOffDay || s.IsThanDefaultWorkingHourPerSaturday)
               .ToList();


            var resultListTimesheetWarningByUserDateAt = from s in listMyTimesheetByIds
                                                         join t in listTimesheetWarningByUserDateAt
                                                         on new { s.UserId, s.DateAt } equals new { t.UserId, t.DateAt }
                                                         select new TimeSheetWarningDto
                                                         {
                                                             Id = s.Id,
                                                             WorkingTime = s.WorkingTime,
                                                             DateAt = s.DateAt,
                                                             TaskName = s.TaskName,
                                                             MytimesheetNote = s.MytimesheetNote,
                                                             ProjectName = s.ProjectName,
                                                             UserId = s.UserId,
                                                             EmailAddress = s.EmailAddress,
                                                             HourOff = t.HourOff,
                                                             TotalWorkingTimeDateAt = t.TotalWorkingTimeDateAt,
                                                             Status = s.Status
                                                         };
            if (listTimesheetWarningByUserDateAt.Count() != 0)
            {
                var result = resultListTimesheetWarningByUserDateAt.ToList();
                foreach (var myTimesheetByUserIds in listMyTimesheetByUserIds)
                {
                    var workingHourDateAt = resultListTimesheetWarningByUserDateAt
                        .Where(s => s.UserId == myTimesheetByUserIds.UserId && s.DateAt.Date == myTimesheetByUserIds.DateAt.Date)
                        .Sum(s => s.TotalWorkingTimeDateAt - s.HourOff) + (myTimesheetByUserIds.TotalWorkingTimeDateAt - myTimesheetByUserIds.HourOff);
                    if (workingHourDateAt > 8)
                    {
                        result.Add(myTimesheetByUserIds);
                    }
                }
                return result.OrderBy(i => i.EmailAddress).ThenByDescending(s => s.DateAt)
                    .ToList();

            }

            return resultListTimesheetWarningByUserDateAt.OrderBy(i => i.EmailAddress).ThenByDescending(s => s.DateAt).ToList();
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timesheet_Approval)]
        public async System.Threading.Tasks.Task<object> ApproveTimesheets(long[] myTimesheetIds)
        {
            int successTS = 0;
            int failTS = 0;
            var lockDate = _commonService.getlockDatePM();
            var userId = AbpSession.UserId.Value;
            //var mtss = await WorkScope.GetAll<MyTimesheet>().Where(s => myTimesheetIds.Contains(s.Id)).ToListAsync();
            var projectIds = WorkScope.GetAll<ProjectUser>()
               .Where(s => s.UserId == AbpSession.UserId && s.Type == ProjectUserType.PM)
               .Select(s => new { s.ProjectId });

            var qMyTimeSheets = from mts in WorkScope.GetAll<MyTimesheet>().Where(s => myTimesheetIds.Contains(s.Id))
                                join pt in WorkScope.GetAll<ProjectTask>() on mts.ProjectTaskId equals pt.Id
                                select new { mts, pt.ProjectId };

            var mtss = await (from mts in qMyTimeSheets
                              join pu in projectIds on mts.ProjectId equals pu.ProjectId
                              select mts.mts).ToListAsync();

            //var hackMtsIds = myTimesheetIds.Except(mtss.Select(s => s.Id));
            //if (hackMtsIds != null && hackMtsIds.Count() > 0)
            //{
            //    throw new UserFriendlyException(String.Format("You are not Project Manager of these Timesheet Ids: {0}", String.Join(", ", hackMtsIds)));
            //}

            myTimesheetIds = mtss.Select(s => s.Id).ToArray();
            // mail
            // get receiver

            var timesheetByUserByProject = await WorkScope.GetRepo<MyTimesheet>()
                .GetAllIncluding(s => s.ProjectTask.Task, s => s.ProjectTask.Project, s => s.User)
                .Where(s => myTimesheetIds.Contains(s.Id))
                .GroupBy(s => new { s.UserId, s.User.EmailAddress })
                .Select(s => new
                {
                    s.Key.UserId,
                    UserEmail = s.Key.EmailAddress,
                    Project = s.GroupBy(x => new { x.ProjectTask.Project.Id, x.ProjectTask.Project.Name, x.ProjectTask.Project.Code })
                    .Select(t => new
                    {
                        ProjectName = t.Key.Name,
                        ProjectCode = t.Key.Code,
                        Timesheets = t.Select(u => new
                        {
                            u.Id,
                            TaskName = u.ProjectTask.Task.Name,
                            Note = u.Note,
                            DateAt = u.DateAt,
                            TypeOfWork = u.TypeOfWork,
                            Charged = u.IsCharged,
                            WorkingTime = u.WorkingTime,
                            IsUnlockedByEmployee = u.IsUnlockedByEmployee ?? false,
                        })
                    })
                }
                ).ToListAsync();

            var approverName = (await WorkScope.GetAsync<User>(AbpSession.UserId.Value)).FullName;
            var isUnlockPM = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == AbpSession.UserId.Value && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet);
            var myApproveTimesheetIds = new List<long>();
            var enableNotify = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailTimesheet);
            foreach (var user in timesheetByUserByProject)
            {
                //var isUnlockUser = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == user.UserId && s.Type == LockUnlockTimesheetType.MyTimesheet);
                var emails = new List<string>() { user.UserEmail };
                var mailBody = new StringBuilder();
                int successTSUser = 0;
                foreach (var project in user.Project)
                {
                    mailBody.Append($@"<div>
                                        <b>Project:</b>
                                        [{project.ProjectCode}]{project.ProjectName}
                                    </div>
                                    <div>
                                        <b>Timesheets:</b>
                                    </div> ");
                    var timesheetTable = new StringBuilder();
                    foreach (var timesheet in project.Timesheets)
                    {
                        if (timesheet.DateAt > lockDate || isUnlockPM || timesheet.IsUnlockedByEmployee)
                        {
                            myApproveTimesheetIds.Add(timesheet.Id);
                            timesheetTable.Append($@"
                                <tr>
                                <td>{timesheet.TaskName}</td>
                                <td>{timesheet.Note}</td>
                                <td>{timesheet.DateAt.ToString("yyyy'-'MM'-'dd")}</td>    
                                <td>{(timesheet.TypeOfWork == TypeOfWork.NormalWorkingHours ? "NormalWorking" : "OverTime")}</td>
                                <td>{(timesheet.Charged ? "Charged" : "")}</td>
                                <td>{TimeSpan.FromMinutes(timesheet.WorkingTime).ToString(@"hh\:mm")}</td>
                                </tr>");
                            successTS++;
                            successTSUser++;
                        }
                        else
                        {
                            failTS++;
                        }
                    }
                    if (successTS == 0 && failTS > 0)
                    {
                        throw new UserFriendlyException("PM hãy vào ims.nccsoft.vn để unlock timesheet!");
                    }
                    mailBody.Append($@"<table border='1'>
                                    <thead>
                                        <tr>
                                            <td>Task name</td>
                                            <td>Note</td>
                                            <td>Date at</td>
                                            <td>Type of work </td>
                                            <td>Charged</td>
                                            <td>Working time</td>
                                        </tr>
                                    </thead>
                                <tbody>{timesheetTable}</tbody>
                                </table>
                                <hr>");
                }
                if (successTSUser > 0 && enableNotify == "true")
                {
                    await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                    {
                        TargetEmails = emails,
                        Body = mailBody.ToString(),
                        Subject = $"{approverName} has approved your timesheets"
                    }, BackgroundJobPriority.High, new TimeSpan(TimeSpan.TicksPerMinute)
                    );
                }
            }

            if (myApproveTimesheetIds.Count > 0)
            {
                var listApprove = await WorkScope.GetAll<MyTimesheet>().Where(s => myApproveTimesheetIds.Contains(s.Id)).ToListAsync();
                foreach (var item in listApprove)
                {
                    item.Status = TimesheetStatus.Approve;
                }
                await WorkScope.UpdateRangeAsync(listApprove);
            }
            else if (failTS > 0)
            {
                throw new UserFriendlyException("PM hãy vào ims.nccsoft.vn để unlock timesheet");
            }

            return new
            {
                Success = string.Format(" - Success {0} timesheets.", successTS),
                SuccessCount = successTS,
                FailedCount = failTS,
                Fail = string.Format(" - Fail {0} timesheets.", failTS),
                LockDate = string.Format(" - Locked date: {0}.", lockDate.ToString("dd'-'MM'-'yyyy")),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timesheet_Approval)]
        public async System.Threading.Tasks.Task<object> RejectTimesheets(long[] myTimesheetIds)
        {
            int successTS = 0;
            int failTS = 0;
            var lockDate = _commonService.getlockDatePM();
            var userId = AbpSession.UserId.Value;
            var qprojectUsers = WorkScope.GetAll<ProjectUser>()
              .Where(s => s.UserId == AbpSession.UserId && s.Type == ProjectUserType.PM)
              .Select(s => new { s.ProjectId });

            var qMyTimeSheets = from mts in WorkScope.GetAll<MyTimesheet>().Where(s => myTimesheetIds.Contains(s.Id))
                                join pt in WorkScope.GetAll<ProjectTask>() on mts.ProjectTaskId equals pt.Id
                                select new { mts, pt.ProjectId };

            var mtss = await (from mts in qMyTimeSheets
                              join pu in qprojectUsers on mts.ProjectId equals pu.ProjectId
                              select mts.mts).ToListAsync();

            //var hackMtsIds = myTimesheetIds.Except(mtss.Select(s => s.Id));
            //if (hackMtsIds != null && hackMtsIds.Count() > 0)
            //{
            //    throw new UserFriendlyException(String.Format("You are not Project Manager of these Timesheet Ids: {0}", String.Join(", ", hackMtsIds)));
            //}

            myTimesheetIds = mtss.Select(s => s.Id).ToArray();
            var timesheetByUserByProject = await WorkScope.GetRepo<MyTimesheet>()
               .GetAllIncluding(s => s.ProjectTask.Task, s => s.ProjectTask.Project, s => s.User)
               .Where(s => myTimesheetIds.Contains(s.Id))
               .GroupBy(s => new { s.UserId, s.User.EmailAddress })
               .Select(s => new
               {
                   s.Key.UserId,
                   UserEmail = s.Key.EmailAddress,
                   Project = s.GroupBy(x => new { x.ProjectTask.Project.Id, x.ProjectTask.Project.Name, x.ProjectTask.Project.Code })
                   .Select(t => new
                   {
                       ProjectName = t.Key.Name,
                       ProjectCode = t.Key.Code,
                       Timesheets = t.Select(u => new
                       {
                           u.Id,
                           TaskName = u.ProjectTask.Task.Name,
                           Note = u.Note,
                           DateAt = u.DateAt,
                           TypeOfWork = u.TypeOfWork,
                           Charged = u.IsCharged,
                           WorkingTime = u.WorkingTime,
                           IsUnlockedByEmployee = u.IsUnlockedByEmployee ?? false
                       })
                   })
               }
               ).ToListAsync();
            var rejecterName = (await WorkScope.GetAsync<User>(AbpSession.UserId.Value)).FullName;
            var isUnlockPM = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == AbpSession.UserId.Value && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet);
            var myRejectTimesheetIds = new List<long>();
            var enableNotify = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailTimesheet);
            foreach (var user in timesheetByUserByProject)
            {
                //var isUnlockUser = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == user.UserId && s.Type == LockUnlockTimesheetType.MyTimesheet);
                var emails = new List<string>() { user.UserEmail };
                var mailBody = new StringBuilder();
                int successTSUser = 0;
                foreach (var project in user.Project)
                {
                    mailBody.Append($@"<div>
                                        <b>Project:</b>
                                        [{project.ProjectCode}]{project.ProjectName}
                                    </div>
                                    <div>
                                        <b>Timesheets:</b>
                                    </div> "
                                    );
                    var timesheetTable = new StringBuilder();
                    foreach (var timesheet in project.Timesheets)
                    {
                        if (timesheet.DateAt > lockDate || isUnlockPM || timesheet.IsUnlockedByEmployee)
                        {
                            myRejectTimesheetIds.Add(timesheet.Id);
                            timesheetTable.Append($@"
                                <tr>
                                <td>{timesheet.TaskName}</td>
                                <td>{timesheet.Note}</td>
                                <td>{timesheet.DateAt.ToString("yyyy'-'MM'-'dd")}</td>    
                                <td>{(timesheet.TypeOfWork == TypeOfWork.NormalWorkingHours ? "NormalWorking" : "OverTime")}</td>
                                <td>{(timesheet.Charged ? "Charged" : "")}</td>
                                <td>{TimeSpan.FromMinutes(timesheet.WorkingTime).ToString(@"hh\:mm")}</td>
                                </tr>");
                            successTS++;
                            successTSUser++;
                        }
                        else
                        {
                            failTS++;
                        }
                    }
                    mailBody.Append($@"<table border='1'>
                                    <thead>
                                        <tr>
                                            <td>Task name</td>
                                            <td>Note</td>
                                            <td>Date at</td>
                                            <td>Type of work </td>
                                            <td>Charged</td>
                                            <td>Working time</td>
                                        </tr>
                                    </thead>
                                <tbody>{timesheetTable}</tbody>
                                </table>
                                <hr>");
                }
                if (successTSUser > 0 && enableNotify == "true")
                {
                    await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                    {
                        TargetEmails = emails,
                        Body = mailBody.ToString(),
                        Subject = $"{ rejecterName } has rejected your timesheets"
                    }, BackgroundJobPriority.High, new TimeSpan(TimeSpan.TicksPerMinute));
                }
            }
            if (myRejectTimesheetIds.Count > 0)
            {
                var listReject = await WorkScope.GetAll<MyTimesheet>().Where(s => myRejectTimesheetIds.Contains(s.Id)).ToListAsync();
                foreach (var item in listReject)
                {
                    item.Status = TimesheetStatus.Reject;
                }
                await WorkScope.UpdateRangeAsync(listReject);
            }
            else if (failTS > 0)
            {
                throw new UserFriendlyException("PM hãy vào ims.nccsoft.vn để unlock timesheet");
            }

            return new
            {
                Success = string.Format(" - Success {0} timesheets.", successTS),
                Fail = string.Format(" - Fail {0} timesheets.", failTS),
                LockDate = string.Format(" - Locked date: {0}.", lockDate.ToString("dd'-'MM'-'yyyy")),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_NormalWorking_LockUnlockTimesheet)]
        public async Task<UnlockTimesheetDto> UnlockTimesheet(UnlockTimesheetDto input)
        {
            if (input.Type == LockUnlockTimesheetType.MyTimesheet)
            {
                var isExistUnlockUser = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == input.UserId && s.Type == LockUnlockTimesheetType.MyTimesheet);
                if (isExistUnlockUser)
                    throw new UserFriendlyException(string.Format("User Id - {0} already unlocked", input.UserId));
            }
            if (input.Type == LockUnlockTimesheetType.ApproveRejectTimesheet)
            {
                var isExistUnlockPM = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == input.UserId && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet);
                if (isExistUnlockPM)
                    throw new UserFriendlyException(string.Format("User Id - {0} already unlocked", input.UserId));
            }
            var item = ObjectMapper.Map<UnlockTimesheet>(input);
            input.Id = await WorkScope.InsertAndGetIdAsync(item);
            return input;

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_NormalWorking_LockUnlockTimesheet)]
        public async System.Threading.Tasks.Task LockTimesheet(LockTimesheetDto input)
        {
            long unLockUserId = 0;
            long unLockPMId = 0;
            if (input.Type == LockUnlockTimesheetType.MyTimesheet)
            {
                unLockUserId = await WorkScope.GetAll<UnlockTimesheet>().Where(s => s.UserId == input.UserId && s.Type == LockUnlockTimesheetType.MyTimesheet)
               .Select(s => s.Id).FirstOrDefaultAsync();
                if (unLockUserId == 0)
                    throw new UserFriendlyException(string.Format("UserId {0} is not exist", input.UserId));

                await WorkScope.GetRepo<UnlockTimesheet>().DeleteAsync(unLockUserId);
            }
            else if (input.Type == LockUnlockTimesheetType.ApproveRejectTimesheet)
            {
                unLockPMId = await WorkScope.GetAll<UnlockTimesheet>().Where(s => s.UserId == input.UserId && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet)
               .Select(s => s.Id).FirstOrDefaultAsync();
                if (unLockPMId == 0)
                    throw new UserFriendlyException(string.Format("UserId {0} is not exist", input.UserId));

                await WorkScope.GetRepo<UnlockTimesheet>().DeleteAsync(unLockPMId);
            }


        }
        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin)]
        public async System.Threading.Tasks.Task<List<ExportTimeSheetOrRemoteDto>> GetAllTimeSheetOrRemote(DateTime date, bool isExportTimeSheet)
        {
            try
            {
                List<ExportTimeSheetOrRemoteDto> listData = new List<ExportTimeSheetOrRemoteDto>();
                if (isExportTimeSheet)
                {
                    listData = await WorkScope.GetAll<MyTimesheet>()
                                    .Where(s => s.DateAt.Date == date.Date && s.Status == TimesheetStatus.Approve)
                                    .Select(s => new ExportTimeSheetOrRemoteDto
                                    {
                                        Id = s.Id,
                                        Name = s.User.Name,
                                        ProjectName = s.ProjectTask.Project.Name,
                                        NormalWorkingHours = s.TypeOfWork == TypeOfWork.NormalWorkingHours ? (s.WorkingTime) / 60 : 0,
                                        OverTime = s.TypeOfWork == TypeOfWork.OverTime ? (s.WorkingTime) / 60 : 0,
                                    }).ToListAsync();
                }
                else
                {
                    listData = await WorkScope.GetAll<AbsenceDayDetail>()
                                .Where(s => s.DateAt.Date == date.Date /*&& s.Request.Status == AbsenceStatus.Approved*/ && s.Request.Type == RequestType.Remote)
                                .Select(s => new ExportTimeSheetOrRemoteDto
                                {
                                    Name = s.Request.User.FullName,
                                    DayOffType = s.DateType,
                                    TimeCustom = (s.DateType == DayType.Custom ? s.AbsenceTime : 0),
                                    AbsenceTime = s.Hour
                                }).ToListAsync();
                }
                return listData;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"error:" + ex.Message);
            }
        }
        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Timesheet_ViewStatus)]
        public async Task<object> GetQuantiyTimesheetStatus(DateTime? startDate, DateTime? endDate, long? projectId, HaveCheckInFilter? checkInFilter, string searchText, long? branchId = 0)
        {
            var projectIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == AbpSession.UserId.Value && s.Type == ProjectUserType.PM)
                .Where(s => !projectId.HasValue || s.ProjectId == projectId)
                .Select(s => s.ProjectId).ToListAsync();

            var userIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => projectIds.Contains(s.ProjectId))
                .Select(s => s.UserId).Distinct().ToListAsync();

            var query = WorkScope.GetAll<MyTimesheet>()
                                 .Where(x => !startDate.HasValue || x.DateAt >= startDate)
                                 .Where(x => !endDate.HasValue || x.DateAt.Date <= endDate)
                                 .Where(x => userIds.Contains(x.UserId))
                                 .Where(x => projectIds.Contains(x.ProjectTask.ProjectId))
                                 .Where(x => string.IsNullOrEmpty(searchText) || x.User.EmailAddress.Contains(searchText) || x.User.UserName.Contains(searchText) || x.User.FullName.Contains(searchText))
                                 .Where(x => !branchId.HasValue || branchId == 0 || x.User.BranchId == branchId)
                                 .Select(x => new QuantiyTimesheetStatusDto
                                 {
                                     UserId = x.UserId,
                                     Status = x.Status,
                                     DateAt = x.DateAt.Date
                                 });

            var listMyTimesheet = new List<QuantiyTimesheetStatusDto>();

            var listTimekeeping = WorkScope.GetAll<Timekeeping>()
           .Select(s => new
           {
               UserId = s.UserId,
               CheckIn = s.CheckIn,
               CheckOut = s.CheckOut,
               DateAt = s.DateAt.Date
           })
           .Where(s => !startDate.HasValue || s.DateAt >= startDate)
           .Where(s => !endDate.HasValue || s.DateAt.Date <= endDate)
           .Where(s => s.UserId.HasValue)
           .ToList();

            foreach (var item in query)
            {
                var timekeepingByUserAtDate = listTimekeeping
                    .Where(s => s.UserId == item.UserId)
                    .Where(s => s.DateAt.Date == item.DateAt.Date)
                    .FirstOrDefault();
                item.CheckIn = timekeepingByUserAtDate?.CheckIn;
                item.CheckOut = timekeepingByUserAtDate?.CheckOut;
                listMyTimesheet.Add(item);
            }
            
            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckIn)
            {
                listMyTimesheet = listMyTimesheet.Where(s => s.CheckIn != null && s.CheckIn != "").ToList();
            }
            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckOut)
            {
                listMyTimesheet = listMyTimesheet.Where(s => s.CheckOut != null && s.CheckOut != "").ToList();
            }
            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckInAndHaveCheckOut)
            {
                listMyTimesheet = listMyTimesheet.Where(s => s.CheckIn != null && s.CheckIn != "" && s.CheckOut != null && s.CheckOut != "").ToList();
            }
            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.HaveCheckInOrHaveCheckOut)
            {
                listMyTimesheet = listMyTimesheet.Where(s => (s.CheckIn != null && s.CheckIn != "") || (s.CheckOut != null && s.CheckOut != "")).ToList();
            }
            if (checkInFilter.HasValue && checkInFilter.Value == HaveCheckInFilter.NoCheckInAndNoCheckOut)
            {
                listMyTimesheet = listMyTimesheet.Where(s => (s.CheckIn == null || s.CheckIn == "") && (s.CheckOut == null || s.CheckOut == "")).ToList();
            }
            
            var statusQuantityList = listMyTimesheet.GroupBy(x => x.Status).Select(x => new
            {
                Status = x.Key,
                Quantity = x.Count()
            }).ToDictionary(x => x.Status, x => x.Quantity);
            TimesheetStatus[] statuses = new TimesheetStatus[] {
                TimesheetStatus.All,
                TimesheetStatus.Approve,
                TimesheetStatus.None,
                TimesheetStatus.Pending,
                TimesheetStatus.Reject };

            var result = new List<Object>();
            foreach (TimesheetStatus status in statuses)
            {
                var quantity = statusQuantityList.ContainsKey(status) ? statusQuantityList[status] : 0;
                result.Add(new { Status = status, Quantity = quantity });
            }
            return result;
        }
    }
}

