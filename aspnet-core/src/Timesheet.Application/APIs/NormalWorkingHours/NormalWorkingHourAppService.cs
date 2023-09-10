using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.UI;
using ClosedXML.Excel;
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
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.NormalWorkingHours.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.NormalWorkingHours
{
    //[AbpAuthorize(Ncc.Authorization.PermissionNames.Report_NormalWorking)]
    [AbpAuthorize]
    public class NormalWorkingHourAppService : AppServiceBase
    {
        //public Task<Object> Test(int year, int month)
        //{
        //    byte[] arrDayOfWeek = { 6, 0, 1, 2, 3, 4, 5 };
        //    var now = DateTimeUtils.GetNow();
        //    var dayOfWeek = (int)Enum.Parse(typeof(System.DayOfWeek), now.DayOfWeek.ToString());
        //    var mondayOrTheFirst = now.Day == 1 ? now
        //        : now.AddDays(-arrDayOfWeek[dayOfWeek]);

        //    return mondayOrTheFirst;
        //}
        public NormalWorkingHourAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [HttpPost]
        [AbpAllowAnonymous]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_NormalWorking,Ncc.Authorization.PermissionNames.Report_NormalWorking_View)]
        public async Task<GridResult<GetNormalWorkingHourDto>> GetAllPagging(GridParam input, int year, int month, long? branchId, long? projectId,
                                                                            bool isThanDefaultWorking, int? checkInFilter, int tsStatusFilter)
        {
            //bool isViewLevel = await this.IsGrantedAsync(Ncc.Authorization.PermissionNames.Report_NormalWorking_ViewLevel);
            byte[] arrDayOfWeek = { 7, 1, 2, 3, 4, 5, 6 };
            var now = DateTimeUtils.GetNow();
            var dayOfWeek = (int)Enum.Parse(typeof(System.DayOfWeek), now.DayOfWeek.ToString());

            //var lockDate = now.Day == 1 ? now.AddDays(-1)
            //   : now.AddDays(-arrDayOfWeek[dayOfWeek]);

            var lockMonth = now.AddDays(-(now.Day));
            var lockWeek = now.AddDays(-arrDayOfWeek[dayOfWeek]);
            var lockDate = lockWeek > lockMonth ? lockWeek : lockMonth;
            var lockDateAfterUnlock = GetFirstDateCanLogTS().Result;

            var unlockTSs = await WorkScope.GetAll<UnlockTimesheet>()
                .Select(s => new { s.UserId, s.Type }).ToListAsync();

            var unlockUserIds = unlockTSs.Where(s => s.Type == LockUnlockTimesheetType.MyTimesheet)
                .Select(s => s.UserId).ToList();
            var unlockPMIds = unlockTSs.Where(s => s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet)
                .Select(s => s.UserId).ToList();

            var pmIds = await WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Type == ProjectUserType.PM)
                .Select(s => s.UserId).Distinct().ToListAsync();

            var dayOffSettings = await WorkScope.GetAll<DayOffSetting>()
                     .Where(s => s.DayOff.Year == year && s.DayOff.Month == month)
                     .Select(s => s.DayOff.Date).ToListAsync();

            var projectMembers = await WorkScope.GetAll<ProjectUser>()
               .Where(s => !projectId.HasValue || s.ProjectId == projectId)
               .Where(s => s.Type != ProjectUserType.DeActive)
               .Select(s => s.UserId).Distinct().ToListAsync();

            var absencedays = await WorkScope.GetAll<AbsenceDayDetail>()
             .Where(s => s.DateAt.Year == year && s.DateAt.Month == month)
             .Where(s => s.Request.Status != RequestStatus.Rejected)
             .Select(s => new AbsenceDayDetailNormal
             {
                 UserId = s.Request.UserId,
                 DateAt = s.DateAt.Date,
                 DateType = s.DateType,
                 Hour = s.Hour,
                 Type = s.Request.Type,
                 AbsenceTime = s.AbsenceTime
             })
             .ToListAsync();

            var today = DateTimeUtils.GetNow();
            var listTimekeeping = await WorkScope.GetAll<Timekeeping>()
                 .Select(s => new CheckInDto
                 {
                     UserId = s.UserId,
                     CheckIn = s.CheckIn,
                     CheckOut = s.CheckOut,
                     DateAt = s.DateAt.Date
                 })
                 .Where(s => s.DateAt.Year == year && s.DateAt.Month == month)
                 .Where(s => s.UserId.HasValue)
                 .Where(s => s.DateAt.Date <= today)
                 .Where(s => !dayOffSettings.Contains(s.DateAt.Date))
                 .Where(s => s.DateAt.DayOfWeek != DayOfWeek.Sunday)
                 .ToListAsync();

            var listUserIdWarning = new List<long>();
            if (isThanDefaultWorking)
            {
                listUserIdWarning = GetListUserIdThanNormalWorkingHour(year, month, branchId, projectId, absencedays, projectMembers, dayOffSettings);
            }

            IEnumerable<long> listUserIdNoCheckIn = null;
            Dictionary<long, IEnumerable<DateTime>> dicUserIdToDatesNoCheckIn = new Dictionary<long, IEnumerable<DateTime>>();
            if (checkInFilter.HasValue)
            {
                dicUserIdToDatesNoCheckIn = GetDicUserIdToDatesCheckIn(year, month, absencedays, dayOffSettings, tsStatusFilter, checkInFilter.Value);
                listUserIdNoCheckIn = dicUserIdToDatesNoCheckIn.Keys;
            }

            TimesheetStatus[] listTimesheetStatus = new TimesheetStatus[] { TimesheetStatus.Pending, TimesheetStatus.Approve };
            var query = from u in WorkScope.GetAll<User>()
                        .Where(s => s.IsActive == true)
                        .Where(s => !isThanDefaultWorking || listUserIdWarning.Contains(s.Id))
                        .Where(s => !checkInFilter.HasValue || listUserIdNoCheckIn.Contains(s.Id))
                        .Where(s => !branchId.HasValue || s.BranchId == branchId)
                        .Where(s => !projectId.HasValue || projectMembers.Contains(s.Id))
                        join t in WorkScope.GetAll<MyTimesheet>()
                        .Where(ts => tsStatusFilter == (int)TsStatusFilter.PendingAndApproved ? listTimesheetStatus.Contains(ts.Status) : ts.Status == TimesheetStatus.Approve)
                        .Where(ts => ts.DateAt.Year == year && ts.DateAt.Month == month)
                        .Where(ts => DateTimeUtils.ListDayOfWeek().Contains(ts.DateAt.DayOfWeek))
                        .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                        on u.Id equals t.UserId into tt
                        orderby u.Id
                        select new GetNormalWorkingHourDto
                        {
                            EmailAddress = u.EmailAddress,
                            UserName = u.UserName,
                            UserId = u.Id,
                            Surname = u.Surname,
                            Name = u.Name,
                            FullName = u.Surname + " " + u.Name,
                            AvatarPath = !string.IsNullOrEmpty(u.AvatarPath) ? u.AvatarPath : "",
                            //Level = isViewLevel ? u.Level : null,
                            Type = u.Type,
                            Sex = u.Sex,
                            BranchDisplayName = u.Branch.DisplayName,
                            BranchColor = u.Branch.Color,
                            TotalWorkingHourOfMonth = (double)tt.Where(s => s.DateAt.DayOfWeek != DayOfWeek.Saturday).Sum(s => s.WorkingTime) / 60,
                            TotalWorkingday = 0,//Math.Round((double)tt.Where(s => s.DateAt.DayOfWeek != DayOfWeek.Saturday).Sum(s => s.WorkingTime) / 480, 2),
                            //TotalOpenTalk = tt.Count(s => s.DateAt.DayOfWeek == DayOfWeek.Saturday && s.WorkingTime > 0),
                            TotalWorkingHour = 0,
                            ListWorkingHour = tt.Select(s => new
                            {
                                Date = s.DateAt.Day,
                                DayName = s.DateAt.DayOfWeek,
                                WorkingHour = s.WorkingTime,
                                DateAt = s.DateAt,
                            }).GroupBy(s => new { s.DayName, s.Date, s.DateAt })
                            .Select(x => new WorkingHourDto
                            {
                                Date = x.Key.Date,
                                DayName = x.Key.DayName.ToString(),
                                WorkingHour = (double)x.Sum(m => m.WorkingHour) / 60,
                                IsOpenTalk = (double)x.Sum(m => m.WorkingHour) / 60 != 0 && x.Key.DayName == DayOfWeek.Saturday ? true : false,
                                OffHour = absencedays.Where(s => s.DateAt.Day == x.Key.Date && s.UserId == u.Id && s.Type == RequestType.Off).Select(h => h.Hour).Sum(),
                                //IsRequestAbsenceType = absencedayMonths.FirstOrDefault(s => s.UserId == u.Id && s.DateAt.Day == x.Key.Date).DateType
                                IsOffDay = DateTimeUtils.IsOffDay(dayOffSettings, x.Key.DateAt),
                            }).OrderBy(x => x.Date)
                        };

            var result = await query.GetGridResult(query, input);

            foreach (var item in result.Items)
            {
                item.IsUnlock = unlockUserIds.Contains(item.UserId);
                item.IsPM = pmIds.Contains(item.UserId);
                item.IsUnlockPM = item.IsPM ? unlockPMIds.Contains(item.UserId) : false;
                item.TotalOpenTalk = item.ListWorkingHour.Where(s => s.IsOpenTalk).Count();
                item.TotalWorkingHour = item.TotalWorkingHourOfMonth + (item.TotalOpenTalk >= 2 ? 8 : item.TotalOpenTalk * 4);

                var listWorkingHour = item.ListWorkingHour.ToList();

                var noCheckIn = dicUserIdToDatesNoCheckIn.ContainsKey(item.UserId) ? dicUserIdToDatesNoCheckIn[item.UserId] : null;

                var listTimekeepingByUser = listTimekeeping.Where(x => x.UserId == item.UserId).ToList();
                for (var date = new DateTime(year, month, 1); date.Month == month; date = date.AddDays(1))
                {
                    var workingDetail = listWorkingHour.Find(s => s.Date == date.Day);
                    var timekeepingByUserAtDate = listTimekeepingByUser.FirstOrDefault(s => s.DateAt.Day == date.Day);
                    if (workingDetail == null)
                    {
                        workingDetail = new WorkingHourDto
                        {
                            Date = date.Day,
                            DayName = date.DayOfWeek.ToString(),
                            IsOpenTalk = false,
                        };
                        listWorkingHour.Add(workingDetail);
                    }
                    workingDetail.IsLock = item.IsUnlock ? date < lockDateAfterUnlock : date < lockDate;
                    workingDetail.IsOffDaySetting = dayOffSettings.Contains(date);

                    var listAbsenceDetaiInDay = new List<AbsenceDetaiInDay>();
                    absencedays.Where(s => s.DateAt.Day == date.Day && s.UserId == item.UserId).ToList().ForEach(e =>
                    {
                        listAbsenceDetaiInDay.Add(
                            new AbsenceDetaiInDay
                            {
                                AbsenceType = e.DateType,
                                Hour = e.Hour,
                                AbsenceTime = e.AbsenceTime,
                                Type = e.Type,
                            }
                        );
                    });
                    workingDetail.ListAbsenceDetaiInDay = listAbsenceDetaiInDay;

                    workingDetail.IsNoCheckIn = noCheckIn != null && noCheckIn.Contains(date);

                    workingDetail.CheckIn = timekeepingByUserAtDate?.CheckIn;
                    workingDetail.CheckOut = timekeepingByUserAtDate?.CheckOut;

                }
                item.ListWorkingHour = listWorkingHour.OrderBy(s => s.Date);
            }
            return result;
        }


        private Dictionary<long, IEnumerable<DateTime>> GetDicUserIdToDatesCheckIn(int year, int month, List<AbsenceDayDetailNormal> absencedays, List<DateTime> dayOffSettings, int tsStatusFilter, int checkInFilter)
        {
            var today = DateTimeUtils.GetNow();
            var dicUserIdNoCheckInOut = WorkScope.GetAll<Timekeeping>()
                .Select(s => new
                {
                    s.UserId,
                    s.CheckIn,
                    s.CheckOut,
                    DateAt = s.DateAt.Date
                })
                .Where(s => checkInFilter == (int)CheckInFilter.NoCheckIn ? s.CheckIn == null || s.CheckIn == "" :
                            checkInFilter == (int)CheckInFilter.NoCheckOut ? s.CheckOut == null || s.CheckOut == "" :
                            ((s.CheckIn == null || s.CheckIn == "") && (s.CheckOut == null || s.CheckOut == "")))
                .Where(s => s.DateAt.Year == year && s.DateAt.Month == month)
                .Where(s => s.DateAt.Date <= today)
                .Where(s => s.UserId.HasValue)
                 .ToList()
                .Where(s => !dayOffSettings.Contains(s.DateAt.Date))
                .Where(s => s.DateAt.DayOfWeek != DayOfWeek.Sunday)
                .GroupBy(s => s.UserId.Value)
                .ToDictionary(s => s.Key, s => s.Select(x => x.DateAt.Date).AsEnumerable());


            var dataUserCheckIn = new Dictionary<long, IEnumerable<DateTime>>();

            var dicAbsencedays = absencedays
                        .Where(x => x.Type == RequestType.Off)
                        .Where(x => x.DateType == DayType.Fullday)
                        .Select(x => new { x.UserId, x.DateAt })
                        .Distinct()
                        .GroupBy(s => s.UserId)
                        .ToDictionary(s => s.Key, s => s.Select(x => x.DateAt.Date).AsEnumerable());

            foreach (var userId in dicUserIdNoCheckInOut.Keys)
            {
                if (!dicAbsencedays.ContainsKey(userId))
                {
                    dataUserCheckIn.Add(userId, dicUserIdNoCheckInOut[userId]);
                }
            }

            if (checkInFilter == (int)CheckInFilter.NoCheckInAndNoCheckOutButHaveTs)
            {
                TimesheetStatus[] listTimesheetStatus = new TimesheetStatus[] { TimesheetStatus.Pending, TimesheetStatus.Approve };

                var dicUserIdLogTsOnDate = WorkScope.GetAll<MyTimesheet>()
                            .Where(ts => tsStatusFilter == (int)TsStatusFilter.PendingAndApproved ? listTimesheetStatus.Contains(ts.Status) :
                            ts.Status == TimesheetStatus.Approve)
                            .Where(ts => ts.DateAt.Year == year && ts.DateAt.Month == month)
                            .Where(ts => ts.DateAt.DayOfWeek != DayOfWeek.Sunday)
                            .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours).Select(x => new { x.UserId, x.DateAt })
                            .Distinct()
                            .GroupBy(s => s.UserId)
                            .ToDictionary(s => s.Key, s => s.Select(x => x.DateAt.Date).AsEnumerable());
                var dateNoCheckInAndCheckOutButHaveTs = new Dictionary<long, IEnumerable<DateTime>>();
                foreach (var userId in dataUserCheckIn.Keys)
                {
                    if (dicUserIdLogTsOnDate.ContainsKey(userId))
                    {
                        var listDateNoCheckInAndCheckOutButHaveTs = dataUserCheckIn[userId].Intersect(dicUserIdLogTsOnDate[userId]);
                        if (listDateNoCheckInAndCheckOutButHaveTs.Count() != 0)
                        {
                            dateNoCheckInAndCheckOutButHaveTs.Add(userId, listDateNoCheckInAndCheckOutButHaveTs);
                        }
                    }
                }
                return dateNoCheckInAndCheckOutButHaveTs;
            }
            return dataUserCheckIn;
        }
        private async Task<DateTime> GetFirstDateCanLogTS()
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
        private List<long> GetListUserIdThanNormalWorkingHour(int year, int month, long? branchId, long? projectId, List<AbsenceDayDetailNormal> absencedays,
                                                                List<long> projectMembers, List<DateTime> dayOffSettings)
        {
            var query = from u in WorkScope.GetAll<User>()
                        .Where(s => s.IsActive == true)
                        .Where(s => !branchId.HasValue || s.BranchId == branchId)
                        .Where(s => !projectId.HasValue || projectMembers.Contains(s.Id))
                        .Where(s => s.EndDateAt == null)
                        join t in WorkScope.GetAll<MyTimesheet>()
                        .Where(ts => ts.Status == TimesheetStatus.Approve)
                        .Where(ts => ts.DateAt.Year == year && ts.DateAt.Month == month)
                        .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                        on u.Id equals t.UserId into tt
                        orderby u.Id
                        select new GetNormalWorkingHourDto
                        {
                            UserId = u.Id,
                            ListWorkingHour = tt.Select(s => new
                            {
                                Date = s.DateAt,
                                DayName = s.DateAt.DayOfWeek,
                                WorkingHour = s.WorkingTime,
                            }).GroupBy(s => new { s.DayName, s.Date })
                            .Select(x => new WorkingHourDto
                            {
                                Date = x.Key.Date.Day,
                                DayName = x.Key.DayName.ToString(),
                                WorkingHour = (double)x.Sum(m => m.WorkingHour) / 60,
                                IsOpenTalk = (double)x.Sum(m => m.WorkingHour) / 60 != 0 && x.Key.DayName == DayOfWeek.Saturday ? true : false,
                                OffHour = absencedays.Where(s => s.DateAt.Day == x.Key.Date.Day && s.UserId == u.Id && s.Type == RequestType.Off).Select(h => h.Hour).Sum(),
                                IsOffDay = DateTimeUtils.IsOffDay(dayOffSettings, x.Key.Date),
                            }).OrderBy(x => x.Date)
                        };

            var result = new List<long>();
            foreach (var item in query.ToList())
            {
                var listWorkingHour = item.ListWorkingHour.Where(x => !x.IsOpenTalk).ToList();

                if (listWorkingHour.Sum(x => x.WorkingHour) > (listWorkingHour.Count() * 8 - listWorkingHour.Sum(x => x.OffHour)))
                {
                    result.Add(item.UserId);
                    continue;
                }

                var listWorkingHourSaturday = item.ListWorkingHour.Where(x => x.DayName == DayOfWeek.Saturday.ToString()).ToList();

                if (listWorkingHourSaturday.Sum(x => x.WorkingHour) > (listWorkingHourSaturday.Count() * 4 - listWorkingHourSaturday.Sum(x => x.OffHour)))
                {
                    result.Add(item.UserId);
                    continue;
                }

                var isOffDay = item.ListWorkingHour.Where(x => x.IsOffDay).Select(x => x.IsOffDay).FirstOrDefault();

                if (isOffDay)
                {
                    result.Add(item.UserId);
                }
            }
            return result;
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Report_NormalWorking)]
        public async Task<Byte[]> ExportNormalWorking(GridParam input, int year, int month, long? branchId, long? projectId,
                                                                            bool isThanDefaultWorking, int? checkInFilter, int tsStatusFilter)
        {
            var normalWorkingHourList = GetAllPagging(input, year, month, branchId, projectId,
                                                                             isThanDefaultWorking, checkInFilter, tsStatusFilter).Result.Items;
            try
            {
                int currentRow = 1;
                using (var workBook = new XLWorkbook())
                {
                    workBook.Author = "NCCSoftHR";
                    //Title
                    var ws = workBook.Worksheets.Add("Report Normal Working");
                    //Header
                    ws.Cell(currentRow, 1).Value = "STT";
                    ws.Cell(currentRow, 2).Value = "Full Name";
                    ws.Cell(currentRow, 3).Value = "Branch";
                    ws.Cell(currentRow, 4).Value = "User Type";
                    ws.Cell(currentRow, 5).Value = "User Level";
                    ws.Cell(currentRow, 6).Value = "Total Opentalk Hours";
                    ws.Cell(currentRow, 7).Value = "Total Working Hour";
                    ws.Cell(currentRow, 8).Value = "Total Working Day";

                    ws.Column(1).Width = 5;
                    ws.Column(2).Width = 25;

                    var header = ws.Range("B1:H1");
                    header.Style.Alignment.SetWrapText(true);
                    string[] weekdays = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };


                    int indexColumnHeader = 9;
                    for (var date = new DateTime(year, month, 1); date.Month == month; date = date.AddDays(1))
                    {
                        ws.Cell(currentRow, indexColumnHeader).Value = date.Day + "-" + weekdays[(int)date.DayOfWeek];
                        ws.Cell(currentRow, indexColumnHeader).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Cell(currentRow, indexColumnHeader).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                        indexColumnHeader++;
                    }

                    //Content
                    foreach (var normalWorkingHour in normalWorkingHourList)
                    {
                        currentRow++;
                        ws.Cell(currentRow, 1).Value = currentRow - 1;
                        ws.Cell(currentRow, 2).Value = normalWorkingHour.FullName;
                        ws.Cell(currentRow, 3).Value = normalWorkingHour.BranchDisplayName;
                        ws.Cell(currentRow, 4).Value = normalWorkingHour.Type.HasValue ? CommonUtils.UserTypeName(normalWorkingHour.Type) : "No data";
                        ws.Cell(currentRow, 5).Value = normalWorkingHour.Level.HasValue ? CommonUtils.UserLevelName(normalWorkingHour.Level) : "No data";
                        ws.Cell(currentRow, 6).Value = normalWorkingHour.TotalOpenTalk;
                        ws.Cell(currentRow, 7).Value = normalWorkingHour.TotalWorkingHour == -1 ? "joined" : normalWorkingHour.TotalWorkingHour.ToString();
                        ws.Cell(currentRow, 8).Value = normalWorkingHour.TotalWorkingday;

                        int indexColumn = 9;
                        foreach (var workingHour in normalWorkingHour.ListWorkingHour)
                        {
                            ws.Cell(currentRow, indexColumn).Value = FillDataCell(workingHour); ;
                            ws.Cell(currentRow, indexColumn).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            indexColumn++;
                        }

                    }
                    ws.Cells().Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    using (var memoryStream = new MemoryStream())
                    {
                        workBook.SaveAs(memoryStream);
                        byte[] file = memoryStream.ToArray();
                        return file;
                    }
                }
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(String.Format($"Error: {e.Message}"));
            }
        }

        private string GetInfoAbsence(AbsenceDetaiInDay absenceDetaiInDay)
        {
            if (absenceDetaiInDay == null) return "";
            var result = "O-";
            switch (absenceDetaiInDay.Type)
            {
                case RequestType.Onsite:
                    result = "OS-";
                    break;
                case RequestType.Remote:
                    result = "R-";
                    break;
                default:
                    result = "O-";
                    break;
            }

            if (absenceDetaiInDay.AbsenceType == DayType.Fullday)
            {
                return result += "FD";
            }
            if (absenceDetaiInDay.AbsenceType == DayType.Morning)
            {
                return result += "M";
            }
            if (absenceDetaiInDay.AbsenceType == DayType.Afternoon)
            {
                return result += "A";
            }
            if (absenceDetaiInDay.AbsenceType == DayType.Custom &&
              absenceDetaiInDay.AbsenceTime == OnDayType.DiMuon)
            {
                return "ĐM " + absenceDetaiInDay.Hour;
            }
            return "VS " + absenceDetaiInDay.Hour;

        }

        private string FillDataCell(WorkingHourDto item)
        {
            var result = "";
            var infoAbsence = "";
            result += item.WorkingHour + "\r\n";
            item.ListAbsenceDetaiInDay.ForEach(absenceDetaiInDay =>
            {
                infoAbsence += GetInfoAbsence(absenceDetaiInDay) + "\r\n";
            });
            result += infoAbsence;
            result += item.CheckInShow + "\r\n";
            result += item.CheckOutShow;
            return result;
        }

        [HttpGet]
        public async Task<GetNormalWorkingHourByUserLoginDto> GetNormalWorkingHourByUserLogin(int year, int month, MyTimesheetFilterStatus status)
        {
            var userId = AbpSession.UserId.Value;
            var now = DateTimeUtils.GetNow();
            var dayOfWeek = (int)Enum.Parse(typeof(System.DayOfWeek), now.DayOfWeek.ToString());
            byte[] arrDayOfWeek = { 7, 1, 2, 3, 4, 5, 6 };
            var lockMonth = now.AddDays(-(now.Day));
            var lockWeek = now.AddDays(-arrDayOfWeek[dayOfWeek]);
            var lockDate = lockWeek > lockMonth ? lockWeek : lockMonth;
            var lockDateAfterUnlock = GetFirstDateCanLogTS().Result;

            var unlockTSs = await WorkScope.GetAll<UnlockTimesheet>()
                .Select(s => new { s.UserId, s.Type }).ToListAsync();

            var unlockUserIds = unlockTSs.Where(s => s.Type == LockUnlockTimesheetType.MyTimesheet)
                .Select(s => s.UserId).ToList();
            var unlockPMIds = unlockTSs.Where(s => s.Type == LockUnlockTimesheetType.ApproveRejectTimesheet)
                .Select(s => s.UserId).ToList();

            var dayOffSettings = await WorkScope.GetAll<DayOffSetting>()
                     .Where(s => s.DayOff.Year == year && s.DayOff.Month == month)
                     .Select(s => s.DayOff.Date).ToListAsync();

            var absencedays = await WorkScope.GetAll<AbsenceDayDetail>()
             .Where(s => s.DateAt.Year == year && s.DateAt.Month == month)
             .Where(s => s.Request.Status != RequestStatus.Rejected)
             .Where(s => s.Request.UserId == userId)
             .Select(s => new AbsenceDayDetailNormal
             {
                 UserId = s.Request.UserId,
                 DateAt = s.DateAt.Date,
                 DateType = s.DateType,
                 Hour = s.Hour,
                 Type = s.Request.Type,
                 AbsenceTime = s.AbsenceTime
             })
             .ToListAsync();

            var listTimekeeping = await WorkScope.GetAll<Timekeeping>()
                 .Select(s => new CheckInDto
                 {
                     UserId = s.UserId,
                     CheckIn = s.CheckIn,
                     CheckOut = s.CheckOut,
                     DateAt = s.DateAt.Date
                 })
                 .Where(s => s.DateAt.Year == year && s.DateAt.Month == month)
                 .Where(s => s.UserId == userId)
                 .Where(s => s.DateAt.Date <= now)
                 .Where(s => !dayOffSettings.Contains(s.DateAt.Date))
                 .Where(s => s.DateAt.DayOfWeek != DayOfWeek.Sunday)
                 .ToListAsync();

            var openTalkTaskId = await SettingManager.GetSettingValueAsync(AppSettingNames.OpenTalkTaskId);

            var listMyTimesheet = await WorkScope.GetAll<MyTimesheet>()
                        .Where(ts=>ts.UserId == userId)
                        .Where(ts => ts.DateAt.Year == year && ts.DateAt.Month == month)
                        .WhereIf(status == MyTimesheetFilterStatus.All, ts => true)
                        .WhereIf(status == MyTimesheetFilterStatus.Pending, ts => ts.Status == TimesheetStatus.Pending)
                        .WhereIf(status == MyTimesheetFilterStatus.Approved, ts => ts.Status == TimesheetStatus.Approve)
                        .WhereIf(status == MyTimesheetFilterStatus.New, ts => ts.Status == TimesheetStatus.None)
                        .WhereIf(status == MyTimesheetFilterStatus.Rejected, ts => ts.Status == TimesheetStatus.Reject)
                        .WhereIf(status == MyTimesheetFilterStatus.PendingApproved, ts => ts.Status == TimesheetStatus.Pending ||
                                                                                        ts.Status == TimesheetStatus.Approve)
                        .Where(ts => DateTimeUtils.ListDayOfWeek().Contains(ts.DateAt.DayOfWeek))
                        .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                         .Select(s => new
                         {
                             DayName = s.DateAt.DayOfWeek,
                             WorkingHour = s.WorkingTime,
                             DateAt = s.DateAt,
                             IsOpenTalk = s.ProjectTask == null ? false : s.ProjectTask.Id == long.Parse(openTalkTaskId) ? true : false,
                         }).GroupBy(s => new { s.DayName, s.DateAt })
                            .Select(x => new WorkingHourDto
                            {
                                Date = x.Key.DateAt.Day,
                                DayName = x.Key.DateAt.DayOfWeek.ToString(),
                                WorkingHour = (double)x.Sum(m => m.WorkingHour) / 60,
                                TotalOpenTalkPerDay = x.Count(m => m.IsOpenTalk),
                                IsOffDay = DateTimeUtils.IsOffDay(dayOffSettings, x.Key.DateAt),
                                OffHour = absencedays.Where(abs => abs.DateAt.Day == x.Key.DateAt.Day && abs.Type == RequestType.Off).Select(h => h.Hour).Sum(),
                            }).OrderBy(x => x.Date).ToListAsync();


            var isUnlock = unlockUserIds.Contains(userId);

            for (var date = new DateTime(year, month, 1); date.Month == month; date = date.AddDays(1))
            {
                var workingDetail = listMyTimesheet.Find(s => s.Date == date.Day);
                var timekeepingByUserAtDate = listTimekeeping.FirstOrDefault(s => s.DateAt.Day == date.Day);
                if (workingDetail == null)
                {
                    workingDetail = new WorkingHourDto
                    {
                        Date = date.Day,
                        DayName = date.DayOfWeek.ToString(),
                        IsOpenTalk = false,
                    };
                    listMyTimesheet.Add(workingDetail);
                }
                workingDetail.IsOffDaySetting = dayOffSettings.Contains(date);
                workingDetail.IsLock = isUnlock ? date < lockDateAfterUnlock : date < lockDate;
                var listAbsenceDetaiInDay = new List<AbsenceDetaiInDay>();
                absencedays.Where(s => s.DateAt.Day == date.Day).ToList().ForEach(e =>
                {
                    listAbsenceDetaiInDay.Add(
                        new AbsenceDetaiInDay
                        {
                            AbsenceType = e.DateType,
                            Hour = e.Hour,
                            AbsenceTime = e.AbsenceTime,
                            Type = e.Type,
                        }
                    );
                });
                workingDetail.ListAbsenceDetaiInDay = listAbsenceDetaiInDay;
                workingDetail.IsNoCheckIn = timekeepingByUserAtDate == default ? false : true;
                workingDetail.CheckIn = timekeepingByUserAtDate?.CheckIn;
                workingDetail.CheckOut = timekeepingByUserAtDate?.CheckOut;
                workingDetail.IsOffDaySetting = dayOffSettings.Contains(date);
            }

            int totalOpenTalk = listMyTimesheet
                                .Sum(s => s.TotalOpenTalkPerDay);
            var result = new GetNormalWorkingHourByUserLoginDto() { };
            result.ListWorkingHour = listMyTimesheet.OrderBy(x => x.Date).ToList();
            result.TotalWorkingHour = (double)listMyTimesheet
                                    .Sum(s => s.WorkingHour);
            result.TotalOpenTalk = totalOpenTalk;

            return result;
        }
    }
}