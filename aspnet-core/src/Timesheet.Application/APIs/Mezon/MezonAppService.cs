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
using Timesheet.APIs.Mezon.Dto;
using Timesheet.Services.W2;
using Timesheet.APIs.RequestDays.Dto;
using Microsoft.Office.Interop.Word;
using Abp.Configuration;
using Microsoft.AspNetCore.Http;

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
        private readonly IW2Service _w2Service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICommonServices _commonService;
        public MezonAppService(IBackgroundJobManager backgroundJobManager, KomuService komuService,
            IWorkScope workScope, IApproveRequestOffServices approveRequestOffServices,
            MezonService mezonService, IUserServices userService, RequestDayAppService requestDayAppService,
            MyTimesheetsAppService myTimesheetsAppService, IW2Service w2Service,
            IHttpContextAccessor httpContextAccessor, ICommonServices commonService) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _komuService = komuService;
            _approveRequestOffServices = approveRequestOffServices;
            _mezonService = mezonService;
            _userService = userService;
            _requestDayAppService = requestDayAppService;
            _myTimesheetsAppService = myTimesheetsAppService;
            _w2Service = w2Service;
            _httpContextAccessor = httpContextAccessor;
            _commonService = commonService;
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

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async System.Threading.Tasks.Task<List<MyTimesheetDto>> SaveListTimeSheet(List<MyTimesheetDto> myTimesheets)
        {
            foreach (var item in myTimesheets)
            {
                await CreateTimeSheet(item);
            }

            return myTimesheets;
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async System.Threading.Tasks.Task ApproveRequestDay(RequestDayMezon requestDayDto)
        {
            string emailPm = requestDayDto.Email;
            var userIdPmCurrent = await _userService.GetUserIdByEmail(emailPm);
            if (!userIdPmCurrent.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailPm);
            }
            long userIdPm = userIdPmCurrent.Value;

            var requestIds = requestDayDto.RequestIds;
            var requests = await WorkScope
            .GetAll<AbsenceDayRequest>()
            .Include(ar => ar.User) // Eager loading User
            .Where(ar => requestDayDto.RequestIds.Contains(ar.Id))
            .ToListAsync();
            var requestDictionary = requests.ToDictionary(ar => ar.Id);

            var dateRemotes = await WorkScope.GetAll<AbsenceDayDetail>()
            .Where(s => requestIds.Contains(s.RequestId)) 
            .Where(s => s.Request.Type == RequestType.Remote)
            .Where(s => s.Request.Status == RequestStatus.Rejected)
            .GroupBy(s => s.RequestId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(s => s.DateAt.ToString("yyyy-MM-dd")).FirstOrDefault());

            foreach (var requestId in requestDayDto.RequestIds)
            {
                if (!requestDictionary.TryGetValue(requestId, out var request))
                {
                    continue;
                }

                if (!await CheckUserIsPMOfUserByEmail(request.UserId, userIdPm))
                {
                    throw new UserFriendlyException("You are not PM of UserId " + request.UserId);
                }

                if (dateRemotes.TryGetValue(requestId, out var dateRemote) && dateRemote != null)
                {
                    ValidateWfhRequest(request.User.EmailAddress, dateRemote);
                }

                request.Status = RequestStatus.Approved;
                await WorkScope.UpdateAsync<AbsenceDayRequest>(request);
                await _requestDayAppService.notifyKomuWhenApproveOrRejectRequest(request, true, userIdPm);
            }
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async System.Threading.Tasks.Task RejectRequestDay(RequestDayMezon requestDayDto)
        {
            string emailPm = requestDayDto.Email;
            var userIdPmCurrent = await _userService.GetUserIdByEmail(emailPm);
            if (!userIdPmCurrent.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailPm);
            }
            long userIdPm = userIdPmCurrent.Value;

            var requests = await WorkScope
                .GetAll<AbsenceDayRequest>()
                .Where(ar => requestDayDto.RequestIds.Contains(ar.Id))
                .ToListAsync();

            var requestDictionary = requests.ToDictionary(ar => ar.Id);

            foreach (var requestId in requestDayDto.RequestIds)
            {
                if (!requestDictionary.TryGetValue(requestId, out var request))
                {
                    continue;
                }

                if (!await CheckUserIsPMOfUserByEmail(request.UserId, userIdPm))
                {
                    throw new UserFriendlyException($"You are not PM of UserId {request.UserId}");
                }

                request.Status = RequestStatus.Rejected;
                await WorkScope.UpdateAsync<AbsenceDayRequest>(request);
                await _requestDayAppService.notifyKomuWhenApproveOrRejectRequest(request, false, userIdPm);
            }
        }

        public async Task<bool> CheckUserIsPMOfUserByEmail(long userId, long userIdPm)
        {
            var qprojectIdsOfSessionUser = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userIdPm && s.Type == ProjectUserType.PM)
                .Where(s => s.Project.Status == ProjectStatus.Active)
                .Select(s => s.ProjectId);

            var qprojectIdsOfUser = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.UserId == userId && s.Type != ProjectUserType.DeActive)
                .Where(s => s.Project.Status == ProjectStatus.Active)
                .Select(s => s.ProjectId);

            return await (from p in qprojectIdsOfSessionUser
                          join p2 in qprojectIdsOfUser on p equals p2
                          select p).AnyAsync();

        }

        private void ValidateWfhRequest(string emailAddress, string dateRemote)
        {
            var wfhRequestDto = _w2Service.GetWfhRequest(emailAddress, dateRemote);

            if (wfhRequestDto == null)
            {
                throw new UserFriendlyException("Cannot get request information from the W2 system!");
            }

            if (wfhRequestDto.Status != WfhW2RequestStatus.Approved)
            {
                throw new UserFriendlyException("This WFH request cannot be approved because it has not been approved/created on the W2 system!");
            }
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<GetTimesheetDto>> GetAllTimesheetOfUser(DateTime startDate, DateTime endDate, string emailAddress)
        {
            var userIdCurrent = await _userService.GetUserIdByEmail(emailAddress);
            if (!userIdCurrent.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }
            long userId = userIdCurrent.Value;

            var openTalkTimes = WorkScope.GetAll<OpenTalk>()
                .Where(x => x.UserId == userId)
                .ToList()
                .GroupBy(x => x.DateAt.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.totalTime));

            var timesheets = await (from myTimesheet in WorkScope.All<MyTimesheet>().AsNoTracking()
                                    join ptask in WorkScope.All<ProjectTask>().AsNoTracking() on myTimesheet.ProjectTaskId equals ptask.Id
                                    join project in WorkScope.All<Project>().AsNoTracking() on ptask.ProjectId equals project.Id
                                    join task in WorkScope.All<Ncc.Entities.Task>().AsNoTracking() on ptask.TaskId equals task.Id
                                    join cus in WorkScope.All<Customer>().AsNoTracking() on project.CustomerId equals cus.Id
                                    where myTimesheet.UserId == userId && myTimesheet.DateAt >= startDate && myTimesheet.DateAt <= endDate
                                    select new GetTimesheetDto
                                    {
                                        Id = myTimesheet.Id,
                                        CustomerName = cus.Name,
                                        DateAt = myTimesheet.DateAt,
                                        ProjectCode = project.Code,
                                        ProjectName = project.Name,
                                        Status = myTimesheet.Status,
                                        TaskName = task.Name,
                                        WorkingTime = myTimesheet.WorkingTime,
                                        ProjectTaskId = ptask.Id,
                                        Note = myTimesheet.Note,
                                        TypeOfWork = myTimesheet.TypeOfWork,
                                        IsCharged = myTimesheet.IsCharged,
                                        Billable = ptask.Billable,
                                        IsTemp = myTimesheet.IsTemp,
                                        ProjectTargetUser = myTimesheet.ProjectTargetUser.User.FullName,
                                        WorkingTimeTargetUser = myTimesheet.TargetUserWorkingTime,
                                        OpenTalkJoinTime = task.Name == "Open Talk" && openTalkTimes.ContainsKey(myTimesheet.DateAt.Date) ? openTalkTimes[myTimesheet.DateAt.Date] : 0
                                    }).ToListAsync();

            return timesheets;
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<string> SubmitTsToPending(StartEndDateDto input, string emailAddress)
        {
            var userIdCurrent = await _userService.GetUserIdByEmail(emailAddress);
            if (!userIdCurrent.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }
            long userId = userIdCurrent.Value;

            var isUnLocked = _myTimesheetsAppService.IsUserUnlockedToLogTS(userId);

            var mytimesheets = await WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.UserId == userId)
                .Where(s => s.DateAt >= input.StartDate.Date && s.DateAt.Date <= input.EndDate)
                .Where(s => s.Status == TimesheetStatus.None)
                .ToListAsync();

            //Valid :
            DateTime lockDate = _commonService.getlockDateUser();
            if (!isUnLocked && mytimesheets.Any(s => s.DateAt.Date < lockDate))
            {
                throw new UserFriendlyException("Go to ims.nccsoft.vn > Unlock timesheet");
            }

            var firstDateCanUnlock = _myTimesheetsAppService.GetFirstDateToLockTS(userId, isUnLocked).Result;
            if (input.EndDate.Date < firstDateCanUnlock)
            {
                throw new UserFriendlyException("Timesheet was locked! You can submit timesheet begin :" + firstDateCanUnlock.ToString("yyyy-MM-dd"));
            }

            foreach (var item in mytimesheets)
            {
                item.Status = TimesheetStatus.Pending;
                if (isUnLocked)
                {
                    item.IsUnlockedByEmployee = isUnLocked;
                }
            }
            await WorkScope.UpdateRangeAsync(mytimesheets);

            await notifySubmitTimesheet(mytimesheets, userId);

            var result = "Submit success " + mytimesheets.Count + " timesheets";
            return result;
        }

        public async System.Threading.Tasks.Task notifySubmitTimesheet(List<MyTimesheet> mytimesheets, long userId)
        {
            var SendEmailSubmitTimesheet = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailTimesheet);
            var NotifyKomuWhenSubmitTimesheet = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuSubmitTimesheet);
            if (NotifyKomuWhenSubmitTimesheet != "true" && SendEmailSubmitTimesheet != "true")
            {
                Logger.Info("SendEmailSubmitTimesheet=" + SendEmailSubmitTimesheet + ", UserId=" + userId);
                Logger.Info("NotifyKomuWhenSubmitTimesheet=" + NotifyKomuWhenSubmitTimesheet + ", UserId=" + userId);
                return;
            }
            var requester = await _myTimesheetsAppService.getNotifyUserInfoDto(userId);
            var receivers = await _myTimesheetsAppService.getReceiverList(mytimesheets);

            _myTimesheetsAppService.notifyKomuWhenSubmitTimesheet(requester, receivers);
            await _myTimesheetsAppService.notifyEmailWhenSubmitTimesheet(requester, receivers);
        }
    }
}


