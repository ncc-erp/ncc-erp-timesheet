using Abp.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using Ncc.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Entities;
using Timesheet.Uitls;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static Ncc.Entities.Enum.StatusEnum;
using Abp.UI;
using Timesheet.APIs.Info.Dto;
using Ncc.Authorization.Users;
using System.Globalization;
using Ncc.Entities;
using Timesheet.DomainServices;
using Branch = Timesheet.Entities.Branch;
using Ncc.IoC;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Timesheet.APIs.Info
{
    public class InfoAppService : AppServiceBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICommonServices _commonServices;
        private readonly IUserServices _userService;
        public InfoAppService(IHttpContextAccessor httpContextAccessor, ICommonServices commonServices, IUserServices userService, IWorkScope workScope) : base(workScope)
        {
            _httpContextAccessor = httpContextAccessor;
            _commonServices = commonServices;
            _userService = userService;
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<UserLockedTimesheetDto> GetAllTimesheetLocked1(string emailAddress)
        {
            var userId = await _userService.GetUserIdByEmail(emailAddress);
            if (!userId.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }
            return await GetAllTimesheetLocked(userId.Value);
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<UserLockedTimesheetDto> GetAllTimesheetLocked(long userId)
        {
            //if (!checkSecurityCode())
            //{
            //    throw new UserFriendlyException("Timesheet server can't connect");
            //}
            var isAlreadyUnlockToLog = IsAlreadyUnlockToLog(userId);
            var isAlreadyUnlockToApprove = IsAlreadyUnlockToApprove(userId);
            var isPM = IsPM(userId);
            var firstDateCanLogIfUnlock = await getStartDateToCheckUnlockTS();
            float fMoneyPMUnlockTimeSheet = getMoneyPMUnlockTimeSheet();          
            if (isAlreadyUnlockToLog && isAlreadyUnlockToApprove)
                {
                    return new UserLockedTimesheetDto
                    {
                        IsUnlockLog = isAlreadyUnlockToLog,
                        IsUnlockApprove = isAlreadyUnlockToApprove,
                        IsPM = isPM,
                        FirstDateCanLogIfUnlock = firstDateCanLogIfUnlock.ToString("dd/MM/yyyy"),
                    };
                }
                List<EmployeeLockedWeekDto> listLockedDate = null;
                int timesLockedEm = 0, amount = 0, lockedPM = 0;
                float amountPM = 0;
                if (!isAlreadyUnlockToLog)
                {
                    listLockedDate = await getMyTimesheetLockedAsync(userId);
                    timesLockedEm = listLockedDate == null ? 0 : listLockedDate.Count();
                    amount = timesLockedEm >= 4 ? 100000 : timesLockedEm * 20000;
                }
                if (!isAlreadyUnlockToApprove)
                {


                    lockedPM = await getTimesheetLockedOfPMAsync(userId);
                    amountPM = fMoneyPMUnlockTimeSheet * lockedPM;
                }

                return new UserLockedTimesheetDto
                {
                    LockedEmployee = listLockedDate,
                    LockedPM = lockedPM,
                    Amount = amount,
                    AmountPM = amountPM,
                    IsUnlockLog = isAlreadyUnlockToLog,
                    IsUnlockApprove = isAlreadyUnlockToApprove,
                    IsPM = isPM,
                    FirstDateCanLogIfUnlock = firstDateCanLogIfUnlock.ToString("dd/MM/yyyy"),
                };
                }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async System.Threading.Tasks.Task UnlockSaturday1(string emailAddress)
        {
            var userId = await _userService.GetUserIdByEmail(emailAddress);
            if (!userId.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }
            await UnlockSaturday(new UnlockEmployeeInputDto { UserId = userId.Value });
        }

        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async System.Threading.Tasks.Task UnlockSaturday(UnlockEmployeeInputDto input)
        {
            if (!checkSecurityCode())
            {
                throw new UserFriendlyException("UnlockSaturday() Wrong security code");
            }
            var listDate = await getMyTimesheetLockedAsync(input.UserId);
            if (listDate != null && listDate.Count() > 0)
            {
                throw new UserFriendlyException("You can't unlock your timesheet for Saturday");
            }

            var fund = await WorkScope.GetAll<Fund>().Where(s => s.Status == FundStatus.Proceeds).FirstOrDefaultAsync();
            if (fund == null)
            {
                await WorkScope.InsertAsync<Fund>(new Fund
                {
                    Amount = 20000,
                    Status = FundStatus.Proceeds
                });
            }
            else
            {
                fund.Amount += 20000;
                await WorkScope.UpdateAsync(fund);
            }
            await WorkScope.InsertAsync<UserUnlockIms>(new UserUnlockIms
            {
                UserId = input.UserId,
                Times = 1,
                IsPayment = false,
                Type = LockUnlockTimesheetType.MyTimesheet,
                Amount = 20000
            });
            //Add unlock user
            await WorkScope.InsertAsync<UnlockTimesheet>(new UnlockTimesheet
            {
                UserId = input.UserId,
                Type = LockUnlockTimesheetType.MyTimesheet
            }); 
        }

        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<ListUserUnlockTSDto> TopUserUnlock()
        {
            //if (!checkSecurityCode())
            //{
            //    throw new UserFriendlyException("Timesheet server can't connect");
            //}
            var listUnlock = await WorkScope.GetAll<UserUnlockIms>()
                .Where(s => s.User.IsActive)
                .Select(s => new
            {
                s.UserId,
                s.User.Surname,
                s.User.Name,
                s.Amount
            }).GroupBy(s => new { s.UserId, s.Surname, s.Name }).Select(s => new UserUnlockTSDto
            {
                FullName = s.Key.Surname + " " + s.Key.Name,
                Amount = s.Sum(t => t.Amount)
            }).OrderByDescending(s => s.Amount).Take(10).ToListAsync();
            int index = 1;
            foreach (var l in listUnlock)
            {
                l.Rank = index;
                index++;
            }
            return new ListUserUnlockTSDto
            {
                ListUnlock = listUnlock,
                TotalAmount = await WorkScope.GetAll<Fund>().Where(s => s.Status == FundStatus.Proceeds).Select(s => s.Amount).FirstOrDefaultAsync()
            };
        }
        [HttpGet]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async Task<List<HistoryUnlockIMSDto>> GetAllHistory()
        {
            //if (!checkSecurityCode())
            //{
            //    throw new UserFriendlyException("Timesheet server can't connect");
            //}
            var captions = getCaptions();
            var random = new Random();
            return await WorkScope.GetAll<UserUnlockIms>().OrderByDescending(s => s.CreationTime).
                Select(s => new HistoryUnlockIMSDto
                {
                    Day = s.CreationTime.ToString("dd/MM/yyy HH:mm"),
                    FullName = s.User.Surname + " " + s.User.Name,
                    Content = $"đã đóng góp ngân khố {s.Amount.ToString("#,#", CultureInfo.InvariantCulture)} VND" + ". " + captions[random.Next(captions.Count)]
                }).Take(10).ToListAsync();
        }
        private async Task<List<EmployeeLockedWeekDto>> getMyTimesheetLockedAsync1(string emailAddress)
        {
            var userId = await _userService.GetUserIdByEmail(emailAddress);
            if (!userId.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }

            return await getMyTimesheetLockedAsync(userId.Value);
        }
       

        public async Task<DateTime> getStartDateToCheckUnlockTS()
        {
            var weeksCanUnlockBefor = int.Parse(SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.WeeksCanUnlockBefor).Result);
            var now = DateTimeUtils.GetNow();

            var DateToLockTimesheetOfLastMonthCfg = await SettingManager.GetSettingValueAsync(AppSettingNames.DateToLockTimesheetOfLastMonth);
            int DateToLockTimesheetOfLastMonth = 5;
            int.TryParse(DateToLockTimesheetOfLastMonthCfg, out DateToLockTimesheetOfLastMonth);

            //var startDateToCheck = now.Day < DateToLockTimesheetOfLastMonth ? now.AddDays(1 - now.Day).AddMonths(-1).Date : now.AddDays(1 - now.Day).Date;
            var startDateToCheck = now.Day < DateToLockTimesheetOfLastMonth 
                                    ? DateTimeUtils.FirstDayOfWeek(now).AddDays(-7 * weeksCanUnlockBefor) 
                                    : DateTimeUtils.Max(DateTimeUtils.FirstDayOfMonth(now), 
                                                        DateTimeUtils.FirstDayOfWeek(now).AddDays(-7 * weeksCanUnlockBefor));

            return startDateToCheck;
        }
        private async Task<DateTime> getStartDateToCheckApprove()
        {
            var now = DateTimeUtils.GetNow();
            var DateToLockTimesheetOfLastMonthCfg = await SettingManager.GetSettingValueAsync(AppSettingNames.DateToLockTimesheetOfLastMonth);
            int DateToLockTimesheetOfLastMonth = 5;
            int.TryParse(DateToLockTimesheetOfLastMonthCfg, out DateToLockTimesheetOfLastMonth);
            var startDateToCheck = now.Day < DateToLockTimesheetOfLastMonth ? now.AddDays(1 - now.Day).AddMonths(-1).Date : now.AddDays(1 - now.Day).Date;
            return startDateToCheck;
        }

        public async Task<List<EmployeeLockedWeekDto>> getMyTimesheetLockedAsync(long userId)
        {
            //Checkunlock Employee
            var hasUnlockTimesheet = await WorkScope.GetAll<UnlockTimesheet>()
                .AnyAsync(s => s.UserId == userId && s.Type == LockUnlockTimesheetType.MyTimesheet);
            if (hasUnlockTimesheet)
            {
                return null;
            }
            var listUnlockWeek = new List<EmployeeLockedWeekDto>();

            var startDateToCheck = await getStartDateToCheckUnlockTS();

            var userInfo = (await WorkScope.GetAll<User>().Where(s => s.Id == userId)
                .Select(s => new { CreationTime = s.CreationTime.Date, UserId = s.Id })
                .FirstOrDefaultAsync());

            var userCreationDate = userInfo.CreationTime.Date;

            startDateToCheck = startDateToCheck > userCreationDate ? startDateToCheck : userCreationDate;

            var lockDate = _commonServices.getlockDateUser();

            Logger.Debug("startDateToCheck: " + startDateToCheck.ToString());
            Logger.Debug("lockDate: " + lockDate.ToString());

            if (lockDate < startDateToCheck)
            {
                return null;
            }

            var mapTimesheet = await WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.Status == TimesheetStatus.Approve || s.Status == TimesheetStatus.Pending)
                .Where(s => s.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .Where(s => s.UserId == userId)
                .Where(s => s.DateAt.Date >= startDateToCheck && s.DateAt.Date <= lockDate)
                .GroupBy(s => s.DateAt.Date)
                .Select(s => new { DateAt = s.Key, WorkingTime = s.Sum(x => x.WorkingTime) })
                .ToDictionaryAsync(s => s.DateAt);

            Logger.Debug("mapTimesheet:");
            foreach (var l in mapTimesheet)
            {
                Logger.Info(l.Key.ToString() + " - " + l.Value.ToString());
            }
            var mapOffDay = await WorkScope.GetAll<AbsenceDayDetail>()
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.Request.Status == RequestStatus.Approved)
                .Where(s => s.Request.UserId == userId)
                .Where(s => s.DateAt >= startDateToCheck && s.DateAt.Date <= lockDate)
                .GroupBy(s => s.DateAt.Date)
                .Select(s => new { DateAt = s.Key, Hour = s.Sum(x => x.Hour) })
                .ToDictionaryAsync(s => s.DateAt);

            Logger.Debug("mapOffDay:");
            foreach (var l in mapOffDay)
            {
                Logger.Info(l.Key.ToString() + " - " + l.Value.ToString());
            }

            var mapWeek = await getMapWorkingDateByWeek(startDateToCheck, lockDate, userId);
            var countWeek = 0;
            foreach (var week in mapWeek)
            {
                countWeek++;
                var listInvalidDate = new List<string>();
                Logger.Debug($"Week {countWeek}: " + DateTimeUtils.ToString(week.Key));
                foreach (var d in week.Value)
                {
                    var workingMinute = mapTimesheet.ContainsKey(d) ? mapTimesheet[d].WorkingTime : 0;

                    var offMinute = mapOffDay.ContainsKey(d) ? mapOffDay[d].Hour * 60 : 0;
                    Logger.Debug($"date={d}, workingMinute={workingMinute} offMinute={offMinute}");

                    workingMinute += (int)offMinute;
                    if (workingMinute < 480)
                    {
                        listInvalidDate.Add(d.ToString("yyyy-MM-dd"));
                        Logger.Info("invalidDate: " + d.ToString("yyyy-MM-dd"));
                    }
                }
                if (listInvalidDate.Count > 0)
                {
                    listUnlockWeek.Add(new EmployeeLockedWeekDto
                    {
                        StartDate = week.Value.FirstOrDefault().ToString("dd/MM/yyyy"),
                        EndDate = week.Value.LastOrDefault().ToString("dd/MM/yyyy"),
                        Day = listInvalidDate
                    });
                }
            }
            return listUnlockWeek;

        }
        
        private async Task<int> getTimesheetLockedOfPMAsync1(string emailAddress)
        {
            var userId = await _userService.GetUserIdByEmail(emailAddress);
            if (!userId.HasValue)
            {
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }

            return await getTimesheetLockedOfPMAsync(userId.Value);
        }

        //Checkunlock PM 
        public async Task<int> getTimesheetLockedOfPMAsync(long userId)
        {
            var isPM = WorkScope.GetAll<ProjectUser>()
                 .Where(s => s.UserId == userId)
                 .Where(s => s.Type == ProjectUserType.PM)
                 .Where(s => s.Project.Status == ProjectStatus.Active)
                 .Any();

            if (!isPM)
            {
                return 0;
            }

            //Checkunlock PM
            var hasUnlockTimesheet = await WorkScope.GetAll<UnlockTimesheet>().AnyAsync(s => s.UserId == userId && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet);
            if (hasUnlockTimesheet)
            {
                return 0;
            }
            int count = 0;

            var startMonth = await getStartDateToCheckApprove();

            var lockDate = _commonServices.getlockDatePM();
            var qprojectIds = WorkScope.GetAll<ProjectUser>().Where(s => s.UserId == userId && s.Type == ProjectUserType.PM).Select(s => s.ProjectId).Distinct();

            var qtimesheets = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.Status == TimesheetStatus.Pending)
                .Where(s => s.IsUnlockedByEmployee != true)
                .Where(s => s.DateAt >= startMonth && s.DateAt.Date <= lockDate)
                .Select(s => new { s.ProjectTask.ProjectId, DateAt = s.DateAt.Date });

            var timesheets = await (from projectId in qprojectIds
                                    join ts in qtimesheets on projectId equals ts.ProjectId
                                    group ts by ts.DateAt.Date into gr
                                    select gr.Key).ToListAsync();


            var mapWeek = await getMapWorkingDateByWeek(startMonth, lockDate, userId);

            if (mapWeek == null)
            {
                return 0;
            }
            Logger.Debug("mapWeek.Count=" + mapWeek.Count);

            foreach (var listDayOfWeek in mapWeek.Values)
            {
                Logger.Debug("listDayOfWeek.Count=" + listDayOfWeek.Count);
                var startWeek = listDayOfWeek.FirstOrDefault();
                var endWeek = listDayOfWeek.LastOrDefault();
                var hasTimesheetPending = timesheets.Any(s => s.Date >= startWeek.Date && s.Date <= endWeek.Date);
                if (hasTimesheetPending) { count++; }
            }
            return count;
        }
        private async Task<Dictionary<DateTime, List<DateTime>>> getMapWorkingDateByWeek(DateTime startMonth, DateTime lockDate, long userId)
        {
            if (lockDate < startMonth) return null;
            else
            {
                var dayOffs = await WorkScope.GetAll<DayOffSetting>()
                    .Where(s => s.DayOff >= startMonth.Date && s.DayOff.Date <= lockDate)
                    .Select(s => s.DayOff.Date)
                    .ToListAsync();

                Logger.Debug("getAllWeek: dayOffs.Count=" + dayOffs.Count);
                List<DateTime> listDay = null;
                var listWeek = new List<List<DateTime>>();
                var mapResult = new Dictionary<DateTime, List<DateTime>>();
                for (var date = startMonth.Date; date <= lockDate; date = date.AddDays(1))
                {

                    Logger.Debug("date: " + date.ToString("yyyy-MM-dd"));
                    var monday = DateTimeUtils.FirstDayOfWeek(date);
                    if (mapResult.ContainsKey(monday))
                    {
                        listDay = mapResult[monday];
                    }
                    else
                    {
                        listDay = new List<DateTime>();
                        mapResult.Add(monday, listDay);
                    }

                    if (date.DayOfWeek == DayOfWeek.Saturday
                        || date.DayOfWeek == DayOfWeek.Sunday
                        || dayOffs.Contains(date))
                    {
                        continue;
                    }
                    listDay.Add(date);
                }
                return mapResult;
            }
        }
        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async System.Threading.Tasks.Task UnlockToLogTimesheet(string emailAddress)
        {
            if (!checkSecurityCode())
            {
                throw new UserFriendlyException("Wrong security code");
            }
            var userId = await _userService.GetUserIdByEmail(emailAddress);
            if (!userId.HasValue)
            {
                Logger.Error("Not found user with email " + emailAddress);
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }
            if(IsAlreadyUnlockToLog(userId.Value))
            {
                return;
            }

            var listUnlockWeekEmployee = await getMyTimesheetLockedAsync(userId.Value);

            var timesLockedEmployee = listUnlockWeekEmployee == null ? 0 : listUnlockWeekEmployee.Count();

            if (timesLockedEmployee > 0)
            {
                var amount = (timesLockedEmployee >= 4 ? 100000 : timesLockedEmployee * 20000);
                var fund = await WorkScope.GetAll<Fund>().Where(s => s.Status == FundStatus.Proceeds).FirstOrDefaultAsync();
                if (fund == null)
                {
                    await WorkScope.InsertAsync<Fund>(new Fund
                    {
                        Amount = amount,
                        Status = FundStatus.Proceeds
                    });
                }
                else
                {
                    fund.Amount += amount;
                    await WorkScope.UpdateAsync(fund);
                }
                await WorkScope.InsertAsync<UserUnlockIms>(new UserUnlockIms
                {
                    UserId = userId.Value,
                    Times = timesLockedEmployee,
                    IsPayment = false,
                    Type = LockUnlockTimesheetType.MyTimesheet,
                    Amount = amount
                });

                //Add unlock user
                await WorkScope.InsertAsync<UnlockTimesheet>(new UnlockTimesheet
                {
                    UserId = userId.Value,
                    Type = LockUnlockTimesheetType.MyTimesheet
                });
            }
        }
        [HttpPost]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public async System.Threading.Tasks.Task UnlockToApproveTimesheet(string emailAddress)
        {
            if (!checkSecurityCode())
            {
                throw new UserFriendlyException("Wrong security code");
            }
            var userId = await _userService.GetUserIdByEmail(emailAddress);
            if (!userId.HasValue)
            {
                Logger.Error("Not found user with email " + emailAddress);
                throw new UserFriendlyException("Not found user with email " + emailAddress);
            }
            if(IsAlreadyUnlockToApprove(userId.Value))
            {
                return;
            }
            var timesLockedPM = await getTimesheetLockedOfPMAsync(userId.Value);

            //Có lỗi
            if (timesLockedPM > 0)
            {
                await UnlockToApproveTimesheet(userId.Value, timesLockedPM);
            }
            //Không lỗi
            else
            {
                await UnlockToApproveTimesheet(userId.Value, 1);
            }
        }
        private async System.Threading.Tasks.Task UnlockToApproveTimesheet(long userId, int timesLockedPM)
        {
            float fMoneyPMUnlockTimeSheet = getMoneyPMUnlockTimeSheet();           
            var amount = timesLockedPM * fMoneyPMUnlockTimeSheet;
            var fund = await WorkScope.GetAll<Fund>().Where(s => s.Status == FundStatus.Proceeds).FirstOrDefaultAsync();
            if (fund == null)
            {
                await WorkScope.InsertAsync<Fund>(new Fund
                {
                    Amount = amount,
                    Status = FundStatus.Proceeds
                });
            }
            else
            {
                fund.Amount += amount;
                await WorkScope.UpdateAsync(fund);
            }
            await WorkScope.InsertAsync<UserUnlockIms>(new UserUnlockIms
            {
                UserId = userId,
                Times = timesLockedPM,
                IsPayment = false,
                Type = LockUnlockTimesheetType.ApproveRejectTimesheet,
                Amount = amount
            });
            //Add unlock pm
            await WorkScope.InsertAsync<UnlockTimesheet>(new UnlockTimesheet
            {
                UserId = userId,
                Type = LockUnlockTimesheetType.ApproveRejectTimesheet
            }); 
      
        }
        private bool IsAlreadyUnlockToLog(long userId)
        {
            return WorkScope.GetAll<UnlockTimesheet>()
                .Any(s => s.UserId == userId && s.Type == LockUnlockTimesheetType.MyTimesheet);
        }
        private bool IsAlreadyUnlockToApprove(long userId)
        {
            return WorkScope.GetAll<UnlockTimesheet>()
                .Any(s => s.UserId == userId && s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet);
        }
        private bool IsPM(long userId)
        {
            return WorkScope.GetAll<ProjectUser>()
                 .Where(s => s.UserId == userId)
                 .Where(s => s.Type == ProjectUserType.PM)
                 .Where(s => s.Project.Status == ProjectStatus.Active)
                 .Any();
        }    

        private bool checkSecurityCode()
        {
            var securityCode = SettingManager.GetSettingValue(AppSettingNames.SecurityCode);
            var header = _httpContextAccessor.HttpContext.Request.Headers;
            var securityCodeHeader = header["securityCode"];
            if (securityCode == securityCodeHeader)
                return true;
            return false;
        }
        private List<string> getCaptions()
        {
            return new List<string>()
            {
                "Cảm ơn bạn rất nhiều!",
                "Unlock liền tay, bay ngay vài chục.",
                "Ngân quỹ đang đầy lên nhiều đó.",
                "Bạn ơi đừng nộp nữa, nhà mình còn gì đâu.",
                "Bạn quả là một thiên thần!",
                "Mình thích thì mình phạt thôi.",
                "Hãy cố gắng lần sau nhé.",
                "Quả là không là gì so với thu nhập.",
                "Chúc bạn may mắn lần sau.",
                "Ngân khố đang giàu lên vì bạn.",
                "Cảnh sát Hiền thích điều này.",
                "Ngô Thu Hiền đã ghim bạn.",
                "Ông trời tạo ra địa chấn, unlock timesheet để làm điểm nhấn.",
                "Của cho không bằng cách cho. Hãy tiếp tục phát huy.",
                "Một đồng tiền công không bằng vài chục tiền phạt.",
                "Ông bà già ta lo hết."
            };
        }
        private float getMoneyPMUnlockTimeSheet()
        {
            var MoneyPMUnlockTimeSheet = SettingManager.GetSettingValueForApplication(AppSettingNames.MoneyPMUnlockTimeSheet);
            float fMoneyPMUnlockTimeSheet;
            bool success = float.TryParse(MoneyPMUnlockTimeSheet, out fMoneyPMUnlockTimeSheet);
            if (!success)
            {
                fMoneyPMUnlockTimeSheet = 20000;
            }
            return fMoneyPMUnlockTimeSheet;
        }
    }
}
