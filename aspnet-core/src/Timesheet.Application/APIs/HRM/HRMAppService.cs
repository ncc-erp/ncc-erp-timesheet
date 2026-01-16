using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.RequestDays.Dto;
using Timesheet.APIs.DayOffs.Dto;
using Timesheet.APIs.NormalWorkingHours.Dto;
using Timesheet.APIs.OverTimeHours.Dto;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;
using Microsoft.AspNetCore.Http;
using Ncc.Configuration;
using Abp.Configuration;
using Ncc.IoC;
using Ncc.Entities;
using Microsoft.AspNetCore.Identity;
using Ncc.Authorization.Roles;
using Timesheet.APIs.Timesheets.Projects.Dto;
using Timesheet.APIs.HRM.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Ncc.Users.Dto;
using Timesheet.Uitls;
using Timesheet.APIs.Timekeepings.Dto;
using Abp.Collections.Extensions;
using Abp.Authorization;
using Timesheet.APIs.OverTimeHours;
using Branch = Timesheet.Entities.Branch;
using Timesheet.Users.Dto;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Timesheet.NCCAuthen;

namespace Timesheet.APIs.HRM
{

    public class HRMAppService : AppServiceBase
    {
        //private readonly UserManager _userManager;
        private readonly OverTimeHourAppService _overTimeHourAppService;
        private readonly IUserServices _userServices;
        public HRMAppService(OverTimeHourAppService overTimeHourAppService, IUserServices userServices, IWorkScope workScope): base(workScope)
        {
            _overTimeHourAppService = overTimeHourAppService;
            _userServices = userServices;

        }

        public List<GetNormalWorkingHRMDto> GetAllNormalWorking(int year, int month)
        {
            var dicUserIdToDicDateToWorkingTime = WorkScope.GetAll<MyTimesheet>()
                                .Select(s => new { s.UserId, DateAt = s.DateAt.Date, s.TypeOfWork, s.WorkingTime, s.Status })
                               .Where(ts => ts.Status == TimesheetStatus.Approve)
                               .Where(ts => ts.DateAt.Year == year && ts.DateAt.Month == month)
                               .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                               .AsEnumerable()
                               .GroupBy(ts => ts.UserId)
                               .ToDictionary(s => s.Key,
                                    s => s.GroupBy(ts => ts.DateAt).ToDictionary(x => x.Key, x => x.Sum(t => t.WorkingTime)));


            var dicOffDate = WorkScope.GetAll<AbsenceDayDetail>()
                .Select(s => new { s.Request.UserId, DateAt = s.DateAt.Date, s.Request.Type, s.DateType, s.Request.Status })
                                  .Where(s => s.DateAt.Year == year && s.DateAt.Month == month)
                                  .Where(s => s.Type == RequestType.Off)
                                  .Where(s => s.DateType != DayType.Custom)
                                  .Where(s => s.Status != RequestStatus.Rejected)
                                  .AsEnumerable()
                                  .GroupBy(s => s.UserId)
                                  .ToDictionary(s => s.Key,
                                    s => s.GroupBy(x => x.DateAt)
                                    .ToDictionary(x => x.Key, t => t.Select(y => y.DateType)
                                    .FirstOrDefault()));

            var workingDates = GetListStandardDay(year, month);

            var resultList = WorkScope.GetAll<User>()
                               .Select(u => new GetNormalWorkingHRMDto
                               {
                                   UserId = u.Id,
                                   NormalizedEmailAddress = u.NormalizedEmailAddress
                               }).ToList();

            resultList.ForEach(item =>
            {
                if (dicUserIdToDicDateToWorkingTime.ContainsKey(item.UserId))
                {
                    var dicDateToWorkingTime = dicUserIdToDicDateToWorkingTime[item.UserId];

                    item.ListWorkingHour = workingDates.Select(date =>
                    {

                        var isOpenTalk = date.DayOfWeek == DayOfWeek.Saturday && dicDateToWorkingTime.ContainsKey(date) && dicDateToWorkingTime[date] >= 120;
                        double workingHour = 0;
                        if (date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if (!dicDateToWorkingTime.ContainsKey(date))
                            {
                                workingHour = 0;
                            }
                            else if (dicOffDate.ContainsKey(item.UserId) && dicOffDate[item.UserId].ContainsKey(date))
                            {
                                var dateOffType = dicOffDate[item.UserId][date];
                                workingHour = dateOffType == DayType.Fullday ? 0 : 4;
                            }
                            else
                            {
                                workingHour = 8;
                            }
                        }
                        else
                        {
                            if (isOpenTalk)
                            {
                                workingHour = 4;
                            }
                        }
                        return new UserNormalWorkingDto
                        {
                            Day = date.Day,
                            DayName = date.DayOfWeek.ToString(),
                            IsOpenTalk = isOpenTalk,
                            WorkingHour = workingHour
                        };
                    }).Where(s => s.WorkingHour > 0)
                    .ToList();

                    if (item.ListWorkingHour == null)
                    {
                        item.ListWorkingHour = new List<UserNormalWorkingDto>();
                    }

                }
            });


            return resultList;
        }


        private List<DateTime> GetListStandardDay(int year, int month)
        {
            DateTime startDate = new DateTime(year, month, 1).Date;
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var offDays = WorkScope.GetAll<DayOffSetting>()
                                   .Where(s => s.DayOff.Year == year && s.DayOff.Month == month)
                                   .Select(s => s.DayOff.Date)
                                   .ToList();

            var WorkingDates = new List<DateTime>();
            var date = startDate;
            while (date <= endDate)
            {
                if (!offDays.Contains(date) && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    WorkingDates.Add(date.Date);
                }
                date = date.AddDays(1);
            }

            return WorkingDates;
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<GetOverTimeHourHRMDto>> GetAllOverTime(int year, int month)
        {
            return await _overTimeHourAppService.GetAllOverTimeForHRM(year, month);
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<GetAbsenceDayUserDto>> GetAllAbsenceday(int year, int month)
        {
            return await (WorkScope.GetAll<AbsenceDayDetail>()
                          .Where(s => s.DateAt.Year == year && s.DateAt.Month == month)
                          .Where(s => s.Request.Status != RequestStatus.Rejected)
                          .Where(s => s.Request.Type == RequestType.Off)
                          .Where(s => s.DateType != DayType.Custom)
                          .GroupBy(s => new { s.Request.UserId, s.Request.User.NormalizedEmailAddress })
                          .Select(t => new GetAbsenceDayUserDto
                          {
                              UserId = t.Key.UserId,
                              NormalizedEmailAddress = t.Key.NormalizedEmailAddress,
                              absenceDayDetails = t.Select(d => new AbsenceDayUserDetailDto
                              {
                                  Id = d.Id,
                                  DateAt = d.DateAt,
                                  //Hour = d.Hour,
                                  Status = d.Request.DayOffType.Status,
                                  DayType = d.DateType
                              }).OrderBy(d => d.DateAt)
                              .ToList()
                          })).ToListAsync();
        }
        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<DayOffDto>> GetAllDayOff(int year, int month)
        {
            return await WorkScope.GetAll<DayOffSetting>()
                     .Where(s => s.DayOff.Year == year && s.DayOff.Month == month)
                     .Select(s => new DayOffDto
                     {
                         Id = s.Id,
                         DayOff = s.DayOff,
                         Name = s.Name,
                         Coefficient = s.Coefficient,
                         Branch = s.Branch
                     })
                     .ToListAsync();
        }



        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<GetProjectPMDto>> GetAllProject()
        {
            var query = await WorkScope.GetAll<Project>()
                .Select(s => new GetProjectPMDto
                {
                    Id = s.Id,
                    Name = s.Name.Normalize(),
                    Code = s.Code
                }).ToListAsync();
            foreach (var q in query)
            {
                var pms = await WorkScope.GetAll<ProjectUser>().Where(s => s.ProjectId == q.Id && s.Type == ProjectUserType.PM).Select(s => s.User.FullName).ToListAsync();
                q.Pms = string.Join(",", pms.ToArray());
            }
            return query;
        }
        //TODO: fail due to setRole function in userService
        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        public async Task<long> CreateUser(CreateUserDto input)
        {
            input.RoleNames = new string[] { StaticRoleNames.Host.BasicUser };
            input.Password = CommonUtils.GenerateRandomPassword(12);
            var userTypeMapers = new Usertype[] { Usertype.Internship, Usertype.Collaborators, Usertype.Staff, Usertype.Staff };
            input.Type = userTypeMapers[(int)input.Type];
            //input.AvatarPath = input.AvatarPath == null ? "" : FileUtils.FullFilePath(input.AvatarPath);
            input.PhoneNumber = String.IsNullOrEmpty(input.PhoneNumber) || input.PhoneNumber.StartsWith("0") || input.PhoneNumber.StartsWith("84") ? input.PhoneNumber : "0" + input.PhoneNumber;
            var user = await _userServices.CreateUserAsync(input);
            return user.Id;
        }

        private async Task<long> GetBranchIdByCode(string code)
        {
            var branchId = await WorkScope.GetAll<Branch>().Where(s => s.Code == code).Select(s => s.Id).FirstOrDefaultAsync();
            if (branchId == default)
            {
                branchId = await WorkScope.GetAll<Branch>().Select(s => s.Id).FirstOrDefaultAsync();
            }
            return branchId;
        }
        //TODO: fail due to setRole function in userService
        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        public async Task<long> CreateUserNew(CreateUserDto input)
        {
            input.RoleNames = new string[] { StaticRoleNames.Host.BasicUser };
            input.Password = CommonUtils.GenerateRandomPassword(12);
            var userTypeMapers = new Usertype[] { Usertype.Internship, Usertype.Collaborators, Usertype.Staff, Usertype.Staff };
            input.Type = userTypeMapers[(int)input.Type];
            input.SalaryAt = input.StartDateAt;
            input.BranchId = await GetBranchIdByCode(input.BranchCode);
            input.PhoneNumber = String.IsNullOrEmpty(input.PhoneNumber) || input.PhoneNumber.StartsWith("0") || input.PhoneNumber.StartsWith("84") ? input.PhoneNumber : "0" + input.PhoneNumber;
            input.BeginLevel = input.Level;
            //input.AvatarPath = input.AvatarPath == null ? "" : FileUtils.FullFilePath(input.AvatarPath);
            var user = await _userServices.CreateUserAsync(input);
            return user.Id;
        }


        private async Task<Timesheet.Entities.Branch> GetBranchByCode(string code)
        {
            var branch = await WorkScope.GetAll<Timesheet.Entities.Branch>().Where(s => s.Code == code).FirstOrDefaultAsync();
            if (branch == default)
            {
                branch = await WorkScope.GetAll<Timesheet.Entities.Branch>().FirstOrDefaultAsync();
            }
            return branch;
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        public async System.Threading.Tasks.Task UpdateUserNew(CreateUserDto input)
        {

            var userTypeMapers = new Usertype[] { Usertype.Internship, Usertype.Collaborators, Usertype.Staff, Usertype.Staff };
            //input.Type = userTypeMapers[(int)input.Type];

            var user = await WorkScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim()).FirstOrDefaultAsync();

            user.Type = userTypeMapers[(int)input.Type];
            user.IsActive = input.IsActive;
            user.Sex = input.Sex;
            user.Name = input.Name;
            user.Surname = input.Surname;
            user.EmailAddress = input.EmailAddress;
            user.NormalizedEmailAddress = input.EmailAddress.ToUpper();
            user.Level = input.Level;
            user.BranchId = await GetBranchIdByCode(input.BranchCode);
            user.UserName = input.UserName?.Replace("@gmail.com", "").Replace("@ncc.asia", "");
            user.NormalizedUserName = input.UserName?.Replace("@gmail.com", "").Replace("@ncc.asia", "").ToUpper();
            if (input.IsActive)
            {
                user.EndDateAt = null;
            }
            else
            {
                user.EndDateAt = DateTimeUtils.GetNow();
            }
            if (!string.IsNullOrEmpty(input.AvatarPath))
            {
                user.AvatarPath = input.AvatarPath;
            }
            // if (input.BranchCode != null)
            // {
            //     //Call from HRM tool
            //     var branch = await GetBranchByCode(input.BranchCode);
            //     user.MorningStartAt = branch.MorningStartAt;
            //     user.MorningEndAt = branch.MorningEndAt;
            //     user.MorningWorking = branch.MorningWorking;
            //     user.AfternoonStartAt = branch.AfternoonStartAt;
            //     user.AfternoonEndAt = branch.AfternoonEndAt;
            //     user.AfternoonWorking = branch.AfternoonWorking;
            // }
            user.PhoneNumber = input.PhoneNumber;
            //await WorkScope.UpdateAsync(user);
            if (!input.IsActive)
            {
                var projectUsers = await WorkScope.GetAll<ProjectUser>()
                    .Where(s => s.Type != ProjectUserType.DeActive)
                    .Where(s => s.UserId == input.Id)
                    .ToListAsync();
                foreach (var item in projectUsers)
                {
                    item.Type = ProjectUserType.DeActive;
                    //await WorkScope.UpdateAsync(item);
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            //await _userServices.UpdateUserAsync(input);
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        public async System.Threading.Tasks.Task UpdateUser(CreateUserDto input)
        {
            var userTypeMapers = new Usertype[] { Usertype.Internship, Usertype.Collaborators, Usertype.Staff, Usertype.Staff };
            //input.Type = userTypeMapers[(int)input.Type];

            var user = await WorkScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim()).FirstOrDefaultAsync();

            user.Type = userTypeMapers[(int)input.Type];
            user.IsActive = input.IsActive;
            user.Sex = input.Sex;
            user.Name = input.Name;
            user.Surname = input.Surname;
            user.EmailAddress = input.EmailAddress;
            user.NormalizedEmailAddress = input.EmailAddress.ToUpper();
            user.Level = input.Level;
            if (input.IsActive)
            {
                user.EndDateAt = null;
            }
            else
            {
                user.EndDateAt = DateTimeUtils.GetNow();
            }

            //user.BranchOld = input.BranchId;
            if (Constants.ConstantUploadFile.Provider == Constants.ConstantUploadFile.AMAZONE_S3
                && !string.IsNullOrEmpty(input.AvatarPath))
            {
                user.AvatarPath = input.AvatarPath;
            }
            user.UserName = input.UserName?.Replace("@gmail.com", "").Replace("@ncc.asia", "");
            user.NormalizedUserName = input.UserName?.Replace("@gmail.com", "").Replace("@ncc.asia", "").ToUpper();
            //await WorkScope.UpdateAsync(user);
            if (!input.IsActive)
            {
                var projectUsers = await WorkScope.GetAll<ProjectUser>()
                    .Where(s => s.Type != ProjectUserType.DeActive)
                    .Where(s => s.UserId == input.Id)
                    .ToListAsync();
                foreach (var item in projectUsers)
                {
                    item.Type = ProjectUserType.DeActive;
                    //await WorkScope.UpdateAsync(item);
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            //await _userServices.UpdateUserAsync(input);
        }
        [HttpGet]
        public async Task<List<PunishmentUnlockTSDto>> GetAllPublishment(int year, int month, LockUnlockTimesheetType unlockType)
        {
            var query = WorkScope.GetAll<UserUnlockIms>()
                .Where(s => s.Type == unlockType)
                .Where(s => s.CreationTime.Year == year && s.CreationTime.Month == month);

            return await (query.Select(s => new
            {
                s.User.NormalizedEmailAddress,
                s.UserId,
                s.Amount,
                s.Times,
            }).GroupBy(s => new { s.UserId, s.NormalizedEmailAddress }).Select(s => new PunishmentUnlockTSDto
            {
                UserId = s.Key.UserId,
                NormalizedEmailAddress = s.Key.NormalizedEmailAddress,
                Amount = s.Sum(t => t.Amount),
                Times = s.Sum(t => t.Times),
            })).ToListAsync();
        }
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<GetTimekeepingByMonthDto>> GetTimekeepingByMonth(int year, int month)
        {

            return await (from t in WorkScope.GetAll<Timekeeping>()
                          where t.DateAt.Month == month && t.DateAt.Year == year
                          group t by new { t.UserId, t.User.NormalizedEmailAddress } into g
                          select new GetTimekeepingByMonthDto
                          {
                              UserId = g.Key.UserId,
                              NormalizedEmailAddress = g.Key.NormalizedEmailAddress,
                              NumbleOfCheckInLateOrNoCheckIn = g.Count(x => x.IsPunishedCheckIn)
                          }).ToListAsync();
        }
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<GetRequestDto>> GetRequestByTimeAndType(DateTime startDate, DateTime endDate, string name, RequestType? type)
        {
            var query = WorkScope.GetAll<AbsenceDayDetail>()
                  .Where(s => s.DateAt >= startDate)
                  .Where(s => s.DateAt <= endDate)
                  .Where(s => !type.HasValue || type.Value < 0 || s.Request.Type == type.Value)
                  .Where(s => string.IsNullOrWhiteSpace(name) || s.Request.User.UserName.Contains(name))
                  .Select(s => new GetRequestDto
                  {
                      Id = s.Request.Id,
                      UserId = s.Request.UserId,
                      Branch = s.Request.User.Branch.Code,
                      AvatarPath = !string.IsNullOrEmpty(s.Request.User.AvatarPath) ? s.Request.User.AvatarPath : "",
                      Sex = s.Request.User.Sex,
                      FullName = s.Request.User.FullName,
                      Name = s.Request.User.Name,                      
                      Type = s.Request.User.Type,
                      DateAt = s.DateAt,
                      DateType = s.DateType,
                      DayOffName = s.Request.DayOffType.Name,
                      Hour = s.Hour,
                      Reason = s.Request.Reason,
                      Status = s.Request.Status,
                      ShortName = s.Request.User.Name,
                      LeavedayType = s.Request.Type
                  });
            return await query.ToListAsync();
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<UserRegisterTimeDto>> GetUserRegisterTime(string branch, DateTime? date)
        {

            var timeline = !date.HasValue ? DateTimeUtils.GetNow().Date : date.Value.Date;
            var historyChangeWorking = await WorkScope.GetAll<HistoryWorkingTime>()
                                        .Where(x => x.Status == RequestStatus.Approved)
                                        .Where(x => x.ApplyDate <= timeline && (!x.LastModificationTime.HasValue || x.LastModificationTime.Value.Date <= timeline))
                                        .Select(x => new
                                        {
                                            userId = x.UserId,
                                            startTime = x.MorningStartTime,
                                            endTime = x.AfternoonEndTime,
                                            dateChange = !x.LastModificationTime.HasValue ? x.ApplyDate :
                                                        (x.ApplyDate < x.LastModificationTime.Value.Date ? x.LastModificationTime.Value : x.ApplyDate)
                                        }).OrderByDescending(x => x.userId).OrderByDescending(x => x.dateChange).ToListAsync();
            // nếu có date thì chỉ lấy các user thay đổi h làm vào ngày này, ko thì lấy tất cả của hiện tại
            if (date.HasValue)
            {
                historyChangeWorking = historyChangeWorking.Where(x => x.dateChange.Date == date.Value.Date).ToList();
            }

            long branchId = await GetBranchIdByCode(branch);

            return await (from u in WorkScope.GetAll<User>().Where(s => s.IsActive)
                          where String.IsNullOrEmpty(branch) || u.BranchId == branchId
                          where !date.HasValue || historyChangeWorking.Select(x => x.userId).Contains(u.Id)
                          select new UserRegisterTimeDto
                          {
                              UserId = u.Id,
                              EmailAddress = u.EmailAddress,
                              MorningStartAt = !historyChangeWorking.Any(x => x.userId == u.Id) ? u.MorningStartAt : historyChangeWorking.Where(x => x.userId == u.Id).First().startTime,
                              AfternoonEndAt = !historyChangeWorking.Any(x => x.userId == u.Id) ? u.AfternoonEndAt : historyChangeWorking.Where(x => x.userId == u.Id).First().endTime,
                              DateChangeWorkingTime = !historyChangeWorking.Any(x => x.userId == u.Id) ? u.StartDateAt :
                                                historyChangeWorking.Where(x => x.userId == u.Id).First().dateChange
                          }).ToListAsync();
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        public async Task<UpdateUserWhenOffDto> UpdateUserWhenOff(UpdateUserWhenOffDto input)
        {
            var currentUser = await WorkScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.Email.ToLower().Trim())
                .FirstOrDefaultAsync();
            if (currentUser == null)
            {
                throw new UserFriendlyException("Email of Timesheet is not the same as hrm");
            }

            currentUser.IsActive = input.IsActive;
            currentUser.IsStopWork = input.IsStopWork;
            await WorkScope.UpdateAsync(currentUser);
            return input;
        }




    }
}
