using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.DayOffs.Dto;
using Timesheet.APIs.HRM.Dto;
using Timesheet.APIs.HRMv2.Dto;
using Timesheet.APIs.NormalWorkingHours.Dto;
using Timesheet.APIs.OverTimeHours;
using Timesheet.APIs.OverTimeHours.Dto;
using Timesheet.Constants;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.NCCAuthen;
using Timesheet.Services.HRMv2;
using Timesheet.Services.HRMv2.Dto;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.HRMv2
{
    public class HRMv2AppService : AppServiceBase
    {
        private readonly OverTimeHourAppService _overTimeHourAppService;
        private readonly IUserServices _userServices;
        private readonly HRMv2Service _hrmv2Service;

        public HRMv2AppService(OverTimeHourAppService overTimeHourAppService,
            HRMv2Service hrmv2Service,
            IUserServices userServices, IWorkScope workScope) : base(workScope)
        {
            _overTimeHourAppService = overTimeHourAppService;
            _userServices = userServices;
            _hrmv2Service = hrmv2Service;
        }

        [HttpPost]
        [AbpAllowAnonymous]
        [NccAuthentication]
        public void UpdateAvatarFromHrm(UpdateAvatarDto input)
        {
            if (string.IsNullOrEmpty(input.AvatarPath))
            {
                Logger.Error($"user with {input.AvatarPath} is null or empty");
                return;
            }
            var user =  GetUserByEmail(input.EmailAddress);

            if (user == null)
            {
                Logger.Error($"not found user with email {input.EmailAddress}");
                return;
            }

            user.AvatarPath = input.AvatarPath;
             WorkScope.UpdateAsync(user);
        }


        private User GetUserByEmail(string emailAddress)
        {
            return  WorkScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == emailAddress.ToLower().Trim())
                .FirstOrDefault();
        }

        [HttpGet]
        private async Task<List<PunishmentEmployeeDto>> GetPunishmentUnlockTS(int year, int month, LockUnlockTimesheetType unlockType)
        {
            var query = WorkScope.GetAll<UserUnlockIms>()
                .Where(s => s.Type == unlockType)
                .Where(s => s.CreationTime.Year == year && s.CreationTime.Month == month);

            var data = await (query.Select(s => new
            {
                s.User.EmailAddress,
                s.Amount,
            }).GroupBy(s => new { s.EmailAddress }).Select(s => new PunishmentEmployeeDto
            {
                Email = s.Key.EmailAddress,
                Money = s.Sum(t => t.Amount)
            })).ToListAsync();
            return data;
        }

        public async Task<List<PunishmentEmployeeDto>> GetPunishmentBasicUserUnlockTS(int year, int month)
        {
            var data = await GetPunishmentUnlockTS(year, month, LockUnlockTimesheetType.MyTimesheet);
            return data;
        }

        public async Task<List<PunishmentEmployeeDto>> GetPunishmentPMUnlockTS(int year, int month)
        {
            var data = await GetPunishmentUnlockTS(year, month, LockUnlockTimesheetType.ApproveRejectTimesheet);
            return data;
        }

        public List<PunishmentEmployeeDto> GetPunishtmentCheckin(int year, int month)
        {
            return WorkScope.GetAll<Timekeeping>()
                .Where(s => s.DateAt.Year == year)
                .Where(s => s.DateAt.Month == month)
                //.Where(s => s.IsPunishedCheckIn)
                .Where(s => s.MoneyPunish > 0)
                .GroupBy(s => s.User.EmailAddress)
                .Select(g => new PunishmentEmployeeDto
                          {
                              Email = g.Key,
                              Money = g.Sum(x=> x.MoneyPunish)
                }).ToList();
        }


        [HttpPost]
        public List<GetEmployeeRequestDayDto> GetAllRequestDay(InputCollectDataForPayslipDto input)
        {

            var queryRequest = WorkScope.GetAll<AbsenceDayDetail>()
                .Include(x => x.Request)
                .ThenInclude(x => x.User)               
                .Select(s => new
                {
                    s.Request.User.NormalizedEmailAddress,
                    RequestType = s.Request.Type,
                    DateType = s.DateType,
                    DateAt = s.DateAt.Date,
                    s.Request.Status,
                    s.Request.DayOffTypeId,
                    LeaveDay = s.Request.DayOffType.Length
                })          
                .Where(s => s.Status != RequestStatus.Rejected)
                .Where(s => s.DateType != DayType.Custom);

            var results = queryRequest.Where(s => s.DateAt.Year == input.Year && s.DateAt.Month == input.Month)
                .Where(s => input.UpperEmails.Contains(s.NormalizedEmailAddress))
                .AsEnumerable()
                .GroupBy(s => s.NormalizedEmailAddress)
                .Select(s => new GetEmployeeRequestDayDto { 
                    NormalizedEmailAddress = s.Key,
                    OffDates = s.Where(x => x.RequestType == RequestType.Off)
                    .Select(y => new OffDateDto { 
                        DateAt = y.DateAt, 
                        DayValue = DayTypeToValue(y.DateType),
                        DayOffTypeId = y.DayOffTypeId,
                        LeaveDay = y.LeaveDay
                    }).ToList(),

                    WorkAtHomeOnlyDates = s.GroupBy(x => x.DateAt)
                        .Select(x => new { DateAt = x.Key, ListRequest = x })                    
                        .Where(x => x.ListRequest.Any(y => y.DateType == DayType.Fullday && y.RequestType == RequestType.Remote)
                                    || x.ListRequest.Any(y => y.DateType == DayType.Fullday && y.RequestType == RequestType.Off)
                                    || (x.ListRequest.Any(y => y.RequestType == RequestType.Off) && x.ListRequest.Any(y => y.RequestType == RequestType.Remote)))
                        .Select(x => x.DateAt).Distinct().ToHashSet()

                }).ToList();


            var emails = results.Where(s => s.OffDates.Any(x => x.LeaveDay > 0)).Select(s => s.NormalizedEmailAddress);
            if (emails == null || !emails.Any())
            {
                return results;
            }

            var dateLastMonth = new DateTime(input.Year, input.Month, 10).AddMonths(-1);
            var dicEmailToListDateOffTypeId = queryRequest.Where(s => s.DateAt.Year == dateLastMonth.Year && s.DateAt.Month == dateLastMonth.Month)
                .Where(s => emails.Contains(s.NormalizedEmailAddress))
                .Where(s => s.LeaveDay > 0)
                .AsEnumerable()
                .GroupBy(s => s.NormalizedEmailAddress)
                .ToDictionary(s => s.Key, s => s.Select( y => new OffDateDto
                {
                    DateAt = y.DateAt,                    
                    DayOffTypeId = y.DayOffTypeId,
                    LeaveDay = y.LeaveDay
                }).ToList());

            results.ForEach(item => {
                item.OffDateLastMonth = dicEmailToListDateOffTypeId.ContainsKey(item.NormalizedEmailAddress) ? dicEmailToListDateOffTypeId[item.NormalizedEmailAddress] : default;                
            });

            return results;
        }

        [HttpPost]
        public List<ChamCongInfoDto> GetChamCongInfo(InputCollectDataForPayslipDto input)
        {
            var strOpentalkTaskId = SettingManager.GetSettingValueForApplication(AppSettingNames.OpenTalkTaskId);
            long opentalkTaskId = long.Parse(strOpentalkTaskId);

            var emails = input.UpperEmails == null ? new List<string>() : input.UpperEmails;

            var resultList = WorkScope.GetAll<MyTimesheet>()
                .Select(s => new { s.Status, DateAt = s.DateAt.Date, s.WorkingTime, s.TypeOfWork, s.User.NormalizedEmailAddress, s.ProjectTaskId, s.User.StartDateAt, s.User.EndDateAt })
                .Where(ts => ts.Status == TimesheetStatus.Approve)
                .Where(s => emails.Contains(s.NormalizedEmailAddress))
                .Where(ts => ts.DateAt.Year == input.Year && ts.DateAt.Month == input.Month)
                .Where(ts => ts.DateAt.DayOfWeek != DayOfWeek.Sunday)
                .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .AsEnumerable()
                .GroupBy(x => new { x.NormalizedEmailAddress, x.StartDateAt, x.EndDateAt})
                .Select(s => new
                {
                    Email = s.Key.NormalizedEmailAddress,
                    ListDate = s.GroupBy(x => x.DateAt).Select(y => new
                    {
                        Date = y.Key,
                        ProjectTaskId = y.Select(z => z.ProjectTaskId),
                        WorkingMinute = y.Sum(z => z.WorkingTime)
                    }).ToList(),
                    StartWorkingDate = s.Key.StartDateAt,
                    StopWorkingDate = s.Key.EndDateAt,
                })
                .Select(x => new ChamCongInfoDto
                {
                    NormalizeEmailAddress = x.Email,
                    
                    OpenTalkDates = x.ListDate.Where(s => s.Date.DayOfWeek == DayOfWeek.Saturday && s.WorkingMinute >= 120)
                    .Select(s => s.Date).ToList(),
                    
                    NormalWorkingDates = x.ListDate.Where(s => s.Date.DayOfWeek != DayOfWeek.Saturday).Where(s => s.ProjectTaskId.Any(p => p != opentalkTaskId)).Where(s => s.WorkingMinute > 0)
                    .Select(s => s.Date).ToList(),

                    StartWorkingDate = x.StartWorkingDate,
                    StopWorkingDate = x.StopWorkingDate,
                }).ToList();

            List<DateTime> listSaturdayDate = DateTimeUtils.GetListSaturdayDate(input.Year, input.Month);
            var dicUserEmailToOpentalkCount = GetDicUserIdToOpentalkCount(input.Year, input.Month);
            ProcessOpentalkByNewWay(resultList, dicUserEmailToOpentalkCount, listSaturdayDate);

            return resultList;
        }

        /// <summary>
        ///  Get Dictionary key : userId, value : opentalk count khác thứ bảy
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public Dictionary<string, int> GetDicUserIdToOpentalkCount(int year, int month)
        {
            var strOpentalkTaskId = SettingManager.GetSettingValueForApplication(AppSettingNames.OpenTalkTaskId);
            long opentalkTaskId = long.Parse(strOpentalkTaskId);

            var resultDic = WorkScope.GetAll<MyTimesheet>()
                .Select(s => new { s.Status, DateAt = s.DateAt.Date, s.WorkingTime, s.TypeOfWork, s.User.NormalizedEmailAddress, s.ProjectTaskId })
                .Where(s => s.ProjectTaskId == opentalkTaskId)
                .Where(s => s.Status == TimesheetStatus.Approve)
                .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .Where(s => s.DateAt.DayOfWeek != DayOfWeek.Saturday && s.DateAt.Month == month && s.DateAt.Year == year)
                .Where(s => s.WorkingTime >= 60)
                .Select(s => new
                {
                    s.NormalizedEmailAddress,
                    DateAt = s.DateAt.Date,
                })
                .ToList()
                .GroupBy(s => s.NormalizedEmailAddress)
                .ToDictionary(s => s.Key, s => s.Select(x => x.DateAt).Distinct().Count());

            return resultDic;
        }

        public void ProcessOpentalkOneUserByNewWay(ChamCongInfoDto dto, int fillOpentalkCount, List<DateTime> listSaturdayDate)
        {
            if (fillOpentalkCount <= 0) return;

            if (dto == null) return;

            var fillOpentalkDates = new List<DateTime>();

            fillOpentalkDates = listSaturdayDate
                .WhereIf(dto.StartWorkingDate != null, 
                s => s >= dto.StartWorkingDate.Value.Date)
                .WhereIf(dto.StopWorkingDate != null,
                s => s <= dto.StopWorkingDate.Value.Date)
                .Except(dto.OpenTalkDates)
                .Take(fillOpentalkCount)
                .ToList();

            dto.OpenTalkDates.AddRange(fillOpentalkDates);
        }

        public void ProcessOpentalkByNewWay(List<ChamCongInfoDto> listChamCongInfoDto, Dictionary<string, int> dicUserIdToOpentalkCount, List<DateTime> listSaturdayDate)
        {
            foreach (var dto in listChamCongInfoDto)
            {
                if (!dicUserIdToOpentalkCount.ContainsKey(dto.NormalizeEmailAddress))
                {
                    continue;
                }

                int fillOpentalkCount = dicUserIdToOpentalkCount[dto.NormalizeEmailAddress];
                ProcessOpentalkOneUserByNewWay(dto, fillOpentalkCount, listSaturdayDate);
            }
        }

        private double DayTypeToValue(DayType dayType)
        {
            return dayType == DayType.Fullday ? 1 : 0.5;
        }

        [HttpGet]
        public HashSet<DateTime> GetSettingOffDates(int year, int month)
        {
            var result = WorkScope.GetAll<DayOffSetting>()
                     .Where(s => s.DayOff.Year == year && s.DayOff.Month == month)
                     .Select(s => s.DayOff.Date).AsEnumerable();
                    
            return result.Distinct().ToHashSet();
        }

        [HttpPost]
        public async Task<List<GetOverTimeHourHRMv2Dto>> GetOTTimesheets(InputCollectDataForPayslipDto input)
        {
            return await _overTimeHourAppService.GetAllOverTimeForHRMv2(input);
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        public async Task<long> CreateUser(CreateUpdateByHRMV2Dto input)
        {
            var user = await _userServices.CreateUserFromHrmv2Async(input);
            return user.Id;
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        public async System.Threading.Tasks.Task UpdateUser(CreateUpdateByHRMV2Dto input)
        {
            await _userServices.UpdateUserFromHrmV2Async(input);
        }

        //TODO: test ComplainPayslipMail funtion
        [HttpPost]
        public async Task<string> ComplainPayslipMail(UpdatePayslipComplainToHrmDto input)
        {
            var email = WorkScope.GetAll<User>()
                .Where(x => x.Id == AbpSession.UserId)
                .Select(x => x.EmailAddress)
                .FirstOrDefault();

            var dto = new InputComplainPayslipMail
            {
                ComplainNote = input.ComplainNote,
                PayslipId = input.PayslipId,
                Email = email
            };

            return await _hrmv2Service.ComplainPayslipMail(dto);
        }
        //TODO: test ConfirmPayslipMail funtion
        [HttpGet]
        public async Task<string> ConfirmPayslipMail(long payslipId)
        {
            var email = WorkScope.GetAll<User>()
               .Where(x => x.Id == AbpSession.UserId)
               .Select(x => x.EmailAddress)
               .FirstOrDefault();

            var dto = new InputConfirmPayslipMail
            {
                Email = email,
                PayslipId = payslipId
            };

            return await _hrmv2Service.ConfirmPayslipMail(dto);
        }
        //TODO: test GetUserInfoByEmail funtion
        [HttpGet]
        public async Task<GetUserInfoByEmailDto> GetUserInfoByEmail(string email)
        {
            return await _hrmv2Service.GetUserInfoByEmail(email);
        }
        //TODO: test GetAllBanks funtion
        [HttpGet]
        public async Task<List<ItemInfoDto>> GetAllBanks()
        {
            return await _hrmv2Service.GetAllBanks();
        }
        //TODO: test GetInfoToUpdate funtion
        [HttpGet]
        public async Task<GetInfoToUPDateProfile>GetInfoToUpdate(string email)
        {
            return await _hrmv2Service.GetInfoToUpdate(email);
        }
        //TODO: test UpdateUserInfo funtion
        [HttpPost]
        public async Task<ResultUpdateInfo> UpdateUserInfo(UpdateUserInfoDto input)
        {
            return await _hrmv2Service.UpdateUserInfo(input);
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [NccAuthentication]
        private async Task<UpdateUserStatusDto> UpdateTimesheetUserStatus(UpdateUserStatusDto input)
        {
            var userToUpdate = await WorkScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim())
                .FirstOrDefaultAsync();

            if (userToUpdate == null)
            {
                throw new UserFriendlyException("Can't found user with the same email with HRM Tool");
            }

            userToUpdate.IsActive = input.IsActive;
            userToUpdate.IsStopWork = input.IsStopWork;
            userToUpdate.EndDateAt = input.StopWorkingTime;
            
            await WorkScope.UpdateAsync(userToUpdate);

            return input;
             

        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<UpdateUserStatusFromHRMDto> ConfirmUserQuit(UpdateUserStatusFromHRMDto input)
        {
            var inputToUpdate = new UpdateUserStatusDto()
            {
                IsActive = false,
                IsStopWork = true,
                EmailAddress = input.EmailAddress,
                StopWorkingTime = input.DateAt
            };
            await UpdateTimesheetUserStatus(inputToUpdate);
            return input;
        }
        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<UpdateUserStatusFromHRMDto> ConfirmUserPause(UpdateUserStatusFromHRMDto input)
        {
            var inputToUpdate = new UpdateUserStatusDto()
            {
                IsActive = true,
                IsStopWork = true,
                EmailAddress = input.EmailAddress,
                StopWorkingTime = input.DateAt
            };
            await UpdateTimesheetUserStatus(inputToUpdate);
            return input;
        }
        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<UpdateUserStatusFromHRMDto> ConfirmUserMaternityLeave(UpdateUserStatusFromHRMDto input)
        {
            var inputToUpdate = new UpdateUserStatusDto()
            {
                IsActive = true,
                IsStopWork = true,
                EmailAddress = input.EmailAddress,
                StopWorkingTime = input.DateAt
            };
            await UpdateTimesheetUserStatus(inputToUpdate);
            return input;
        }
        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<UpdateUserStatusFromHRMDto> ConfirmUserBackToWork(UpdateUserStatusFromHRMDto input)
        {
            var inputToUpdate = new UpdateUserStatusDto()
            {
                IsActive = true,
                IsStopWork = false,
                EmailAddress = input.EmailAddress,
                StopWorkingTime = null
            };
            await UpdateTimesheetUserStatus(inputToUpdate);
            return input;
        }
    }
}
