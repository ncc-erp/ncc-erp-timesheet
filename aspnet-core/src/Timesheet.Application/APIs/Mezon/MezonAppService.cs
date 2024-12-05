using Abp.BackgroundJobs;
using Abp.UI;
using MassTransit.Initializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.MyAbsenceDays.Dto;
using Timesheet.APIs.RequestDays.Dto;
using Timesheet.BackgroundJob;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Services.Komu;
using Timesheet.Services.Mezon;
using Timesheet.Timesheets.MyTimesheets.Dto;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.APIs.RequestDays;
using Ncc.Authorization.Users;
using Timesheet.Timesheets.Projects.Dto;
using Timesheet.Timesheets.MyTimesheets;
using Abp.Authorization;
using AutoMapper.QueryableExtensions;
using Timesheet.APIs.DayOffs.Dto;

namespace Timesheet.APIs.Mezon
{
    public class MezonAppService : AppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly KomuService _komuService;
        public readonly IApproveRequestOffServices _approveRequestOffServices;
        private readonly MezonService _mezonService;
        private readonly IUserServices _userService;
        private readonly RequestDayAppService _requestDayAppService;
        private readonly MyTimesheetsAppService _myTimesheetsAppService;
        public MezonAppService(IBackgroundJobManager backgroundJobManager, KomuService komuService,
            IWorkScope workScope, IApproveRequestOffServices approveRequestOffServices,
            MezonService mezonService, IUserServices userService, RequestDayAppService requestDayAppService,
            MyTimesheetsAppService myTimesheetsAppService) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _komuService = komuService;
            _approveRequestOffServices = approveRequestOffServices;
            _mezonService = mezonService;
            _userService = userService;
            _requestDayAppService = requestDayAppService;
            _myTimesheetsAppService = myTimesheetsAppService;
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<MyRequestDto> SubmitToPendingNewRequestDay(MyRequestDto input)
        {
            var userIdCurrent = await _userService.GetUserIdByEmail(input.EmailAddress);
            if (!userIdCurrent.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + input.EmailAddress);
            }
            long userId = userIdCurrent.Value;

            var requester = GetSessionUserInfoDtoByUserId(userId);

            return await _requestDayAppService.ProcessSubmitToPendingNew(input, userId, requester);
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<ProjectIncludingTaskDto>> GetProjectsIncludingTasks(string emailAddress)
        {
            var userIdCurrent = await _userService.GetUserIdByEmail(emailAddress);
            if (!userIdCurrent.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }
            long userId = userIdCurrent.Value;

            var qProjectUserType = WorkScope.GetAll<ProjectUser>()
                  .Where(s => s.UserId == userId && s.Type != ProjectUserType.DeActive)
                  .Select(s => new
                  {
                      projectId = s.ProjectId,
                      s.Type
                  });

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

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<MyTimesheetDto> CreateTimeSheet(MyTimesheetDto input)
        {
            if (input.WorkingTime < 0)
                throw new UserFriendlyException("You can't log this time sheet with working time < 0");

            var userIdCurrent = await _userService.GetUserIdByEmail(input.EmailAddress);
            if (!userIdCurrent.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + input.EmailAddress);
            }
            long userId = userIdCurrent.Value;
            input.UserId = userId;

            //var isUnlock = await validUnLockTimsheet(input);
            var isUnlocked = _myTimesheetsAppService.IsUserUnlockedToLogTS(userId);
            _myTimesheetsAppService.CheckValidCreateUpdateTimesheet(input, isUnlocked);
            await _myTimesheetsAppService.validLogTimsheetInFuture(input);

            if (input.TypeOfWork == TypeOfWork.NormalWorkingHours)
            {
                //await validLogTimesheetOnDateOff(input.DateAt);
                //var maxHourCanLog = int.Parse(SettingManager.GetSettingValue(AppSettingNames.MaxTimeSheetHourPerDay));
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime, maxHourCanLog);
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime)
            }
            _myTimesheetsAppService.validTotalLogTimesheet(input.DateAt, input.WorkingTime, _myTimesheetsAppService.getMaxHourCanLog(), userId);


            await _myTimesheetsAppService.validLogOTTimesheet(input.DateAt, input.WorkingTime, input.TypeOfWork, userId);

            bool IsTemp = _myTimesheetsAppService.UserIsTempInProject(userId, input.ProjectTaskId);

            var timesheet = ObjectMapper.Map<MyTimesheet>(input);
            if (timesheet.ProjectTaskId == Convert.ToInt64(await SettingManager.GetSettingValueAsync(AppSettingNames.ProjectTaskId)))
            {
                timesheet.WorkingTime = 240;
            }
            timesheet.UserId = userId;
            timesheet.Status = TimesheetStatus.None;
            timesheet.DateAt = input.DateAt.Date;
            timesheet.IsUnlockedByEmployee = isUnlocked;
            timesheet.IsTemp = IsTemp;
            input.Id = await WorkScope.InsertAndGetIdAsync(timesheet);
            return input;
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<AbsenceTypeDto>> GetAllAbsenceType()
        {
            return await WorkScope.GetAll<DayOffType>().ProjectTo<AbsenceTypeDto>().ToListAsync();
        }
    }
}


