using Ncc;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Entities;
using Timesheet.Timesheets.MyTimesheets.Dto;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ncc.Authorization.Users;
using static Ncc.Entities.Enum.StatusEnum;
using Microsoft.EntityFrameworkCore;
using Abp.UI;
using Abp.Authorization;
using Timesheet.Extension;
using Abp.BackgroundJobs;
using Timesheet.BackgroundJob;
using Ncc.Configuration;
using Timesheet.DomainServices;
using Timesheet.Uitls;
using Timesheet.APIs.Timesheets.MyTimesheets.Dto;
using Timesheet.APIs.MyAbsenceDays.Dto;
using Timesheet.DomainServices.Dto;
using Timesheet.Timesheets.Projects.Dto;
using Abp.Configuration;
using Microsoft.AspNetCore.Http;
using Ncc.IoC;
using Timesheet.NCCAuthen;
using Timesheet.Services.Komu;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Timesheet.Timesheets.MyTimesheets
{
    //[AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet, Ncc.Authorization.PermissionNames.Home)]

    public class MyTimesheetsAppService : AppServiceBase
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ICommonServices _commonService;
        private readonly KomuService _komuService;

        public MyTimesheetsAppService(IBackgroundJobManager backgroundJobManager, KomuService komuService,
            ICommonServices commonService, IWorkScope workScope) : base(workScope)
        {
            _backgroundJobManager = backgroundJobManager;
            _commonService = commonService;
            _komuService = komuService;
        }
        [HttpGet]
        //[AbpAuthorize(Ncc.Authorization.PermissionNames.Home)]
        public async Task<List<TimeStatisticMemberDto>> GetTimesheetStatisticMembers(DateTime? startDate, DateTime? endDate)
        {

            return await (from myTimesheet in WorkScope.GetAll<MyTimesheet>()
                        .Where(s => !startDate.HasValue || s.DateAt >= startDate)
                        .Where(s => !endDate.HasValue || s.DateAt <= endDate)
                        .Where(s => s.Status == TimesheetStatus.Approve)
                        .Where(s => s.User.IsActive == true)
                          join user in WorkScope.GetAll<User>() on myTimesheet.UserId equals user.Id
                          join ptask in WorkScope.GetAll<ProjectTask>() on myTimesheet.ProjectTaskId equals ptask.Id
                          select new
                          {
                              UserId = user.Id,
                              UserName = user.Name + " " + user.Surname,
                              myTimesheet.WorkingTime,
                              ptask.Billable,
                              myTimesheet.TypeOfWork,
                              myTimesheet.IsCharged
                          })
                        .GroupBy(s => new { s.UserId, s.UserName })
                .Select(s => new TimeStatisticMemberDto
                {
                    UserID = s.Key.UserId,
                    UserName = s.Key.UserName,
                    TotalWorkingTime = s.Sum(x => x.WorkingTime),
                    BillableWorkingTime = s.Sum(x => (x.Billable
                                                    && (x.TypeOfWork == TypeOfWork.NormalWorkingHours
                                                        || (x.TypeOfWork == TypeOfWork.OverTime && x.IsCharged)))
                                                        ? x.WorkingTime : 0)
                }).ToListAsync();
        }

        [HttpGet]
        //[AbpAuthorize(Ncc.Authorization.PermissionNames.Home)]
        public async Task<List<TimeStatisticTaskDto>> GetTimesheetStatisticTasks(DateTime? startDate, DateTime? endDate)
        {

            var qprojectTask = WorkScope.GetRepo<ProjectTask>().GetAllIncluding(s => s.Task)
            .Select(s => new { s.TaskId, TaskName = s.Task.Name, s.Id, s.Billable });

            return await (from mts in WorkScope.All<MyTimesheet>()
                          join ptask in qprojectTask on mts.ProjectTaskId equals ptask.Id
                          where (!startDate.HasValue || mts.DateAt >= startDate)
                          where (!endDate.HasValue || mts.DateAt <= endDate)
                          where (mts.Status == TimesheetStatus.Approve)
                          select new
                          {
                              ptask.TaskName,
                              ptask.TaskId,
                              mts.WorkingTime,
                              ptask.Billable,
                              mts.IsCharged,
                              mts.TypeOfWork
                          }).GroupBy(s => new { s.TaskId, s.TaskName })
                            .Select(s => new TimeStatisticTaskDto
                            {
                                TaskName = s.Key.TaskName,
                                TaskId = s.Key.TaskId,
                                TotalWorkingTime = s.Sum(x => x.WorkingTime),
                                BillableWorkingTime = s.Sum(x => (x.Billable
                                                    && (x.TypeOfWork == TypeOfWork.NormalWorkingHours
                                                        || (x.TypeOfWork == TypeOfWork.OverTime && x.IsCharged)))
                                                        ? x.WorkingTime : 0)
                            }).ToListAsync();
        }
        [HttpGet]
        //[AbpAuthorize(Ncc.Authorization.PermissionNames.Home)]
        public async Task<List<TimeStatisticProjectDto>> GetTimesheetStatisticProjects(DateTime? startDate, DateTime? endDate)
        {
            var qprojectTask = WorkScope.GetRepo<ProjectTask>().GetAllIncluding(s => s.Project)
                .Select(s => new { s.Id, s.ProjectId, ProjectName = s.Project.Name, s.Billable });

            return await (from myTimesheet in WorkScope.All<MyTimesheet>()
                          join pTask in qprojectTask on myTimesheet.ProjectTaskId equals pTask.Id
                          where (!startDate.HasValue || myTimesheet.DateAt >= startDate)
                          where (!endDate.HasValue || myTimesheet.DateAt <= endDate)
                          where (myTimesheet.Status == TimesheetStatus.Approve)
                          select new
                          {
                              myTimesheet.WorkingTime,
                              pTask.ProjectId,
                              pTask.ProjectName,
                              pTask.Billable,
                              myTimesheet.TypeOfWork,
                              myTimesheet.IsCharged
                          }).GroupBy(s => new { s.ProjectId, s.ProjectName })
                            .Select(s => new TimeStatisticProjectDto
                            {
                                ProjectId = s.Key.ProjectId,
                                ProjectName = s.Key.ProjectName,
                                BillableWorkingTime = s.Sum(x => (x.Billable
                                                    && (x.TypeOfWork == TypeOfWork.NormalWorkingHours
                                                        || (x.TypeOfWork == TypeOfWork.OverTime && x.IsCharged)))
                                                        ? x.WorkingTime : 0),
                                TotalWorkingTime = s.Sum(x => x.WorkingTime)
                            }).ToListAsync();
        }
        [HttpGet]
        //[AbpAuthorize(Ncc.Authorization.PermissionNames.Home)]
        public async Task<List<TimeStatisticCustomerDto>> GetTimesheetStatisticClients(DateTime? startDate, DateTime? endDate)
        {
            var qprojectTask = from ptask in WorkScope.All<ProjectTask>()
                               join p in WorkScope.All<Project>() on ptask.ProjectId equals p.Id
                               join c in WorkScope.All<Customer>() on p.CustomerId equals c.Id
                               select (new
                               {
                                   CustomerId = c.Id,
                                   CustomerName = c.Name,
                                   ptask.Billable,
                                   ptask.Id
                               });
            return await (from myTimesheet in WorkScope.All<MyTimesheet>()
                          join pt in qprojectTask on myTimesheet.ProjectTaskId equals pt.Id
                          where (!startDate.HasValue || myTimesheet.DateAt >= startDate)
                          where (!endDate.HasValue || myTimesheet.DateAt <= endDate)
                          where (myTimesheet.Status == TimesheetStatus.Approve)
                          select new
                          {
                              myTimesheet.WorkingTime,
                              pt.Billable,
                              pt.CustomerId,
                              pt.CustomerName,
                              myTimesheet.TypeOfWork,
                              myTimesheet.IsCharged
                          }).GroupBy(s => new { s.CustomerId, s.CustomerName })
                                .Select(s => new TimeStatisticCustomerDto
                                {
                                    CustomerId = s.Key.CustomerId,
                                    CustomerName = s.Key.CustomerName,
                                    TotalWorkingTime = s.Sum(x => x.WorkingTime),
                                    BillableWorkingTime = s.Sum(x => (x.Billable
                                                     && (x.TypeOfWork == TypeOfWork.NormalWorkingHours
                                                         || (x.TypeOfWork == TypeOfWork.OverTime && x.IsCharged)))
                                                         ? x.WorkingTime : 0)
                                }).ToListAsync();
        }

        [HttpGet]
        //[AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_View, Ncc.Authorization.PermissionNames.Home)]
        public async Task<TimesheetReportDto> GetTimesheetReportHours(DateTime? startDate, DateTime? endDate)
        {
            var qmts = WorkScope.GetRepo<MyTimesheet>().GetAllIncluding(s => s.ProjectTask)
                .Where(s => !startDate.HasValue || s.DateAt >= startDate.Value)
                .Where(s => !endDate.HasValue || s.DateAt <= endDate.Value)
                .Where(s => s.Status == TimesheetStatus.Approve)
                .Select(s => new
                {
                    Billable = s.ProjectTask.Billable &&
                    (s.TypeOfWork == TypeOfWork.NormalWorkingHours || (s.TypeOfWork == TypeOfWork.OverTime && s.IsCharged)) ? s.WorkingTime : 0,
                    TotalTime = s.WorkingTime,
                    NormalWorkingHours = s.TypeOfWork == TypeOfWork.NormalWorkingHours ? s.WorkingTime : 0,
                    OvertimeBillable = s.TypeOfWork == TypeOfWork.OverTime && s.ProjectTask.Billable && s.IsCharged ? s.WorkingTime : 0,
                    OvertimeNonBillable = s.TypeOfWork == TypeOfWork.OverTime && (!s.ProjectTask.Billable || !s.IsCharged) ? s.WorkingTime : 0,
                });

            var timesheetReport = new TimesheetReportDto
            {
                Billable = await qmts.SumAsync(s => s.Billable),
                NonBillable = await qmts.SumAsync(s => s.TotalTime - s.Billable),
                NormalWorkingHours = await qmts.SumAsync(s => s.NormalWorkingHours),
                OvertimeBillable = await qmts.SumAsync(s => s.OvertimeBillable),
                OvertimeNonBillable = await qmts.SumAsync(s => s.OvertimeNonBillable)
            };
            timesheetReport.HoursTracked = timesheetReport.Billable + timesheetReport.NonBillable;
            return timesheetReport;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_View)]
        public async Task<List<GetTimesheetDto>> GetAllTimesheetOfUser(DateTime startDate, DateTime endDate)
        {
            return await (from myTimesheet in WorkScope.All<MyTimesheet>()
                          join ptask in WorkScope.All<ProjectTask>() on myTimesheet.ProjectTaskId equals ptask.Id
                          join project in WorkScope.All<Project>() on ptask.ProjectId equals project.Id
                          join task in WorkScope.All<Ncc.Entities.Task>() on ptask.TaskId equals task.Id
                          join cus in WorkScope.All<Customer>() on project.CustomerId equals cus.Id
                          where (myTimesheet.UserId == AbpSession.UserId.Value && myTimesheet.DateAt >= startDate && myTimesheet.DateAt <= endDate)
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
                              IsTemp = myTimesheet.IsTemp
                          }).ToListAsync();
        }

        private async System.Threading.Tasks.Task<bool> validUnLockTimsheet(MyTimesheetDto input)
        {
            //if (input.DateAt.Month < DateTimeUtils.GetNow().Month)
            //{
            //    var DateToLockTimesheetLastMonthString = await SettingManager.GetSettingValueAsync(AppSettingNames.DateToLockTimesheetOfLastMonth);
            //    var DeadLineDay = 5;
            //    int.TryParse(DateToLockTimesheetLastMonthString, out DeadLineDay);
            //    if (DateTimeUtils.GetNow().Day >= DeadLineDay)
            //    {
            //        throw new UserFriendlyException("Timesheet in last month was locked! DeadLineDay=" + DeadLineDay);
            //    }
            //}
            var firstDateCanUnlock = GetFirstDateToLockTS(AbpSession.UserId.Value, false).Result;
            if (input.DateAt.Date < firstDateCanUnlock)
            {
                throw new UserFriendlyException("Timesheet was locked! You can log timesheet begin :" + firstDateCanUnlock.ToString("yyyy-MM-dd"));
            }
            else
            {
                return true;
            }

            //var userId = AbpSession.UserId.Value;
            //var isUnlock = await WorkScope.GetAll<UnlockTimesheet>()
            //    .AnyAsync(s => s.UserId == userId && s.Type == LockUnlockTimesheetType.MyTimesheet);
            //if (!isUnlock)
            //{
            //    var lockDate = _commonService.getlockDateUser();
            //    if (input.DateAt.Date < lockDate)
            //    {
            //        throw new UserFriendlyException(string.Format("Go to ims.nccsoft.vn > Unlock timesheet"));
            //    }
            //}
            //return isUnlock;
        }


        private void CheckValidCreateUpdateTimesheet(MyTimesheetDto input, bool isUnlocked)
        {
           
            var firstDateCanUnlock = GetFirstDateToLockTS(AbpSession.UserId.Value, isUnlocked).Result;
            if (input.DateAt.Date < firstDateCanUnlock)
            {
                throw new UserFriendlyException("Timesheet was locked! You can log timesheet begin :" + firstDateCanUnlock.ToString("yyyy-MM-dd"));
            }
           
        }
        private async Task<DateTime> GetFirstDateToLockTS(long userId, bool isUnlocked)
        {
            var weeksCanUnlockBefor = int.Parse(SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.WeeksCanUnlockBefor).Result);
            var now = DateTimeUtils.GetNow();

            var DateToLockTimesheetOfLastMonthCfg = await SettingManager.GetSettingValueAsync(AppSettingNames.DateToLockTimesheetOfLastMonth);
            int DateToLockTimesheetOfLastMonth = 5;
            int.TryParse(DateToLockTimesheetOfLastMonthCfg, out DateToLockTimesheetOfLastMonth);

            var days = isUnlocked ? weeksCanUnlockBefor * -7 : 0;

            //var startDateToCheck = now.Day < DateToLockTimesheetOfLastMonth ? now.AddDays(1 - now.Day).AddMonths(-1).Date : now.AddDays(1 - now.Day).Date;
            var startDateToCheck = now.Day < DateToLockTimesheetOfLastMonth
                                    ? DateTimeUtils.FirstDayOfWeek(now).AddDays(days)
                                    : DateTimeUtils.Max(DateTimeUtils.FirstDayOfMonth(now),
                                                        DateTimeUtils.FirstDayOfWeek(now).AddDays(days));

            return startDateToCheck;
        }

        private bool IsUserUnlockedToLogTS(long userId)
        {
            return WorkScope.GetAll<UnlockTimesheet>()
                .Any(s => s.UserId == userId && s.Type == LockUnlockTimesheetType.MyTimesheet);
        }
        private async System.Threading.Tasks.Task validLogTimsheetInFuture(MyTimesheetDto input)
        {
            var canLogTimesheetInFuture = await SettingManager.GetSettingValueAsync(AppSettingNames.LogTimesheetInFuture);
            var dayAllow = await SettingManager.GetSettingValueAsync(AppSettingNames.DayAllowLogTimesheetInFuture);
            if (canLogTimesheetInFuture.Equals("false"))
            {
                if (input.DateAt.CompareTo(DateTime.Now) > 0)
                {
                    throw new UserFriendlyException("You can't log time sheet for the future");
                }
            }
            else
            {
                if (input.DateAt.CompareTo(DateTime.Now.AddDays(double.Parse(dayAllow))) > 0)
                {
                    throw new UserFriendlyException("You can't log time sheet with date > " + DateTime.Now.AddDays(double.Parse(dayAllow)).ToString("yyyy-MM-dd"));
                }
            }
        }

        private async System.Threading.Tasks.Task validLogTimesheetOnDateOff(DateTime dateAt)
        {
            var isDayoff = await WorkScope.GetAll<DayOffSetting>()
                .AnyAsync(s => s.DayOff.Date == dateAt.Date);

            if (isDayoff)
            {
                throw new UserFriendlyException(string.Format("{0} is not a working day.", dateAt.ToString("yyyy-MM-dd")));
            }
        }

        private async System.Threading.Tasks.Task validLogTimesheetOver8h(DateTime dateAt, int workingTime, int maxHour)
        {
            var userId = AbpSession.UserId.Value;

            double sumWorkingTimeOlds = WorkScope.GetAll<MyTimesheet>()
               .Where(s => s.UserId == userId && s.DateAt.Date == dateAt.Date)
               .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
               .Sum(s => s.WorkingTime);

            double sumWorkingTime = sumWorkingTimeOlds + workingTime;

            if (sumWorkingTime > maxHour * 60)
            {
                throw new UserFriendlyException(string.Format($"total normal working time on {dateAt.ToString("yyyy-MM-dd")} can't  > {maxHour} hours"));
            }


            var offHour = await WorkScope.GetAll<AbsenceDayDetail>().Where(s => s.Request.UserId == userId && s.DateAt.Date == dateAt.Date)
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.Request.Status != RequestStatus.Rejected)
                .Select(s => s.Hour)
                .FirstOrDefaultAsync();

            var q = await WorkScope.GetAll<AbsenceDayDetail>().Where(s => s.Request.UserId == userId && s.DateAt.Date == dateAt.Date)
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.Request.Status != RequestStatus.Rejected)
                .Select(s => s.Hour)
                .FirstOrDefaultAsync();

            if (sumWorkingTime > (maxHour - offHour) * 60)
            {
                throw new UserFriendlyException(String.Format("You requested off {0} hour. So can't log total normal working > {1} on {2}", offHour, maxHour - offHour, dateAt.ToString("yyyy-MM-dd")));
            }
        }
        private void validTotalLogTimesheet(DateTime dateAt, int workingTime, int maxHour)
        {
            var userId = AbpSession.UserId.Value;

            double sumWorkingTimeOlds = WorkScope.GetAll<MyTimesheet>()
               .Where(s => s.UserId == userId && s.DateAt.Date == dateAt.Date)
               .Sum(s => s.WorkingTime);

            double sumWorkingTime = sumWorkingTimeOlds + workingTime;

            if (sumWorkingTime > maxHour * 60)
            {
                throw new UserFriendlyException(string.Format($"total working time on {dateAt.ToString("yyyy-MM-dd")} can't  > {maxHour} hours"));
            }
        }
        private async System.Threading.Tasks.Task validLogOTTimesheet(DateTime dateAt, int workingTime, TypeOfWork typeOfWork)
        {
            if (typeOfWork == TypeOfWork.NormalWorkingHours)
            {
                return;
            }
            if (dateAt.DayOfWeek != DayOfWeek.Saturday)
            {
                return;
            }
            var normalWorkingMinute = await sumNormalWorkingMinute(AbpSession.UserId.Value, dateAt);

            var OTMinute = workingTime - (240 - normalWorkingMinute);

            if (normalWorkingMinute < 240)
            {
                throw new UserFriendlyException($"Saturday morning is NORMAL WORKING. You have to log 4h NORMAL WORKING first. So, the rest {CommonUtils.ConvertHourToHHmm(OTMinute)} is OT");
            }
        }

        private async Task<int> sumNormalWorkingMinute(long userId, DateTime dateAt)
        {
            return await WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.DateAt.Date == dateAt.Date)
                .Where(s => s.UserId == userId)
                .Where(s => s.Status != TimesheetStatus.Reject)
                .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .SumAsync(s => s.WorkingTime);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_AddNew)]
        public async Task<MyTimesheetDto> Create(MyTimesheetDto input)
        {
            if (input.WorkingTime < 0)
                throw new UserFriendlyException("You can't log this time sheet with working time < 0");

            var userId = AbpSession.UserId.Value;

            //var isUnlock = await validUnLockTimsheet(input);
            var isUnlocked = IsUserUnlockedToLogTS(userId);
            CheckValidCreateUpdateTimesheet(input, isUnlocked);
            await validLogTimsheetInFuture(input);

            if (input.TypeOfWork == TypeOfWork.NormalWorkingHours)
            {
                //await validLogTimesheetOnDateOff(input.DateAt);
                //var maxHourCanLog = int.Parse(SettingManager.GetSettingValue(AppSettingNames.MaxTimeSheetHourPerDay));
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime, maxHourCanLog);
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime)
            }
            validTotalLogTimesheet(input.DateAt, input.WorkingTime, getMaxHourCanLog());


            await validLogOTTimesheet(input.DateAt, input.WorkingTime, input.TypeOfWork);

            bool IsTemp = UserIsTempInProject(userId, input.ProjectTaskId);

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

        private bool UserIsTempInProject(long userId, long projectTaskId)
        {
            var projectId = WorkScope.GetAll<ProjectTask>()
                .Where(s => s.Id == projectTaskId)
                .Select(s => s.ProjectId)
                .FirstOrDefault();
            if (projectId == default)
            {
                throw new UserFriendlyException($"Not found ProjectTask by Id {projectTaskId}");
            }
            bool IsTemp = WorkScope.GetAll<ProjectUser>()
               .Where(s => s.UserId == userId)
               .Where(s => s.ProjectId == projectId)
               .Select(s => s.IsTemp).FirstOrDefault();
            return IsTemp;
        }

        private async System.Threading.Tasks.Task validUpdateNormalToOT(MyTimesheet entity, MyTimesheetDto dto)
        {
            if (entity.TypeOfWork == dto.TypeOfWork || dto.TypeOfWork == TypeOfWork.NormalWorkingHours)
            {
                return;
            }
            if (dto.DateAt.DayOfWeek != DayOfWeek.Saturday)
            {
                return;
            }
            var normalWorkingMinute = await sumNormalWorkingMinute(AbpSession.UserId.Value, dto.DateAt);
            normalWorkingMinute -= entity.WorkingTime;

            if (normalWorkingMinute < 240)
            {
                throw new UserFriendlyException($"Saturday morning is NORMAL WORKING. You have to log 4h NORMAL WORKING first.");
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_Edit)]
        public async Task<MyTimesheetDto> Update(MyTimesheetDto input)
        {
            if (input.WorkingTime < 0)
                throw new UserFriendlyException("You can't log this time sheet");

            //var isUnlock = await validUnLockTimsheet(input);
            var isUnlocked = IsUserUnlockedToLogTS(AbpSession.UserId.Value);
            CheckValidCreateUpdateTimesheet(input, isUnlocked);


            var item = await WorkScope.GetAsync<MyTimesheet>(input.Id);
            if (item == null)
                throw new UserFriendlyException(string.Format("Timesheet Id {0} is not exist", input.Id));

            if (AbpSession.UserId.Value != item.UserId)
                throw new UserFriendlyException(string.Format("You can't update other people's timesheet"));

            if (item.Status == TimesheetStatus.Approve)
                throw new UserFriendlyException(string.Format("You can't update approved Timesheet"));


            if (input.TypeOfWork == TypeOfWork.NormalWorkingHours)
            {
                //await validLogTimesheetOnDateOff(input.DateAt);
                //var maxHourCanLog = int.Parse(SettingManager.GetSettingValue(AppSettingNames.MaxTimeSheetHourPerDay));
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime - item.WorkingTime, maxHourCanLog);
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime - item.WorkingTime);
            }
            else
            {
                await validUpdateNormalToOT(item, input);
            }
            validTotalLogTimesheet(input.DateAt, input.WorkingTime - item.WorkingTime, getMaxHourCanLog());

            var status = item.Status;
            input.IsTemp = item.IsTemp;

            ObjectMapper.Map(input, item);

            item.Status = status;
            item.DateAt = item.DateAt.Date;
            if (item.TypeOfWork == TypeOfWork.NormalWorkingHours)
                item.IsCharged = false;

            item.IsUnlockedByEmployee = isUnlocked;
            if (item.ProjectTaskId == Convert.ToInt64(await SettingManager.GetSettingValueAsync(AppSettingNames.ProjectTaskId)))
            {
                item.WorkingTime = 240;
            }

            await WorkScope.UpdateAsync(item);

            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_View)]
        public async Task<MyTimesheetDto> Get(long id)
        {
            var item = await WorkScope.GetAsync<MyTimesheet>(id);
            return ObjectMapper.Map<MyTimesheetDto>(item);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_Submit)]
        public async System.Threading.Tasks.Task<List<MyTimesheetDto>> SaveList(List<MyTimesheetDto> myTimesheets)
        {
            foreach (var item in myTimesheets)
            {
                await Create(item);
            }

            return myTimesheets;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_Delete)]
        public async System.Threading.Tasks.Task Delete(long Id)
        {
            var item = await WorkScope.GetAsync<MyTimesheet>(Id);
            if (item == null)
                throw new UserFriendlyException(String.Format("MyTimeSheet Id {0} is not exist", Id));

            if (item.Status == TimesheetStatus.Approve)
                throw new UserFriendlyException(String.Format("MyTimeSheet Id {0} is approved", Id));

            await WorkScope.DeleteAsync<MyTimesheet>(Id);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_Submit)]
        public async Task<string> SubmitToPending(StartEndDateDto input)
        {
            var isUnLocked = IsUserUnlockedToLogTS(AbpSession.UserId.Value);

            var mytimesheets = await WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.UserId == AbpSession.UserId.Value)
                .Where(s => s.DateAt >= input.StartDate.Date && s.DateAt.Date <= input.EndDate)
                .Where(s => s.Status == TimesheetStatus.None)
                .ToListAsync();

            //Valid :
            DateTime lockDate = _commonService.getlockDateUser();
            if (!isUnLocked && mytimesheets.Any(s => s.DateAt.Date < lockDate))
            {
                throw new UserFriendlyException("Go to ims.nccsoft.vn > Unlock timesheet");
            }

            foreach (var item in mytimesheets)
            {
                item.Status = TimesheetStatus.Pending;
                if (isUnLocked)
                {
                    item.IsUnlockedByEmployee = isUnLocked;
                }
                await WorkScope.UpdateAsync(item);
            }

            await notifySubmitTimesheet(mytimesheets);

            var result = "Submit success " + mytimesheets.Count + " timesheets";
            return result;
        }

        private async System.Threading.Tasks.Task notifyEmailWhenSubmitTimesheet(NotifyUserInfoDto requester, List<NotifyKomuTimesheetDto> receivers)
        {
            var SendEmailSubmitTimesheet = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailTimesheet);
            if (SendEmailSubmitTimesheet != "true")
            {
                Logger.Info("SendEmailSubmitTimesheet != true = stop");
                return;
            }
            foreach (var project in receivers)
            {
                if (project.PMs.Count() > 0)
                {
                    // Noi dung email
                    StringBuilder timesheetList = new StringBuilder();
                    foreach (var timesheet in project.Timesheets)
                    {
                        timesheetList.Append($@"
                                    <tr>
                                    <td>{timesheet.DateAt.ToString("yyyy'-'MM'-'dd")}</td>
                                    <td>{timesheet.TaskName}</td>
                                    <td>{timesheet.Note}</td>
                                    <td>{TimeSpan.FromMinutes(timesheet.WorkingTime).ToString(@"hh\:mm")}</td>
                                    <td>{(timesheet.TypeOfWork == TypeOfWork.NormalWorkingHours ? "NormalWorking" : "OverTime")}</td>
                                    <td>{(timesheet.IsCharged ? "Charged" : "")}</td>
                                    </tr>");
                    }
                    var emailBody = $@"
                                <hr>
                                <div>
                                  <div><strong>Project:</strong>[{project.ProjectCode}]{project.ProjectName}</div><hr>
                                    <table border='1'>
                                        <thead>
                                            <tr>
                                                <td>Date at</td>
                                                <td>Task Name</td>
                                                <td>Note</td>
                                                <td>Working time</td>
                                                <td>Type</td>
                                                <td>Charged</td>
                                            </tr>
                                        </thead>
                                    <tbody>{timesheetList}</tbody>
                                    </table>
                                    </div>";

                    var emailSubject = $"{requester.ToEmailString()} has submited timesheets for [{project.ProjectName}]";

                    await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                    {
                        TargetEmails = project.Emails,
                        Body = emailBody,
                        Subject = emailSubject
                    });

                }
            }
        }

        private async Task<NotifyUserInfoDto> getNotifyUserInfoDto(long userId)
        {
            return await WorkScope.GetAll<User>().Where(s => s.Id == userId)
                .Select(user => new NotifyUserInfoDto
                {
                    Branch = user.BranchOld,
                    EmailAddress = user.EmailAddress,
                    FullName = user.FullName,
                    KomuUserId = user.KomuUserId,
                    Type = user.Type,
                    UserId = user.Id,
                    BranchDisplayName = user.Branch.DisplayName
                }).FirstOrDefaultAsync();
        }

        public async Task<List<NotifyKomuTimesheetDto>> getReceiverList(List<MyTimesheet> mytimesheets)
        {
            var mytimesheetIds = mytimesheets.Select(s => s.Id).ToList();

            var projectTimesheets = await WorkScope.GetRepo<MyTimesheet>().GetAllIncluding(s => s.ProjectTask, s => s.ProjectTask.Project, s => s.ProjectTask.Task, s => s.User)
                .Where(s => mytimesheetIds.Contains(s.Id))
                .GroupBy(s => new { s.ProjectTask.ProjectId, s.ProjectTask.Project.Code, s.ProjectTask.Project.Name })
                .Select(s => new
                {
                    s.Key.ProjectId,
                    s.Key.Code,
                    s.Key.Name,
                    TimeSheets = s.Select(x => new TimesheetKomuDto
                    {
                        Id = x.Id,
                        DateAt = x.DateAt,
                        WorkingTime = x.WorkingTime,
                        TypeOfWork = x.TypeOfWork,
                        IsCharged = x.IsCharged,
                        Note = x.Note,
                        TaskName = x.ProjectTask.Task.Name,
                        IsUnlockedByEmployee = x.IsUnlockedByEmployee
                    }).ToList()
                }).ToListAsync();

            var projectIds = projectTimesheets.Select(s => s.ProjectId).ToList();


            var pms = await WorkScope.GetAll<ProjectUser>()
            .Where(s => s.Project.Status == ProjectStatus.Active)
            .Where(s => s.Type == ProjectUserType.PM)
            .Where(s => projectIds.Contains(s.ProjectId))
            .GroupBy(s => new { s.ProjectId, s.Project.KomuChannelId, IsNotifyToKomu = s.Project.IsNoticeKMSubmitTS })
            .Select(s => new
            {
                ProjectId = s.Key.ProjectId,
                KomuChannelId = s.Key.KomuChannelId,
                IsNotifyToKomu = s.Key.IsNotifyToKomu,
                Emails = s.Select(x => x.User.EmailAddress),

                PMs = s.Select(x => new NotifyUserInfoDto
                {
                    EmailAddress = x.User.EmailAddress,
                    FullName = x.User.FullName,
                    KomuUserId = x.User.KomuUserId,
                    UserId = x.UserId
                }).ToList()

            }).ToListAsync();

            var result = (from pt in projectTimesheets
                          join pm in pms on pt.ProjectId equals pm.ProjectId
                          select new NotifyKomuTimesheetDto
                          {
                              KomuChannelId = pm.KomuChannelId,
                              IsNotifyKomu = pm.IsNotifyToKomu,
                              ProjectId = pt.ProjectId,
                              Timesheets = pt.TimeSheets,
                              ProjectCode = pt.Code,
                              ProjectName = pt.Name,
                              Emails = pm.Emails.ToList(),
                              PMs = pm.PMs

                          }).ToList();

            return result;
        }

        private async System.Threading.Tasks.Task notifySubmitTimesheet(List<MyTimesheet> mytimesheets)
        {
            var SendEmailSubmitTimesheet = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailTimesheet);
            var NotifyKomuWhenSubmitTimesheet = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuSubmitTimesheet);
            if (NotifyKomuWhenSubmitTimesheet != "true" && SendEmailSubmitTimesheet != "true")
            {
                Logger.Info("SendEmailSubmitTimesheet=" + SendEmailSubmitTimesheet + ", AbpSessionUserId=" + AbpSession.UserId);
                Logger.Info("NotifyKomuWhenSubmitTimesheet=" + NotifyKomuWhenSubmitTimesheet + ", AbpSessionUserId=" + AbpSession.UserId);
                return;
            }
            var requester = await getNotifyUserInfoDto(AbpSession.UserId.Value);
            var receivers = await getReceiverList(mytimesheets);

            notifyKomuWhenSubmitTimesheet(requester, receivers);
            await notifyEmailWhenSubmitTimesheet(requester, receivers);
        }

        private void notifyKomuWhenSubmitTimesheet(NotifyUserInfoDto requester, List<NotifyKomuTimesheetDto> receivers)
        {
            var NotifyKomuWhenSubmitTimesheet = SettingManager.GetSettingValueForApplication(AppSettingNames.SendKomuSubmitTimesheet);
            if (NotifyKomuWhenSubmitTimesheet != "true")
            {
                Logger.Info("NotifyKomuWhenSubmitTimesheet != true");
                return;
            }

            var sb = new StringBuilder();
            foreach (var project in receivers)
            {
                if (!project.IsNotifyKomu)
                {
                    Logger.Info($"notifyKomuWhenSubmitRequest() projectId={project.ProjectId}, IsNotifyKomu={project.IsNotifyKomu}, KomuChannelId={project.KomuChannelId}");
                }
                else
                {
                    sb.Clear();
                    sb.AppendLine($"PM {project.KomuPMsTag()}: {requester.KomuAccountInfo} has submitted following timesheets:");
                    sb.AppendLine("```");
                    sb.Append(project.TimesheetsKomuMsg());
                    sb.AppendLine("```");
                    _komuService.NotifyToChannel(sb.ToString(), project.KomuChannelId);
                    Logger.Info(sb.ToString());

                }
            }
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.MyTimesheet_Edit)]
        public async Task<MyTimesheetDto> SaveAndReset(MyTimesheetDto input)
        {
            List<MyTimesheet> myTimesheets = new List<MyTimesheet>();

            if (input.WorkingTime < 0)
                throw new UserFriendlyException("You can't log this time sheet");

            var mts = await WorkScope.GetAsync<MyTimesheet>(input.Id);

            if (mts == null || mts.Status != TimesheetStatus.Reject)
                throw new UserFriendlyException(string.Format("MyTimesheet Id {0} is not exist or status is not rejected.", input.Id));

            if (input.TypeOfWork == TypeOfWork.NormalWorkingHours)
            {
                //await validLogTimesheetOnDateOff(input.DateAt);
                //var maxHourCanLog = int.Parse(SettingManager.GetSettingValue(AppSettingNames.MaxTimeSheetHourPerDay));
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime - mts.WorkingTime, maxHourCanLog);
                //await validLogTimesheetOver8h(input.DateAt, input.WorkingTime - mts.WorkingTime);
            }
            else
            {
                await validUpdateNormalToOT(mts, input);
            }
            validTotalLogTimesheet(input.DateAt, input.WorkingTime - mts.WorkingTime, getMaxHourCanLog());

            ObjectMapper.Map(input, mts);
            mts.Status = TimesheetStatus.Pending;
            mts.IsUnlockedByEmployee = true;
            await WorkScope.UpdateAsync<MyTimesheet>(mts);
            myTimesheets.Add(mts);
            await notifySubmitTimesheet(myTimesheets);

            return input;
        }
        private int getMaxHourCanLog()
        {
            return int.Parse(SettingManager.GetSettingValue(AppSettingNames.MaxTimeSheetHourPerDay));
        }
        private async Task<ValidateTimesheetResult> calculateAllowMinuteUserCanLogTS(long userId, DateTime dateAt)
        {
            var isDayoff = await WorkScope.GetAll<DayOffSetting>()
                .AnyAsync(s => s.DayOff == dateAt);

            if (isDayoff)
            {
                return new ValidateTimesheetResult
                {
                    AllowWorkingMitue = 0,
                    Message = string.Format("Fail! {0} is not a working day.", dateAt.ToString("yyyy-MM-dd"))
                };
            }

            int loggedWorkingMinute = WorkScope.GetAll<MyTimesheet>()
           .Where(s => s.UserId == userId && s.DateAt.Date == dateAt)
           .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
           .Sum(s => s.WorkingTime);

            string message = "";
            if (loggedWorkingMinute > 0)
            {
                message = "Fail! You already logged " + (loggedWorkingMinute / 60) + "h. ";
            }
            else
            {
                loggedWorkingMinute = 0;
            }

            int totalAllowWorkingMinute = 8 * 60;

            if (dateAt.DayOfWeek == DayOfWeek.Saturday)
            {
                totalAllowWorkingMinute = 4 * 60;
            }

            var offHour = await WorkScope.GetAll<AbsenceDayDetail>()
                .Where(s => s.Request.UserId == userId && s.DateAt.Date == dateAt)
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.Request.Status != RequestStatus.Rejected)
                .Select(s => s.Hour)
                .FirstOrDefaultAsync();

            if (offHour == default)
            {
                offHour = 0;
            }
            else
            {
                message += "Fail! You already requested off " + offHour + "h. ";
            }
            message += ". Date at " + dateAt.ToString("yyyy-MM-dd");

            int allowWorkingMitue = totalAllowWorkingMinute - loggedWorkingMinute - (int)(offHour * 60);

            return new ValidateTimesheetResult
            {
                AllowWorkingMitue = allowWorkingMitue,
                Message = message
            };

        }


        private DateTime? GetLastWorkingDate(long userId)
        {
            DateTime today = DateTimeUtils.GetNow().Date;
            int count = 7;

            var settingOffDates = WorkScope.GetAll<DayOffSetting>()
                .Where(s => s.DayOff > today.AddDays(-count))
                .Select(s => s.DayOff.Date)
                .ToList();

            var userOffDates = WorkScope.GetAll<AbsenceDayDetail>()
                .Select(s => new { s.Request.UserId, s.DateAt.Date, RequestType = s.Request.Type, s.DateType, s.Request.Status })
                .Where(s => s.UserId == userId && s.Status != RequestStatus.Rejected)
                .Where(s => s.Date > today.AddDays(-count))
                .Where(s => s.RequestType == RequestType.Off && s.DateType == DayType.Fullday)
                .Select(s => s.Date)
                .ToList();

            var exceptDates = settingOffDates.Union(userOffDates).ToHashSet();


            var lastWorkingDate = today.AddDays(-1);
            while(count >=0)
            {
                if (!exceptDates.Contains(lastWorkingDate)
                    && lastWorkingDate.DayOfWeek != DayOfWeek.Sunday
                    && lastWorkingDate.DayOfWeek != DayOfWeek.Saturday
                    && DateTimeUtils.IsTheSameWeek(today, lastWorkingDate)
                    && lastWorkingDate.Month == today.Month)
                {
                    return lastWorkingDate;
                }
                lastWorkingDate = lastWorkingDate.AddDays(-1);
                count--;
            }

            return null;

        }

        [AbpAllowAnonymous]
        [NccAuthentication]
        public async Task<string> CreateByKomu(MyTimesheetByKomuDto input)
        {
            if (input.Hour <= 0)
            {
                input.Hour = 1;
            }

            var dateAt = input.DateAt.HasValue ? input.DateAt.Value.Date : DateTimeUtils.GetNow().Date;

            var user = await WorkScope.GetAll<User>().Where(s => s.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim())
                .Select(s => new { s.Id, s.DefaultProjectTaskId, s.BranchId, s.IsActive })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return "Failed! Not found user with emailAddress = " + input.EmailAddress;
            }

            if (!user.IsActive)
            {
                return "Failed! User is inactive with emailAddress = " + input.EmailAddress;
            }

            if (!user.DefaultProjectTaskId.HasValue)
            {
                return "Failed! Not set Project Task Default yet. Go to timesheet.nccsoft.vn > My timesheet > Add new timesheet to set it.";
            }

            var userId = user.Id;
            var defaultProjectTaskId = user.DefaultProjectTaskId;

            var projectTask = await WorkScope.GetAll<ProjectTask>()
                               .Include(s => s.Task)
                               .Include(s => s.Project)
                               .Where(s => s.Id == defaultProjectTaskId)
                               .Select(s => new
                               {
                                   s.Id,
                                   ProjectName = s.Project.Name,
                                   TaskName = s.Task.Name,
                                   s.ProjectId
                               }).FirstOrDefaultAsync();

            if (projectTask == default)
            {
                return "Fail! Not found projectTask in DB with Id " + defaultProjectTaskId;
            }

            var projectUser = WorkScope.GetAll<ProjectUser>()
                .Where(p => p.ProjectId == projectTask.ProjectId)
                .Where(p => p.Project.Status != ProjectStatus.Deactive)
                .Where(p => p.Type != ProjectUserType.DeActive)
                .Where(p => p.UserId == userId)
                .Select(s => new { s.IsTemp })
                .FirstOrDefault();

            if (projectUser == default)
            {
                return "Fail! User is not in this project or project is closed";
            }

            var myTS = WorkScope.GetAll<MyTimesheet>()
                .Include(s => s.ProjectTask)
                .Where(s => s.ProjectTask.ProjectId == projectTask.ProjectId)
                .Where(s => s.DateAt.Date == dateAt)
                .Where(s => s.UserId == userId)
                .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .FirstOrDefault();

            if (myTS != default)
            {
                if(myTS.Status == TimesheetStatus.Approve)
                {
                    return "Fail! Timesheet của bạn đã được Approve";
                }
                myTS.Note = input.Note;
                myTS.WorkingTime = (int)(input.Hour * 60);
                WorkScope.Update(myTS);
            }
            else
            {
                myTS = new MyTimesheet
                {
                    DateAt = dateAt,
                    UserId = userId,
                    TypeOfWork = TypeOfWork.NormalWorkingHours,
                    IsCharged = false,
                    Note = input.Note,
                    ProjectTaskId = defaultProjectTaskId.Value,
                    Status = TimesheetStatus.None,
                    WorkingTime = (int)(input.Hour * 60),
                    IsTemp = projectUser.IsTemp
                };

                await WorkScope.InsertAsync(myTS);
            }

            return string.Format("Success! Bạn đã log: **{0}** task: **{1}** project: **{2}** ngày **{3}** với note là : **{4}**",
                        CommonUtils.ConvertHourToHHmm(myTS.WorkingTime),
                        projectTask.TaskName,
                        projectTask.ProjectName,
                        myTS.DateAt.ToString("yyyy-MM-dd"),
                        input.Note
                    );
        }


        [AbpAllowAnonymous]
        [NccAuthentication]
        public async Task<string> CreateFullByKomu(MyFullTimesheetByKomuDto input)
        {
            var user = await WorkScope.GetAll<User>()
                .Where(s => s.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim())
                .Select(s => new { s.Id, s.BranchId, s.IsActive })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return "Fail! Not found user with emailAddress = " + input.EmailAddress;
            }

            if (!user.IsActive)
            {
                return "Fail! User with emailAddress " + input.EmailAddress + " inactive";
            }

            var projectUser = WorkScope.GetAll<ProjectUser>()
                .Where(p => p.Project.Code.ToLower().Trim() == input.ProjectCode.ToLower().Trim())
                .Where(p => p.Project.Status != ProjectStatus.Deactive)
                .Where(p => p.Type != ProjectUserType.DeActive)
                .Where(p => p.User.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim())
                .Select(s => new { s.IsTemp })
                .FirstOrDefault();

            if (projectUser == default)
            {
                return "Fail! Not found project with code " + input.ProjectCode + " or User is not in this project or project is closed";
            }

            var projectTask = WorkScope.GetAll<ProjectTask>()
                .Where(t => t.Task.Name.ToLower() == input.TaskName.ToLower().Trim())
                .Where(p => p.Project.Code.ToLower().Trim() == input.ProjectCode.ToLower().Trim())
                .Select(t => new { t.Id, t.Project.Name })
                .FirstOrDefault();

            if (projectTask == null)
            {
                return "Fail! Not found task with name " + input.TaskName + " or the task is not in this project";
            }

            DateTime today = DateTimeUtils.GetNow().Date;
            int workingMinute = input.Hour > 0 ? (int)(input.Hour * 60) : 0;
            if (workingMinute <= 0)
            {
                var validate = await calculateAllowMinuteUserCanLogTS(user.Id, today);
                workingMinute = validate.AllowWorkingMitue;
            }

            if (workingMinute <= 0)
            {
                return CommonUtils.ConvertHourToHHmm(workingMinute) + " is illegal because this value must be than 0h";
            }
            var timesheet = new MyTimesheet
            {
                DateAt = today,
                UserId = user.Id,
                TypeOfWork = TypeOfWork.NormalWorkingHours,
                WorkingTime = workingMinute,
                Note = input.Note,
                ProjectTaskId = projectTask.Id,
                Status = TimesheetStatus.None,
                IsTemp = projectUser.IsTemp
            };

            await WorkScope.InsertAsync(timesheet);

            return $"Success! Bạn đã log ```{CommonUtils.ConvertHourToHHmm(timesheet.WorkingTime)}``` " +
                $"cho task ```{input.TaskName}``` của dự án ```{projectTask.Name}``` " +
                $"ngày ```{timesheet.DateAt.ToString("yyyy-MM-dd")}``` " +
                $"dạng {(timesheet.IsTemp ? "(temp)" : "(official)")} với note là : ```{input.Note}```";
        }

        [HttpGet]
        public WarningMyTimesheetDto WarningMyTimesheet(DateTime dateAt, int workingTime,int? timesheetId)
        {
            var userId = AbpSession.UserId.Value;
            var timekeepings = WorkScope.GetAll<Timekeeping>()
                    .Where(x => x.UserId == userId)
                    .Where(x => x.DateAt == dateAt.Date)
                    .Select(x => new
                    {
                        x.CheckIn,
                        x.CheckOut
                    }).FirstOrDefault();

            int workingTimeLogged = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.UserId == userId)
                .Where(s => s.DateAt == dateAt.Date)
                .Where(s => !timesheetId.HasValue || s.Id != timesheetId)
                .Where(s => s.Status != TimesheetStatus.Reject)
                .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .Select(s => s.WorkingTime).Sum();

            var absencedays = WorkScope.GetAll<AbsenceDayDetail>()
             .Where(s => s.Request.UserId == userId)
             .Where(s => s.DateAt == dateAt.Date)
             .Where(s => s.Request.Type == RequestType.Off)
             .Where(s => s.Request.Status != RequestStatus.Rejected)
             .Select(s => new
             {
                 DateType = s.DateType,
                 Hour = s.Hour,
                 Type = s.Request.Type,
                 AbsenceTime = s.AbsenceTime
             }).ToList();

            var isDayOffSetting =  WorkScope.GetAll<DayOffSetting>()
                     .Where(s => s.DayOff.Date == dateAt.Date)
                     .Select(s => s.DayOff.Date).Any();

            return new WarningMyTimesheetDto()
            {
                UserId = userId,
                DateAt = dateAt,
                HourOff = absencedays.Where(s => s.DateType != DayType.Custom).Select(x => x.Hour).Sum(),
                HourDiMuon = absencedays.Where(s => s.DateType == DayType.Custom)
                                        .Where(s => s.AbsenceTime == OnDayType.DiMuon)
                                        .Select(x => x.Hour).FirstOrDefault(),
                HourVeSom = absencedays.Where(s => s.DateType == DayType.Custom)
                                       .Where(s => s.AbsenceTime == OnDayType.VeSom)
                                       .Select(x => x.Hour).FirstOrDefault(),
                IsOffHalfDay = absencedays.Where(s => (s.DateType == DayType.Morning || s.DateType == DayType.Afternoon)).Any(),
                WorkingTime = workingTime,
                WorkingTimeLogged = workingTimeLogged,
                CheckIn = timekeepings == default ? null : timekeepings.CheckIn,
                CheckOut = timekeepings == default ? null : timekeepings.CheckOut,
                IsOffDay = isDayOffSetting,
            };
        }
    }
}