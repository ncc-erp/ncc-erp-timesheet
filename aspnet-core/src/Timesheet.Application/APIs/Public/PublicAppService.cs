using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.HRM.Dto;
using Timesheet.APIs.OverTimeHours;
using Timesheet.APIs.OverTimeHours.Dto;
using Timesheet.APIs.ProjectManagement.Dto;
using Timesheet.APIs.Public.Dto;
using Timesheet.APIs.ReviewInterns.Dto;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Public
{

    public class PublicAppService : AppServiceBase
    {
        private readonly OverTimeHourAppService _overTimeHourAppService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkScope _ws;
        public PublicAppService(OverTimeHourAppService overTimeHourAppService, IHttpContextAccessor httpContextAccessor, IWorkScope ws) : base(ws)
        {
            _overTimeHourAppService = overTimeHourAppService;
            _ws = ws;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<UserWorkFromHomeDto>> GetUserWorkFromHome(DateTime? date)
        {
            if (!date.HasValue)
            {
                date = DateTimeUtils.GetNow().Date;
            }
            return await WorkScope.GetAll<AbsenceDayDetail>()
            .Where(s => s.Request.Status != RequestStatus.Rejected)
            .Where(s => s.Request.Type == RequestType.Remote)
            .Where(s => s.DateAt == date.Value.Date)
            .Select(s => new UserWorkFromHomeDto
            {
                UserId = s.Request.UserId,
                EmailAddress = s.Request.User.EmailAddress,
                DateAt = date,
                Status = Enum.GetName(typeof(RequestStatus), s.Request.Status),
                CreationTime = s.CreationTime
            }).ToListAsync();
        }

        public async Task<List<WorkingStatusUserDto>> GetAllUserLeaveDay(DateTime? date)
        {
            DateTime dateAt = date.HasValue ? date.Value.Date : DateTimeUtils.GetNow().Date;
            return await queryAbsenceDay(dateAt)
                .Where(s => s.RequestType == RequestType.Off)
                .ToListAsync();
        }

        [HttpPost]
        [AbpAllowAnonymous]
        public List<UserInfoByEmail> GetAllUserByEmail(List<string> listEmail)
        {

            var qproject = _ws.GetAll<Project>()
                .Where(s => s.Status == ProjectStatus.Active)
                .Where(s => !s.isAllUserBelongTo)
                .Select(s => new { s.Id, s.Name });

            var qprojectUsers = from pu in _ws.GetAll<ProjectUser>()
                                .Where(s => s.User.IsActive && s.Type != ProjectUserType.DeActive)
                                join p in qproject on pu.ProjectId equals p.Id
                                select new
                                {
                                    pu.ProjectId,
                                    pu.UserId,
                                    pu.Type,
                                    ProjectName = p.Name,

                                };



            var dicUserIdToUserInfo = _ws.GetAll<User>()
                .Where(s => s.IsActive)
                .Select(s => new UserInfoByEmail
                {
                    UserId = s.Id,
                    FullName = s.FullName,
                    Branch = s.Branch.DisplayName,
                    UserType = s.Type,
                    EmailAddress = s.EmailAddress,
                }).ToDictionary(s => s.UserId);

            var userIds = dicUserIdToUserInfo.Keys;

            var dicUserIdToListProject = qprojectUsers
                .Where(s => userIds.Contains(s.UserId))
                .GroupBy(s => s.UserId)
                .ToDictionary(s => s.Key, s => s.Select(x => new { x.ProjectId, x.ProjectName }).ToList());


            var dicProjectIdToListPMId = qprojectUsers
                .Where(s => s.Type == ProjectUserType.PM)
                .GroupBy(s => s.ProjectId)
                .ToDictionary(s => s.Key, s => s.Select(x => x.UserId).ToList());

            var results = new List<UserInfoByEmail>();
            foreach (var item in dicUserIdToUserInfo)
            {
                var userId = item.Key;
                var userInfo = item.Value;
                if (!listEmail.Contains(userInfo.EmailAddress))
                {
                    continue;
                }
                if (dicUserIdToListProject.ContainsKey(userId))
                {
                    var projects = dicUserIdToListProject[userId];

                    userInfo.ProjectUsers = projects.Select(s => new PDto
                    {
                        ProjectId = s.ProjectId,
                        ProjectName = s.ProjectName,
                        Pms = dicProjectIdToListPMId.ContainsKey(s.ProjectId) ?
                        dicProjectIdToListPMId[s.ProjectId]
                        .Where(pmId => dicUserIdToUserInfo.ContainsKey(pmId))
                        .Select(pmId => dicUserIdToUserInfo[pmId].FullName).ToList()
                        : new List<string>()
                    }).ToList();

                }
                results.Add(userInfo);


            }

            return results;
        }

        public async Task<WorkingStatusUserDto> GetWorkingStatusByUser(string emailAddress, DateTime? date)
        {
            DateTime dateAt = date.HasValue ? date.Value.Date : DateTimeUtils.GetNow().Date;
            return await queryAbsenceDay(dateAt)
                .Where(s => s.EmailAddress == emailAddress)
                .FirstOrDefaultAsync();
        }

        private IQueryable<WorkingStatusUserDto> queryAbsenceDay(DateTime dateAt)
        {
            return WorkScope.GetAll<AbsenceDayDetail>()
                .Include(s => s.Request)
                .Include(s => s.Request.User)
                .Where(s => s.Request.Status != RequestStatus.Rejected)
                .Where(s => s.DateAt.Date == dateAt)
                .Select(s => new WorkingStatusUserDto
                {
                    EmailAddress = s.Request.User.EmailAddress.ToLower().Trim(),
                    DateAt = s.DateAt,
                    RequestType = s.Request.Type,
                    DayType = s.DateType,
                    OnDayType = s.AbsenceTime,
                    Hour = s.Hour,
                    Status = s.Request.Status
                });
        }


        public async Task<Object> GetAllUser()
        {
            var users = await WorkScope.GetAll<User>()
                .Where(s => s.IsActive)
                .Where(s => !s.IsStopWork)
               .Select(s => new
               {
                   s.EmailAddress,
                   s.FullName,
                   s.Sex,
                   s.BranchId
               })
               .ToListAsync();

            return users;

        }

        [HttpGet]
        public async Task<List<KmUserDto>> KmGetAllUsers()
        {
            var users = await WorkScope.GetAll<User>().Select(x => new KmUserDto()
            {
                FullName = x.FullName,
                EmailAddress = x.EmailAddress,
                Surname = x.Surname,
                UserName = x.UserName,
                Name = x.Name
            }).ToListAsync();
            return users;
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<KmProjectDto>> GetAllProject()
        {
            var data = await WorkScope.GetAll<Project>()
                .Select(s => new KmProjectDto
                {
                    Id = s.Id,
                    Name = s.Name.Normalize(),
                    Code = s.Code,
                    KomuChannelId = s.KomuChannelId
                }).ToListAsync();
            return data;
        }

        public async Task<string> CheckUserLogEnoughTimesheetThisWeek(string EmailAddress)
        {
            var user = await WorkScope.GetAll<User>().Where(s => s.EmailAddress.ToLower().Trim() == EmailAddress.ToLower().Trim())
               .Select(s => new { s.Id, s.IsActive })
               .FirstOrDefaultAsync();

            if (user == null)
            {
                return "Fail! Not found user with emailAddress = " + EmailAddress;
            }

            if (!user.IsActive)
            {
                return "Fail! user is inactive with emailAddress = " + EmailAddress;
            }

            var userId = user.Id;

            DateTime today = DateTimeUtils.GetNow().Date;

            DateTime firstDayOfWeek = DateTimeUtils.FirstDayOfWeek(today);
            DateTime lastDayOfWeek = DateTimeUtils.LastDayOfWeek(today);

            var totalLoggedWeekMinute = WorkScope.GetAll<MyTimesheet>()
               .Where(s => s.UserId == userId)
               .Where(s => s.DateAt >= firstDayOfWeek && s.DateAt.Date <= lastDayOfWeek)
               .Where(s => s.Status == TimesheetStatus.Pending || s.Status == TimesheetStatus.Approve)
               .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
               .Sum(s => s.WorkingTime);

            var totalOffWeekHour = await WorkScope.GetAll<AbsenceDayDetail>()
                .Where(s => s.Request.UserId == userId)
                   .Where(s => s.DateAt >= firstDayOfWeek && s.DateAt.Date <= lastDayOfWeek)
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.Request.Status == RequestStatus.Pending || s.Request.Status == RequestStatus.Approved)
                .SumAsync(s => s.Hour);

            var missMinute = 40 * 60 - totalOffWeekHour * 60 - totalLoggedWeekMinute;

            if (missMinute > 0)
            {
                return string.Format("This week is NOT OK! Off:```{0}```Logged normal working time:```{1}```Miss hour:```{2}```",
                  CommonUtils.ConvertHourToHHmm((int)(totalOffWeekHour * 60)),
                  CommonUtils.ConvertHourToHHmm(totalLoggedWeekMinute),
                  CommonUtils.ConvertHourToHHmm((int)missMinute)
              );
            }

            return string.Format("This week is OK! Off: ```{0}``` Logged normal working time: ```{1}```",
             CommonUtils.ConvertHourToHHmm((int)(totalOffWeekHour * 60)),
             CommonUtils.ConvertHourToHHmm(totalLoggedWeekMinute)
            );
        }


        public async Task<List<UserWorkingStatusDto>> GetListUserLogTimesheetThisWeekNotOk()
        {
            var users = await WorkScope.GetAll<User>()
                .Where(s => s.IsActive)
                .Where(s => !s.IsStopWork)
               .Select(s => new
               {
                   s.Id,
                   s.EmailAddress,
                   s.FullName
               }).ToListAsync();


            DateTime firstDayOfWeek = DateTimeUtils.FirstDayOfCurrentyWeek();
            DateTime lastDayOfWeek = DateTimeUtils.LastDayOfCurrentWeek();

            var mapUserIdToWorkingMinutes = await WorkScope.GetAll<MyTimesheet>()
               .Where(s => s.DateAt >= firstDayOfWeek && s.DateAt.Date <= lastDayOfWeek)
               .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
               .Where(s => s.Status == TimesheetStatus.Pending || s.Status == TimesheetStatus.Approve)
               .GroupBy(s => s.UserId)
               .Select(s => new { UserId = s.Key, WorkingTime = s.Sum(x => x.WorkingTime) })
               .ToDictionaryAsync(s => s.UserId);

            var mapUserIdToOffHour = await WorkScope.GetAll<AbsenceDayDetail>()
                .Where(s => s.DateAt >= firstDayOfWeek && s.DateAt.Date <= lastDayOfWeek)
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.Request.Status == RequestStatus.Approved)
                .GroupBy(s => s.Request.UserId)
                .Select(s => new { UserId = s.Key, OffHour = s.Sum(x => x.Hour) })
                .ToDictionaryAsync(s => s.UserId);

            var result = new List<UserWorkingStatusDto>();
            foreach (var user in users)
            {
                var totalLoggedWeekMinute = mapUserIdToWorkingMinutes.ContainsKey(user.Id) ? mapUserIdToWorkingMinutes[user.Id].WorkingTime : 0;
                var totalOffWeekHour = mapUserIdToOffHour.ContainsKey(user.Id) ? mapUserIdToOffHour[user.Id].OffHour : 0;
                var missMinute = 40 * 60 - totalOffWeekHour * 60 - totalLoggedWeekMinute;
                if (missMinute > 0)
                {
                    var sresult = string.Format("This week is NOT OK! Off:```{0}```PENDING, APPROVED normal working timesheet:```{1}```Miss hour:```{2}```",
                      CommonUtils.ConvertHourToHHmm((int)(totalOffWeekHour * 60)),
                      CommonUtils.ConvertHourToHHmm(totalLoggedWeekMinute),
                      CommonUtils.ConvertHourToHHmm((int)missMinute));

                    result.Add(new UserWorkingStatusDto
                    {
                        EmailAddress = user.EmailAddress,
                        FullName = user.FullName,
                        Result = sresult
                    });
                }
            }

            return result;


        }


        [HttpGet]
        [AbpAllowAnonymous]
        public async Task<TotalWorkingTimeDto> GetTimesheetOfUserInProject(string projectCode, string emailAddress, DateTime startDate, DateTime endDate)
        {
            return await WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.Status == TimesheetStatus.Approve)
                .Where(s => s.DateAt >= startDate.Date && s.DateAt.Date <= endDate)
                .Where(s => s.ProjectTask.Project.Code.ToLower().Trim() == projectCode.ToLower().Trim())
                .Where(s => s.User.EmailAddress.ToLower().Trim() == emailAddress.ToLower().Trim())
                .GroupBy(s => new { s.ProjectTask.ProjectId, s.ProjectTask.Project.Code })
                .Select(s => new TotalWorkingTimeDto
                {
                    ProjectId = s.Key.ProjectId,
                    ProjectCode = s.Key.Code,
                    NormalWorkingMinute = s.Where(x => x.TypeOfWork == TypeOfWork.NormalWorkingHours).Sum(x => x.WorkingTime),
                    OTMinute = s.Where(x => x.TypeOfWork == TypeOfWork.OverTime).Sum(x => x.WorkingTime),
                    OTNoChargeMinute = s.Where(x => x.TypeOfWork == TypeOfWork.OverTime && !x.IsCharged).Sum(x => x.WorkingTime),
                }).FirstOrDefaultAsync();

        }
        [HttpGet]
        [AbpAllowAnonymous]
        public TotalWorkingTimeDto GetTimesheetOfUserInProjectNew(string projectCode, string emailAddress, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(projectCode))
            {
                throw new UserFriendlyException("project code is null or empty");
            }
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new UserFriendlyException("project code is null or empty");
            }
            emailAddress = emailAddress.ToLower().Trim();
            var normalWorkingTimeStandard = GetNormalWorkingTimeStandardOfUser(emailAddress, startDate, endDate);

            var qMyWorkingTime = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.Status == TimesheetStatus.Approve)
                .Where(s => s.DateAt >= startDate.Date && s.DateAt.Date <= endDate)
                .Where(s => s.User.EmailAddress.ToLower().Trim() == emailAddress);

            var openTalkId = long.Parse(SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OpenTalkTaskId).Result);

            var normalWorkingTimeAll = (double)(qMyWorkingTime
                                                .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                                                .Where(s => s.ProjectTaskId != openTalkId)
                                                .Sum(s => s.WorkingTime))
                                                / 60;

            var qMyWorkingTimeOfProject = qMyWorkingTime.Where(s => s.ProjectTask.Project.Code.ToLower().Trim() == projectCode.ToLower().Trim());

            long projectId = qMyWorkingTimeOfProject.Select(s => s.ProjectTask.ProjectId).FirstOrDefault();
            var normalWorkingMinute = qMyWorkingTimeOfProject.Where(x => x.TypeOfWork == TypeOfWork.NormalWorkingHours).Sum(x => x.WorkingTime);
            var myOTMinute = qMyWorkingTimeOfProject.Where(x => x.TypeOfWork == TypeOfWork.OverTime).Sum(x => x.WorkingTime);
            var myOTNoChargeMinute = qMyWorkingTimeOfProject.Where(x => x.TypeOfWork == TypeOfWork.OverTime && !x.IsCharged).Sum(x => x.WorkingTime);

            return new TotalWorkingTimeDto
            {
                ProjectId = projectId,
                ProjectCode = projectCode,
                NormalWorkingMinute = normalWorkingMinute,
                OTMinute = myOTMinute,
                OTNoChargeMinute = myOTNoChargeMinute,
                NormalWorkingTimeAll = normalWorkingTimeAll,
                NormalWorkingTimeStandard = normalWorkingTimeStandard,
            };
        }
        private double GetNormalWorkingTimeStandardOfUser(string emailAddress, DateTime startDate, DateTime endDate)
        {
            return GetNormalWorkingTimeStandard(startDate, endDate) - GetHourOffOfUser(emailAddress, startDate, endDate);
        }
        private double GetHourOffOfUser(string emailAddress, DateTime startDate, DateTime endDate)
        {
            var hour = WorkScope.GetAll<AbsenceDayDetail>()
                                         .Where(s => s.DateAt.Date >= startDate && s.DateAt.Date <= endDate)
                                         .Where(s => s.Request.Type == RequestType.Off)
                                         .Where(s => s.Request.Status != RequestStatus.Rejected)
                                         .Where(s => s.Request.User.EmailAddress.ToLower().Trim() == emailAddress)
                                         .Sum(s => s.Hour);

            return hour;
        }
        private double GetNormalWorkingTimeStandard(DateTime startDate, DateTime endDate)
        {
            var dayOffs = WorkScope.GetAll<DayOffSetting>()
                                         .Where(s => s.DayOff.Date >= startDate && s.DayOff.Date <= endDate)
                                         .Select(s => s.DayOff);

            var normalWorkingTimeStandard = 0;

            var i = startDate.Date;
            while (i <= endDate)
            {
                if (!
                    (dayOffs.Any(s => s.Date == i.Date)
                    || i.DayOfWeek == DayOfWeek.Saturday
                    || i.DayOfWeek == DayOfWeek.Sunday))
                {
                    normalWorkingTimeStandard += 8;
                }

                i = i.AddDays(1);
            }
            return normalWorkingTimeStandard;
        }

        [HttpPost]
        [AbpAllowAnonymous]
        public List<UserInfoToProject> GetUserInProjectFromTimesheet(string projectCode, List<string> exclusionEmails, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(projectCode))
            {
                Logger.Debug("projectCode is null or empty");
                return default;
            }
            projectCode = projectCode.ToLower().Trim();
            if (exclusionEmails == null || exclusionEmails.Count() == 0)
            {
                Logger.Debug("emailAddress is null or empty");
            }

            var userIds = WorkScope.GetAll<ProjectUser>()
                                     .Select(s => new { s.UserId, s.User.EmailAddress, Code = s.Project.Code.ToLower().Trim(), s.Type })
                                     .Where(s => s.Code == projectCode)
                                     .Where(s => s.Type != ProjectUserType.DeActive)
                                     .Where(s => !exclusionEmails.Contains(s.EmailAddress))
                                     .Select(s => s.UserId);

            var userIdsLogTS = WorkScope.GetAll<MyTimesheet>()
                                          .Select(s => new
                                          {
                                              s.DateAt,
                                              s.Status,
                                              ProjectCode = s.ProjectTask.Project.Code.ToLower().Trim(),
                                              s.UserId,
                                          })
                                          .Where(s => s.DateAt >= startDate.Date)
                                          .Where(s => s.DateAt.Date <= endDate)
                                          .Where(s => s.Status == TimesheetStatus.Approve)
                                          .Where(s => s.ProjectCode == projectCode)
                                          .Where(s => userIds.Contains(s.UserId))
                                          .Select(s => s.UserId)
                                          .Distinct()
                                          .ToList();

            var usersInfoToHRM = WorkScope.GetAll<User>()
                                          .Where(s => userIdsLogTS.Contains(s.Id))
                                          .Select(s => new UserInfoToProject
                                          {
                                              UserId = s.Id,
                                              FullName = s.FullName,
                                              IsActive = s.IsActive,
                                              Branch = s.Branch.Id,
                                              BranchColor = s.Branch.Color,
                                              BranchDisplayName = s.Branch.DisplayName,
                                              AvatarPath = s.AvatarPath,
                                              EmailAddress = s.EmailAddress,
                                              UserType = s.Type,
                                              UserLevel = s.Level,
                                          }).ToList();
            return usersInfoToHRM;
        }


        [AbpAllowAnonymous]
        public async Task<TimesheetChartDto> GetTimesheetWeeklyChartOfUserInProject(string projectCode, string emailAddress, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(projectCode))
            {
                throw new UserFriendlyException("project code is null or empty");
            }

            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new UserFriendlyException("emailAddress is null or empty");
            }

            return await GetTimesheetWeeklyChart(projectCode, new List<string>() { emailAddress }, startDate, endDate);
        }


        [AbpAllowAnonymous]
        public async Task<TimesheetChartDto> GetTimesheetWeeklyChartOfProject(string projectCode, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(projectCode))
            {
                throw new UserFriendlyException("project code is null or empty");
            }
            return await GetTimesheetWeeklyChart(projectCode, null, startDate, endDate);
        }

        [AbpAllowAnonymous]
        public async Task<TimesheetChartDto> GetTimesheetWeeklyChartOfUser(string emailAddress, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new UserFriendlyException("emailAddress is null or empty");
            }
            return await GetTimesheetWeeklyChart(null, new List<string>() { emailAddress }, startDate, endDate);
        }

        [AbpAllowAnonymous]
        [HttpPost]
        public async Task<TimesheetChartDto> GetTimesheetWeeklyChartOfUserGroupInProject(InputGetTimesheetChartOfUserGroupDto input)
        {
            if (string.IsNullOrEmpty(input.ProjectCode))
            {
                throw new UserFriendlyException("project code is null or empty");
            }

            if (input.Emails.IsNullOrEmpty() || string.IsNullOrEmpty(input.Emails[0]))
            {
                throw new UserFriendlyException("emails is null or empty");
            }
            return await GetTimesheetWeeklyChart(input.ProjectCode, input.Emails, input.StartDate, input.EndDate);
        }

        private async Task<TimesheetChartDto> GetTimesheetWeeklyChart(string projectCode, List<string> emails, DateTime startDate, DateTime endDate)
        {
            var query = IQueryMyTimesheetContainEmails(projectCode, emails, startDate, endDate);

            var mapTimesheet = await query
                          .GroupBy(s => s.DateAt.AddDays(s.DateAt.DayOfWeek == DayOfWeek.Sunday ? -6 : 1 - (int)s.DateAt.DayOfWeek).Date)
                          .Select(s => new
                          {
                              FirstDayOfWeek = s.Key,
                              NormalWorkingHour = s.Where(x => x.TypeOfWork == TypeOfWork.NormalWorkingHours).Sum(x => x.WorkingTime) / 60,
                              OverTimeHour = s.Where(x => x.TypeOfWork == TypeOfWork.OverTime).Sum(p => p.WorkingTime) / 60,
                              OTNoChargeHour = s.Where(x => x.TypeOfWork == TypeOfWork.OverTime && !x.IsCharged).Sum(p => p.WorkingTime) / 60,
                          }).ToDictionaryAsync(s => s.FirstDayOfWeek);

            var listWeek = DateTimeUtils.GetListWeek(startDate, endDate);
            var result = new TimesheetChartDto()
            {
                Labels = listWeek.Select(s => DateTimeUtils.GetWeekName(s)).ToList(),
                NormalWoringHours = new List<float>(),
                OverTimeHours = new List<float>(),
                OTNoChargeHours = new List<float>(),
            };

            foreach (var monday in listWeek)
            {
                if (mapTimesheet.ContainsKey(monday))
                {
                    var ts = mapTimesheet[monday];
                    result.NormalWoringHours.Add(ts.NormalWorkingHour);
                    result.OverTimeHours.Add(ts.OverTimeHour);
                    result.OTNoChargeHours.Add(ts.OTNoChargeHour);
                }
                else
                {
                    result.NormalWoringHours.Add(0);
                    result.OverTimeHours.Add(0);
                    result.OTNoChargeHours.Add(0);
                }

            }

            return result;
        }


        private IQueryable<MyTimesheet> IQueryMyTimesheetContainEmails(string projectCode, List<string> emails, DateTime startDate, DateTime endDate)
        {
            var query = WorkScope.GetAll<MyTimesheet>()
                          .Include(s => s.ProjectTask)
                          .Include(s => s.ProjectTask.Project)
                          .Where(s => s.DateAt >= startDate.Date)
                          .Where(s => s.DateAt.Date <= endDate)
                          .Where(s => s.Status == TimesheetStatus.Approve);

            if (!emails.IsNullOrEmpty())
            {
                if (emails.Count == 1 & !string.IsNullOrEmpty(emails[0]))
                {
                    var emailAddress = emails[0].ToLower().Trim();
                    query = query.Where(s => s.User.EmailAddress.ToLower().Trim() == emailAddress);
                }
                else
                {
                    query = query.Where(s => emails.Contains(s.User.EmailAddress));
                }

            }

            if (!string.IsNullOrEmpty(projectCode))
            {
                query = query.Where(s => s.ProjectTask.Project.Code.ToLower().Trim() == projectCode.ToLower().Trim());
            }
            return query;
        }


        [AbpAllowAnonymous]
        [HttpPost]
        public async Task<EffortChartDto> GetEffortMonthlyChartOfUserGroupInProject(InputGetTimesheetChartOfUserGroupDto input)
        {
            if (string.IsNullOrEmpty(input.ProjectCode))
            {
                throw new UserFriendlyException("project code is null or empty");
            }

            var startDate = DateTimeUtils.FirstDayOfMonth(input.StartDate);
            var endDate = DateTimeUtils.LastDayOfMonth(input.EndDate);

            return await GetEffortMonthlyChart(input.ProjectCode, input.Emails, startDate, endDate);
        }


        private async Task<EffortChartDto> GetEffortMonthlyChart(string projectCode, List<string> emails, DateTime startDate, DateTime endDate)
        {
            var query = IQueryMyTimesheetNotContainEmails(projectCode, emails, startDate, endDate);
            return await GroupTSByMonthlyToChart(query, projectCode, startDate, endDate);
        }


        private IQueryable<MyTimesheet> IQueryMyTimesheetNotContainEmails(string projectCode, List<string> emails, DateTime startDate, DateTime endDate)
        {
            var query = WorkScope.GetAll<MyTimesheet>()
                          .Include(s => s.ProjectTask)
                          .Include(s => s.ProjectTask.Project)
                          .Where(s => s.DateAt >= startDate.Date)
                          .Where(s => s.DateAt.Date <= endDate)
                          .Where(s => s.Status == TimesheetStatus.Approve);

            if (!emails.IsNullOrEmpty())
            {
                if (emails.Count == 1 & !string.IsNullOrEmpty(emails[0]))
                {
                    var emailAddress = emails[0].ToLower().Trim();
                    query = query.Where(s => s.User.EmailAddress.ToLower().Trim() != emailAddress);
                }
                else
                {
                    query = query.Where(s => !emails.Contains(s.User.EmailAddress));
                }

            }

            if (!string.IsNullOrEmpty(projectCode))
            {
                query = query.Where(s => s.ProjectTask.Project.Code.ToLower().Trim() == projectCode.ToLower().Trim());
            }
            return query;
        }


        private async Task<EffortChartDto> GroupTSByMonthlyToChart(IQueryable<MyTimesheet> query, string projectCode, DateTime startDate, DateTime endDate)
        {
            var mapTimesheet = await query
                         .GroupBy(s => s.DateAt.AddDays(1 - s.DateAt.Day).Date)
                         .Select(s => new
                         {
                             FirstDayOfMonth = s.Key,
                             NormalWorkingMinute = s.Where(x => x.TypeOfWork == TypeOfWork.NormalWorkingHours).Sum(x => x.WorkingTime),
                         }).ToDictionaryAsync(s => s.FirstDayOfMonth);

            var listMonth = DateTimeUtils.GetListMonth(startDate, endDate);

            var listOT = await _overTimeHourAppService.GetListOverTimeForChart(startDate, endDate, projectCode, null);

            var result = new EffortChartDto()
            {
                Labels = listMonth.Select(s => DateTimeUtils.GetMonthName(s)).ToList(),
                NormalWorkingHours = new List<float>(),
                ManDays = new List<double>(),
                OTxCofficientHours = new List<double>()
            };

            foreach (var month in listMonth)
            {
                var otHour = CaculateOTHour(month, listOT);
                result.OTxCofficientHours.Add(otHour);

                var normalWorkingHour = mapTimesheet.ContainsKey(month) ? (float)mapTimesheet[month].NormalWorkingMinute / 60 : 0f;
                result.NormalWorkingHours.Add(normalWorkingHour);

                var manDay = (float)(otHour + normalWorkingHour) / 8;
                result.ManDays.Add(manDay);
            }

            return result;
        }


        private IQueryable<MyTimesheet> IQueryMyTimesheetOfficial(string projectCode, DateTime startDate, DateTime endDate)
        {
            var query = WorkScope.GetAll<MyTimesheet>()
                .Select(s => new
                {
                    TS = s,
                    ProjectCode = s.ProjectTask.Project.Code
                }).Where(s => s.TS.DateAt >= startDate.Date)
                .Where(s => s.TS.DateAt.Date <= endDate)
                .Where(s => s.TS.Status == TimesheetStatus.Approve)
                .Where(s => s.ProjectCode == projectCode)
                .Where(s => !s.TS.IsTemp)
                .Select(s => s.TS);

            return query;
        }

        [AbpAllowAnonymous]
        [HttpPost]
        public async Task<EffortChartDto> GetEffortMonthlyChartProject(InputGetEffortMonthlyChartDto input)
        {
            if (string.IsNullOrEmpty(input.ProjectCode))
            {
                throw new UserFriendlyException("project code is null or empty");
            }

            var startDate = DateTimeUtils.FirstDayOfMonth(input.StartDate);
            var endDate = DateTimeUtils.LastDayOfMonth(input.EndDate);

            var query = IQueryMyTimesheetOfficial(input.ProjectCode, startDate, endDate);
            return await GroupTSByMonthlyToChart(query, input.ProjectCode, startDate, endDate);

        }

        private double CaculateOTHour(DateTime month, List<GetOverTimeHourDto> listOT)
        {
            return listOT.Sum(s => s.ListOverTimeHour.Where(x => x.DateAt.Year == month.Year && x.DateAt.Month == month.Month)
            .Sum(x => x.OTHour));
        }


        [AbpAllowAnonymous]
        [HttpPost]
        public async Task<TimesheetTaxDto> GetTimesheetDetailForTax(InputTimesheetTaxDto input)
        {
            var timesheets = await WorkScope.All<MyTimesheet>()
                .Where(s => s.DateAt.Year == input.Year)
                .Where(s => s.DateAt.Month == input.Month)
                .Where(s => input.ProjectCodes.Contains(s.ProjectTask.Project.Code))
                .Where(s => s.Status != TimesheetStatus.Reject)
                .Select(s => new TimesheetDto
                {
                    DateAt = s.DateAt,
                    ActualEmailAddress = s.User.EmailAddress,
                    Note = s.Note,
                    ProjectCode = s.ProjectTask.Project.Code,
                    ProjectName = s.ProjectTask.Project.Name,
                    TaskName = s.ProjectTask.Task.Name,
                    ActualWorkingMinute = s.WorkingTime,
                    ShadowEmailAddress = s.ProjectTargetUser != null ? s.ProjectTargetUser.User.EmailAddress : null,
                    ShadowWorkingMinute = s.TargetUserWorkingTime,
                    WorkType = s.TypeOfWork
                }).ToListAsync();

            var settingDayOffs = await WorkScope.All<DayOffSetting>()
                .Where(s => s.DayOff.Year == input.Year)
                .Where(s => s.DayOff.Month == input.Month)
                .Select(s => s.DayOff.Date)
                .ToListAsync();

            var workingDates = DateTimeUtils.GetListWorkingDate(settingDayOffs, input.Year, input.Month);

            return new TimesheetTaxDto
            {
                ListTimesheet = timesheets,
                ListWorkingDay = workingDates
            };

        }

        [AbpAllowAnonymous]
        [HttpGet]
        public async Task<List<Object>> GetDataForCheckPoint(DateTime startDate, DateTime endDate, Usertype? userType)
        {
            var querymts = WorkScope.GetAll<MyTimesheet>()
              .Where(s => s.DateAt >= startDate && s.DateAt.Date <= endDate)
              .Where(s => s.Status >= TimesheetStatus.Pending)
              .Where(s => s.User.IsActive && !s.User.IsStopWork)
              .Where(s => s.User.Level >= UserLevel.Intern_2)
              .Where(s => !userType.HasValue || s.User.Type == userType.Value)
              .Where(s => !s.ProjectTask.Project.isAllUserBelongTo);


            var userIds = querymts
                .Select(s => s.UserId)
                .Distinct()
                .ToList();


            var qmts = querymts
                .Select(s => new
                {
                    s.UserId,
                    s.Id,
                    s.WorkingTime,
                    s.ProjectTask.ProjectId
                });


            var qprojectPms = from s in (WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => new { s.ProjectId, PMId = s.UserId }))
                              group s by s.ProjectId into g
                              select new { ProjectId = g.Key, PmId = g.FirstOrDefault().PMId };

            var mapUserPmId = await (from s in (from mts in qmts
                                                join pu in qprojectPms on mts.ProjectId equals pu.ProjectId
                                                select new { mts.UserId, mts.WorkingTime, pu.PmId })
                                     group s by s.UserId into g
                                     select new
                                     {
                                         UserId = g.Key,
                                         PM = g.GroupBy(s => s.PmId, (key, lst) => new { PmId = key, WorkingTime = lst.Sum(s => s.WorkingTime) })
                                         .OrderByDescending(s => s.WorkingTime)
                                         .FirstOrDefault()
                                     }).ToDictionaryAsync(s => s.UserId, s => s.PM.PmId);

            var mapUserInfo = WorkScope.GetAll<User>()
                .Where(s => s.IsActive)
                .Select(s => new { s.Id, s.FullName, s.EmailAddress, s.Branch, s.Type, s.Level })
                .ToDictionary(s => s.Id);

            var resultList = new List<Object>();
            foreach (var userId in userIds)
            {
                if (mapUserPmId.ContainsKey(userId))
                {
                    var reviewerId = mapUserPmId[userId];
                    var reviewDetail = new
                    {
                        UserId = userId,
                        FullName = mapUserInfo[userId].FullName,
                        EmailAddress = mapUserInfo[userId].EmailAddress,
                        Branch = mapUserInfo[userId].Branch,
                        BranchName = mapUserInfo[userId].Branch.ToString(),
                        Type = mapUserInfo[userId].Type,
                        TypeName = mapUserInfo[userId].Type.ToString(),
                        //Level = mapUserInfo[userId].Level,
                        LevelName = mapUserInfo[userId].Level.ToString(),

                        ReviewerId = reviewerId,
                        ReviewerName = mapUserInfo[reviewerId].FullName,
                        ReviewerEmail = mapUserInfo[reviewerId].EmailAddress,
                    };

                    resultList.Add(reviewDetail);

                }


            }

            return resultList;
        }
        [HttpGet]
        public List<UserInfo> GetAllUsers()
        {
            return WorkScope.GetAll<User>().Select(x => new UserInfo()
            {
                UserType = x.Type,
                FullName = x.FullName,
                BranchName = x.Branch.Name,
                EmailAddress = x.EmailAddress,
                AvatarPath = x.AvatarPath
            }).ToList();
        }
        [HttpGet]
        public List<PMsOfUser> GetPMsOfUser(string email)
        {
            var qPms = WorkScope.GetAll<ProjectUser>()
                    .Where(p => p.Type == ProjectUserType.PM)
                    .Select(p => new
                    {
                        ProjectId = p.ProjectId,
                        UserType = p.User.Type,
                        FullName = p.User.FullName,
                        BranchName = p.User.Branch.Name,
                        EmailAddress = p.User.EmailAddress,
                        AvatarPath = p.User.AvatarPath
                    });
            return WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Project.Status == ProjectStatus.Active)
                .Where(s => s.Type != ProjectUserType.DeActive)
                .Where(x => x.User.EmailAddress == email)
                .Select(x => new PMsOfUser()
                {
                    ProjectName = x.Project.Name,
                    ProjectCode = x.Project.Code,
                    PMs = qPms
                    .Where(p => p.ProjectId == x.ProjectId)
                    .Select(p => new UserInfo
                    {
                        UserType = p.UserType,
                        FullName = p.FullName,
                        BranchName = p.BranchName,
                        EmailAddress = p.EmailAddress,
                        AvatarPath = p.AvatarPath
                    }).ToList()
                }).ToList();
        }

        [AbpAllowAnonymous]
        [HttpGet]
        public List<TimesheetAndCheckInOutAllUserDto> GetTimesheetAndCheckInOutAllUser(DateTime startDate, DateTime endDate)
        {
            var dicTimekeepings = WorkScope.GetAll<Timekeeping>()
                    .Where(x => x.UserId.HasValue && x.DateAt != null)
                    .Where(x => x.DateAt >= startDate.Date && x.DateAt.Date <= endDate)
                    .Select(x => new
                    {
                        x.UserId,
                        DateAt = x.DateAt.Date,
                        x.CheckIn,
                        x.CheckOut
                    })
                    .AsEnumerable()
                    .GroupBy(s => s.UserId)
                    .ToDictionary(s => s.Key, s => s
                                                .GroupBy(x => x.DateAt)
                                                .ToDictionary(x => x.Key, x => x.Select(y => new
                                                {
                                                    y.CheckIn,
                                                    y.CheckOut
                                                }).FirstOrDefault()));

            var dicTimesheets = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.DateAt >= startDate.Date && s.DateAt.Date <= endDate)
                .Where(s => s.Status == TimesheetStatus.Approve || s.Status == TimesheetStatus.Pending)
                .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .Select(s => new
                {
                    s.DateAt,
                    s.UserId,
                    s.WorkingTime,
                    s.User.EmailAddress,
                    s.User.FullName,
                    BranchName = s.User.Branch.Name,
                    s.User.Type
                })
                .AsEnumerable()
                .GroupBy(s => new { s.UserId, s.EmailAddress, s.FullName, s.BranchName, s.Type })
                .ToDictionary(s => s.Key, s => s
                                            .GroupBy(x => x.DateAt)
                                            .ToDictionary(x => x.Key, x => x.Sum(y => y.WorkingTime)));

            var result = from u in dicTimesheets
                         select new TimesheetAndCheckInOutAllUserDto()
                         {
                             EmailAddress = u.Key.EmailAddress,
                             FullName = u.Key.FullName,
                             UserType = u.Key.Type,
                             Branch = u.Key.BranchName,
                             ListDate = u.Value.Select(s => new TimesheetAndCheckInOutDto()
                             {
                                 DateAt = s.Key.Date,
                                 CheckIn = dicTimekeepings.ContainsKey(u.Key.UserId) ?
                                                 dicTimekeepings[u.Key.UserId].ContainsKey(s.Key.Date) ? dicTimekeepings[u.Key.UserId][s.Key.Date].CheckIn : ""
                                                 : "",
                                 CheckOut = dicTimekeepings.ContainsKey(u.Key.UserId) ? dicTimekeepings[u.Key.UserId].ContainsKey(s.Key.Date) ?
                                                 dicTimekeepings[u.Key.UserId][s.Key.Date].CheckOut : ""
                                                 : "",
                                 TimeSheetMinute = s.Value
                             })
                             .OrderBy(s => s.DateAt)
                             .ToList()
                         };

            return result.ToList();
        }


        [HttpGet]
        public GetResultConnectDto CheckConnect()
        {
            var secretCode = SettingManager.GetSettingValue(AppSettingNames.SecurityCode);
            var header = _httpContextAccessor.HttpContext.Request.Headers;

            var securityCodeHeader = header["X-Secret-Key"];
            var result = new GetResultConnectDto();
            if (!IsCheckSecurityCodeCorrectForProject())
            {
                result.IsConnected = false;
                result.Message = $"SecretCode does not match: " + securityCodeHeader + " != ***" + secretCode.Substring(secretCode.Length - 3);
                return result;
            }
            result.IsConnected = true;
            result.Message = "Connected";
            return result;
        }

        protected bool IsCheckSecurityCodeCorrectForProject()
        {
            var secretCode = SettingManager.GetSettingValue(AppSettingNames.SecurityCode);
            var header = _httpContextAccessor.HttpContext.Request.Headers;

            var securityCodeHeader = header["X-Secret-Key"];
            if (string.IsNullOrEmpty(securityCodeHeader))
            {
                securityCodeHeader = header["securityCode"];
            }

            return secretCode == securityCodeHeader;
        }
        [HttpGet]
        [AbpAllowAnonymous]
        public async Task<List<GetWorkingTimeDto>> GetCurrentWorkingTimeAllUserWorking()
        {
            return await WorkScope.GetAll<User>()
                .Where(s => s.IsActive)
                .Where(s => !s.IsStopWork)
                .Select(user => new GetWorkingTimeDto
                {
                    UserEmail = user.EmailAddress,
                    AfternoonEndTime = user.AfternoonEndAt,
                    AfternoonStartTime = user.AfternoonStartAt,
                    MorningEndTime = user.MorningEndAt,
                    MorningStartTime = user.MorningStartAt,
                })
                .ToListAsync();
        }
    }
}
