using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Ncc;
using Ncc.IoC;
using Ncc.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using Branch = Timesheet.Entities.Branch;

namespace Timesheet.APIs.Reports
{
    [AbpAuthorize]
    public class OfficeWorkingReportAppService : AppServiceBase
    {
        private readonly MezonService _mezonService;

        public OfficeWorkingReportAppService(IWorkScope workScope, MezonService mezonService) : base(workScope)
        {
            _mezonService = mezonService;
        }

        public class OfficeWorkingTopDto
        {
            public long UserId { get; set; }
            public string UserName { get; set; }
            public string OfficeName { get; set; }
            public string OfficeCode { get; set; }
            public int TotalMinutesLW { get; set; }
            public double TotalHoursLW => Math.Round(TotalMinutesLW / 60.0, 2);
        }

        public class OfficeWorkingTopLWLMDto
        {
            public long UserId { get; set; }
            public string UserName { get; set; }
            public string OfficeName { get; set; }
            public string OfficeCode { get; set; }
            public int TotalAllLW { get; set; }
            public int OfficeLW { get; set; }
            public int WfhLW { get; set; }
            public int TotalAllLM { get; set; }
            public int OfficeLM { get; set; }
            public int WfhLM { get; set; }
            public double TotalAllLWHours => Math.Round(TotalAllLW / 60.0, 2);
            public double OfficeLWHours => Math.Round(OfficeLW / 60.0, 2);
            public double WfhLWHours => Math.Round(WfhLW / 60.0, 2);
            public double TotalAllLMHours => Math.Round(TotalAllLM / 60.0, 2);
            public double OfficeLMHours => Math.Round(OfficeLM / 60.0, 2);
            public double WfhLMHours => Math.Round(WfhLM / 60.0, 2);
        }

        public struct RemoteFlags
        {
            public bool Morning { get; set; }
            public bool Afternoon { get; set; }

            public RemoteFlags(bool morning, bool afternoon)
            {
                Morning = morning;
                Afternoon = afternoon;
            }
        }

        private static (DateTime start, DateTime end) GetLastWeekRange(DateTime reportDate)
        {
            var thisWeekStart = DateTimeUtils.FirstDayOfWeek(reportDate);
            var lwStart = thisWeekStart.AddDays(-7).Date;
            var lwEnd = thisWeekStart.AddDays(-1).Date;
            return (lwStart, lwEnd);
        }

        private static bool TryParseMinutes(string input, out int minutes)
        {
            minutes = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            // Try standard formats
            if (TimeSpan.TryParseExact(input, new[] { @"hh\:mm", @"h\:mm", @"hh\:mm\:ss", @"h\:mm\:ss" }, CultureInfo.InvariantCulture, out var ts))
            {
                minutes = (int)ts.TotalMinutes;
                return true;
            }
            // Try flexible DateTime parse (e.g., HH:mm:ss.fffffff)
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                minutes = dt.Hour * 60 + dt.Minute;
                return true;
            }
            // Try cut first 5 chars (HH:mm)
            if (input.Length >= 5 && input[2] == ':')
            {
                if (int.TryParse(input.Substring(0, 2), out var h) && int.TryParse(input.Substring(3, 2), out var m))
                {
                    minutes = h * 60 + m;
                    return true;
                }
            }
            return false;
        }

        private static int ComputeWorkingMinutes(string checkIn, string checkOut)
        {
            if (!TryParseMinutes(checkIn, out var ciMin) || !TryParseMinutes(checkOut, out var coMin))
            {
                return 0;
            }
            if (coMin <= ciMin) return 0;

            var total = coMin - ciMin;
            // Subtract lunch break 60 minutes if crosses 12:00-13:00 window
            var noonStartMin = 12 * 60;
            var noonEndMin = 13 * 60;
            if (ciMin < noonStartMin && coMin > noonEndMin)
            {
                total -= 60;
            }
            return Math.Max(total, 0);
        }

        private static (int morning, int afternoon) ComputeSplitWorkingMinutes(string checkIn, string checkOut)
        {
            if (!TryParseMinutes(checkIn, out var ciMin) || !TryParseMinutes(checkOut, out var coMin))
            {
                return (0, 0);
            }
            if (coMin <= ciMin) return (0, 0);
            int noonStart = 12 * 60, noonEnd = 13 * 60;
            int morning = 0, afternoon = 0;
            if (ciMin < noonStart)
            {
                morning = Math.Max(0, Math.Min(coMin, noonStart) - ciMin);
            }
            if (coMin > noonEnd)
            {
                afternoon = Math.Max(0, coMin - Math.Max(ciMin, noonEnd));
            }
            return (morning, afternoon);
        }

        [AbpAuthorize]
        [HttpGet, HttpPost]
        public async Task<List<OfficeWorkingTopDto>> GetTopByOffice(
            [FromQuery] long officeId,
            [FromQuery] int limit = 20,
            [FromQuery] DateTime? reportDate = null,
            [FromQuery] long? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var now = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
            DateTime rangeStart;
            DateTime rangeEnd;
            if (startDate.HasValue && endDate.HasValue)
            {
                rangeStart = startDate.Value.Date;
                rangeEnd = endDate.Value.Date;
            }
            else
            {
                var lw = GetLastWeekRange(now);
                rangeStart = lw.start;
                rangeEnd = lw.end;
            }

            // Lấy danh sách user thuộc officeId
            var users = await (from u in WorkScope.GetAll<Ncc.Authorization.Users.User>()
                               join b in WorkScope.GetAll<Branch>() on u.BranchId equals b.Id into bj
                               from b in bj.DefaultIfEmpty()
                               where u.IsActive && u.BranchId == officeId
                               select new
                               {
                                   u.Id,
                                   u.Name,
                                   u.EmailAddress,
                                   OfficeName = b != null ? b.Name : string.Empty,
                                   OfficeCode = b != null ? b.Code : string.Empty
                               }).AsNoTracking().ToListAsync();

            var mapUserId = users.ToDictionary(x => x.Id, x => x);
            var mapEmail = users
                .Where(x => !string.IsNullOrEmpty(x.EmailAddress))
                .GroupBy(x => x.EmailAddress.Trim().ToLower())
                .ToDictionary(g => g.Key, g => g.First());

            var tkList = await WorkScope.GetAll<Timekeeping>()
                .Where(t => t.DateAt >= rangeStart && t.DateAt <= rangeEnd)
                .AsNoTracking()
                .ToListAsync();

            var aggMap = new Dictionary<long, (string name, string officeName, string officeCode, int minutes)>();

            foreach (var t in tkList)
            {
                long? uId = t.UserId;
                var matched = false;
                dynamic uinfo = null;
                if (uId.HasValue && mapUserId.TryGetValue(uId.Value, out var infoById))
                {
                    matched = true;
                    uinfo = infoById;
                }
                else if (!string.IsNullOrWhiteSpace(t.UserEmail))
                {
                    var key = t.UserEmail.Trim().ToLower();
                    if (mapEmail.TryGetValue(key, out var infoByEmail))
                    {
                        matched = true;
                        uId = infoByEmail.Id;
                        uinfo = infoByEmail;
                    }
                }

                if (!matched || !uId.HasValue)
                {
                    continue; // ngoài office hoặc không map được user
                }
                if (userId.HasValue && uId.Value != userId.Value)
                {
                    continue; // lọc 1 user cụ thể
                }

                var minutes = ComputeWorkingMinutes(t.CheckIn, t.CheckOut);
                if (minutes <= 0) continue;

                if (!aggMap.ContainsKey(uId.Value))
                {
                    aggMap[uId.Value] = (uinfo.Name, uinfo.OfficeName, uinfo.OfficeCode, 0);
                }
                var current = aggMap[uId.Value];
                current.minutes += minutes;
                aggMap[uId.Value] = current;
            }

            var result = aggMap
                .Select(kv => new OfficeWorkingTopDto
                {
                    UserId = kv.Key,
                    UserName = kv.Value.name,
                    OfficeName = kv.Value.officeName,
                    OfficeCode = kv.Value.officeCode,
                    TotalMinutesLW = kv.Value.minutes
                })
                .OrderByDescending(x => x.TotalMinutesLW); // Sắp xếp từ lớn xuống nhỏ

            // Nếu limit = int.MaxValue thì lấy tất cả, không cần Take()
            if (limit == int.MaxValue)
            {
                return result.ToList();
            }
            else
            {
                return result.Take(limit).ToList();
            }
        }

        [AbpAuthorize]
        [HttpGet, HttpPost]
        public async Task<string> NotifyTopByOffice(
            [FromQuery] long? officeId = null,
            [FromQuery] int limit = 20,
            [FromQuery] DateTime? reportDate = null,
            [FromQuery] string mezonUrl = null,
            [FromQuery] long? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool showAll = false,
            [FromQuery] bool allOffices = false)
        {
            // Nếu showAll = true, set limit = int.MaxValue để lấy tất cả
            var actualLimit = showAll ? int.MaxValue : limit;

            var sb = new StringBuilder();

            // Nếu allOffices = true, hiển thị tất cả văn phòng
            if (allOffices)
            {
                var allOfficesReport = await GetAllOfficesReport(actualLimit, reportDate, userId, startDate, endDate);
                
                // Send notification to Mezon
                var notificationUrl = mezonUrl;
                if (string.IsNullOrWhiteSpace(notificationUrl))
                {
                    notificationUrl = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);
                }

                if (!string.IsNullOrWhiteSpace(notificationUrl))
                {
                    _mezonService.NotifyToChannel(notificationUrl, allOfficesReport);
                }
                
                return allOfficesReport;
            }

            // Nếu không có officeId, báo lỗi
            if (!officeId.HasValue)
            {
                return "❌ Vui lòng cung cấp officeId hoặc sử dụng allOffices=true";
            }

            var items = await GetTopByOffice(officeId.Value, actualLimit, reportDate, userId, startDate, endDate);

            var branchInfo = await WorkScope.GetAll<Branch>()
                .Where(b => b.Id == officeId.Value)
                .Select(b => new { b.Name, b.Code, b.DisplayName })
                .FirstOrDefaultAsync();

            var vpName = branchInfo?.Code ?? branchInfo?.Name ?? $"Office#{officeId.Value}";
            var now = (reportDate?.Date ?? DateTimeUtils.GetNow().Date);
            DateTime s, e;
            if (startDate.HasValue && endDate.HasValue)
            {
                s = startDate.Value.Date;
                e = endDate.Value.Date;
            }
            else
            {
                var lw = GetLastWeekRange(now);
                s = lw.start; e = lw.end;
            }
            // Nếu không truyền start/end → hiển thị bảng cả LW & LM
            if (!startDate.HasValue || !endDate.HasValue)
            {
                // Sử dụng reportDate nếu có, nếu không thì dùng thời gian hiện tại
                var baseTime = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
                var (lwStart, lwEnd) = GetLastWeekRange(baseTime);
                var lmStart = DateTimeUtils.FirstDayOfMonth(baseTime.AddMonths(-1));
                var lmEnd = DateTimeUtils.LastDayOfMonth(baseTime.AddMonths(-1));
                var combined = await GetTopByOfficeLWLM(officeId.Value, actualLimit, reportDate, userId);

                
                sb.AppendLine($"**{(actualLimit == int.MaxValue ? "Tất cả" : $"Top {actualLimit}")} – VP {vpName}**");
                sb.AppendLine($"**LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})**");
                sb.AppendLine();

                if (combined.Count == 0)
                {
                    sb.AppendLine("Không có dữ liệu.");
                }
                else
                {
                    
                    sb.AppendLine("```");
                    sb.AppendLine("┌─────┬───────────────────────────────┬──────────┬──────────┬──────────┬──────────┬──────────┬──────────┐");
                    sb.AppendLine("│ STT │           Họ và Tên           │ LW Total │ LW Office│  LW WFH  │ LM Total │ LM Office│  LM WFH  │");
                    sb.AppendLine("├─────┼───────────────────────────────┼──────────┼──────────┼──────────┼──────────┼──────────┼──────────┤");
                    
                    int idx = 1;
                    
                    var itemsToShow = showAll ? combined : combined.Take(actualLimit);
                    
                    foreach (var item in itemsToShow)
                    {
                        var name = item.UserName ?? "N/A";
                        
                        if (name.Length > 29) name = name.Substring(0, 26) + "...";
                        
                        
                        sb.AppendLine($"│{idx,4} │ {name,-29}   {item.TotalAllLWHours,15:F1}     {item.OfficeLWHours,15:F1}   {item.WfhLWHours,15:F1}  {item.TotalAllLMHours,15:F1}     {item.OfficeLMHours,15:F1}   {item.WfhLMHours,15:F1} │");
                        idx++;
                    }
                    
                    sb.AppendLine("└─────┴───────────────────────────────┴──────────┴──────────┴──────────┴──────────┴──────────┴──────────┘");
                    sb.AppendLine($"Hiển thị: {itemsToShow.Count()}/{combined.Count} nhân viên");
                    sb.AppendLine("```");
                    sb.AppendLine();
                    sb.AppendLine($"📈 **Tổng số nhân viên:** {combined.Count}");
                    sb.AppendLine($"📅 **Thời gian:** LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
                }
            }
            else
            {
                
                if (userId.HasValue)
                {
                    sb.AppendLine($"Báo cáo theo TotalTime – VP {vpName} – UserId {userId.Value} ({s:dd/MM}–{e:dd/MM})");
                }
                else
                {
                    sb.AppendLine($"{(showAll ? "Tất cả" : $"Top {actualLimit}")} theo TotalTime – VP {vpName} ({s:dd/MM}–{e:dd/MM})");
                }
                if (items.Count == 0)
                {
                    sb.AppendLine("Không có dữ liệu.");
                }
                else
                {
                    int idx = 1;
                    foreach (var i in items)
                    {
                        var officeShown = string.IsNullOrWhiteSpace(i.OfficeCode) ? i.OfficeName : i.OfficeCode;
                        sb.AppendLine($"{idx}) {i.UserName} | {officeShown} | {i.TotalHoursLW}h");
                        idx++;
                    }
                }
            }

            var url = mezonUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                // fallback to app setting if provided
                url = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                _mezonService.NotifyToChannel(url, sb.ToString());
            }

            return sb.ToString();
        }

        [AbpAuthorize]
        [HttpGet, HttpPost]
        public async Task<List<OfficeWorkingTopLWLMDto>> GetTopByOfficeLWLM(
            [FromQuery] long officeId,
            [FromQuery] int limit = int.MaxValue,
            [FromQuery] DateTime? reportDate = null,
            [FromQuery] long? userId = null)
        {
            var now = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
            var (lwStart, lwEnd) = GetLastWeekRange(now);
            var lmStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));

            // Users in office
            var users = await (from u in WorkScope.GetAll<Ncc.Authorization.Users.User>()
                               join b in WorkScope.GetAll<Branch>() on u.BranchId equals b.Id into bj
                               from b in bj.DefaultIfEmpty()
                               where u.IsActive && u.BranchId == officeId
                               select new
                               {
                                   u.Id,
                                   Name = !string.IsNullOrEmpty(u.Surname) && !string.IsNullOrEmpty(u.Name) 
                                          ? $"{u.Surname} {u.Name}" 
                                          : (!string.IsNullOrEmpty(u.Name) ? u.Name : u.Surname),
                                   u.EmailAddress,
                                   OfficeName = b != null ? b.Name : string.Empty,
                                   OfficeCode = b != null ? b.Code : string.Empty
                               }).AsNoTracking().ToListAsync();

            var mapUserId = users.ToDictionary(x => x.Id, x => x);
            var mapEmail = users.Where(x => !string.IsNullOrEmpty(x.EmailAddress))
                                .GroupBy(x => x.EmailAddress.Trim().ToLower())
                                .ToDictionary(g => g.Key, g => g.First());

            // Timekeepings in both ranges
            var minStart = lwStart < lmStart ? lwStart : lmStart;
            var maxEnd = lwEnd > lmEnd ? lwEnd : lmEnd;

            // Debug logging
            Logger.Error($"DEBUG: LW Range: {lwStart:yyyy-MM-dd} to {lwEnd:yyyy-MM-dd}");
            Logger.Error($"DEBUG: LM Range: {lmStart:yyyy-MM-dd} to {lmEnd:yyyy-MM-dd}");
            Logger.Error($"DEBUG: Query Range: {minStart:yyyy-MM-dd} to {maxEnd:yyyy-MM-dd}");
            Logger.Error($"DEBUG: OfficeId: {officeId}, UserId: {userId}");

            var tkList = await WorkScope.GetAll<Timekeeping>()
                .Where(t => t.DateAt >= minStart && t.DateAt <= maxEnd)
                .AsNoTracking()
                .ToListAsync();

            Logger.Error($"DEBUG: Found {tkList.Count} timekeeping records");
            Logger.Error($"DEBUG: Found {users.Count} users in office");

            // Debug remote data
            var allRemoteRequests = await WorkScope.GetAll<AbsenceDayDetail>()
                .Include(d => d.Request)
                .Where(d => d.DateAt >= minStart && d.DateAt <= maxEnd)
                .AsNoTracking()
                .ToListAsync();
            
            Logger.Error($"DEBUG: Total AbsenceDayDetail records in range: {allRemoteRequests.Count}");
            Logger.Error($"DEBUG: Remote type requests: {allRemoteRequests.Count(x => x.Request.Type == RequestType.Remote)}");
            Logger.Error($"DEBUG: Approved remote requests: {allRemoteRequests.Count(x => x.Request.Type == RequestType.Remote && x.Request.Status == RequestStatus.Approved)}");
            
            // Log some sample data
            foreach (var sample in allRemoteRequests.Take(5))
            {
                Logger.Error($"DEBUG Sample: UserId={sample.Request.UserId}, Date={sample.DateAt:yyyy-MM-dd}, Type={sample.Request.Type}, Status={sample.Request.Status}, DateType={sample.DateType}");
            }

            // Remote map from AbsenceDayDetail (Request.Type == Remote, Approved)
            var remoteDetails = await WorkScope.GetAll<AbsenceDayDetail>()
                .Include(d => d.Request)
                .Where(d => d.DateAt >= minStart && d.DateAt <= maxEnd)
                .Where(d => d.Request.Status == RequestStatus.Approved)
                .Where(d => d.Request.Type == RequestType.Remote)
                .AsNoTracking()
                .ToListAsync();

            var remoteMap = new Dictionary<(long userId, DateTime date), RemoteFlags>();
            foreach (var d in remoteDetails)
            {
                var key = (d.Request.UserId, d.DateAt.Date);
                remoteMap.TryGetValue(key, out var val);

                switch (d.DateType)
                {
                    case DayType.Fullday:
                        val = new RemoteFlags(true, true);
                        break;
                    case DayType.Morning:
                        val = new RemoteFlags(true, val.Afternoon);
                        break;
                    case DayType.Afternoon:
                        val = new RemoteFlags(val.Morning, true);
                        break;
                }
                remoteMap[key] = val;
            }

            var agg = new Dictionary<long, OfficeWorkingTopLWLMDto>();

            foreach (var t in tkList)
            {
                long? uid = t.UserId;
                dynamic uinfo = null;
                if (uid.HasValue && mapUserId.TryGetValue(uid.Value, out var i1))
                {
                    uinfo = i1;
                }
                else if (!uid.HasValue && !string.IsNullOrWhiteSpace(t.UserEmail))
                {
                    var k = t.UserEmail.Trim().ToLower();
                    if (mapEmail.TryGetValue(k, out var i2))
                    {
                        uid = i2.Id;
                        uinfo = i2;
                    }
                }
                if (uinfo == null || !uid.HasValue) continue;
                if (userId.HasValue && uid.Value != userId.Value) continue;

                // Parse tracker time to get actual working minutes
                var trackerMinutes = 0;
                if (!string.IsNullOrWhiteSpace(t.TrackerTime) && TimeSpan.TryParse(t.TrackerTime, out var trackerSpan))
                {
                    trackerMinutes = (int)trackerSpan.TotalMinutes;
                }
                
                var (mMin, aMin) = ComputeSplitWorkingMinutes(t.CheckIn, t.CheckOut);
                if (mMin + aMin + trackerMinutes <= 0) continue;

                if (!agg.ContainsKey(uid.Value))
                {
                    agg[uid.Value] = new OfficeWorkingTopLWLMDto
                    {
                        UserId = uid.Value,
                        UserName = uinfo.Name,
                        OfficeName = uinfo.OfficeName,
                        OfficeCode = uinfo.OfficeCode
                    };
                }
                var row = agg[uid.Value];

                var remoteFlags = remoteMap.TryGetValue((uid.Value, t.DateAt.Date), out var rf)
                    ? rf
                    : new RemoteFlags(false, false);

                bool inLW = t.DateAt.Date >= lwStart && t.DateAt.Date <= lwEnd;
                bool inLM = t.DateAt.Date >= lmStart && t.DateAt.Date <= lmEnd;

                if (inLW)
                {
                    if (remoteFlags.Morning || remoteFlags.Afternoon)
                    {
                        // WFH: use tracker time as WFH time
                        row.TotalAllLW += trackerMinutes;
                        row.WfhLW += trackerMinutes;
                        // Office time = 0 when WFH
                    }
                    else
                    {
                        // Office: use check-in/out (with break logic) + tracker time
                        int officeTime = mMin + aMin; // Already calculated with break exclusion
                        row.TotalAllLW += officeTime + trackerMinutes;
                        row.OfficeLW += officeTime + trackerMinutes;
                    }
                }
                if (inLM)
                {
                    if (remoteFlags.Morning || remoteFlags.Afternoon)
                    {
                        // WFH: use tracker time as WFH time
                        row.TotalAllLM += trackerMinutes;
                        row.WfhLM += trackerMinutes;
                        // Office time = 0 when WFH
                    }
                    else
                    {
                        // Office: use check-in/out (with break logic) + tracker time
                        int officeTime = mMin + aMin; // Already calculated with break exclusion
                        row.TotalAllLM += officeTime + trackerMinutes;
                        row.OfficeLM += officeTime + trackerMinutes;
                    }
                }
            }

            Logger.Error($"DEBUG: Final aggregated users: {agg.Count}");

            
            foreach (var user in users)
            {
                if (!agg.ContainsKey(user.Id))
                {
                    agg[user.Id] = new OfficeWorkingTopLWLMDto
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        OfficeName = user.OfficeName,
                        OfficeCode = user.OfficeCode,
                        TotalAllLW = 0,
                        OfficeLW = 0,
                        WfhLW = 0,
                        TotalAllLM = 0,
                        OfficeLM = 0,
                        WfhLM = 0
                    };
                }
            }

            var rs = agg.Values
                .OrderByDescending(x => x.TotalAllLW) // Sắp xếp theo tổng giờ LW từ lớn xuống nhỏ
                .ThenByDescending(x => x.TotalAllLM); // Nếu LW bằng nhau thì sắp xếp theo LM

            var result = limit == int.MaxValue ? rs.ToList() : rs.Take(limit).ToList();
            Logger.Error($"DEBUG: Returning {result.Count} results");

            return result;
        }

        private async Task<string> GetAllOfficesReport(int actualLimit, DateTime? reportDate, long? userId, DateTime? startDate, DateTime? endDate)
        {
            var sb = new StringBuilder();
            
            // Lấy tất cả văn phòng
            var offices = await WorkScope.GetAll<Branch>()
                .Where(b => b.Id > 0)
                .Select(b => new { b.Id, b.Name, b.Code, b.DisplayName })
                .OrderBy(b => b.Code ?? b.Name)
                .AsNoTracking()
                .ToListAsync();

            if (offices.Count == 0)
            {
                return "❌ Không tìm thấy văn phòng nào";
            }

            var baseTime = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
            var (lwStart, lwEnd) = GetLastWeekRange(baseTime);
            var lmStart = DateTimeUtils.FirstDayOfMonth(baseTime.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(baseTime.AddMonths(-1));

            sb.AppendLine($"📊 **BÁO CÁO TẤT CẢ VĂN PHÒNG**");
            sb.AppendLine($"**LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})**");
            sb.AppendLine($"**Tổng số văn phòng: {offices.Count}**");
            sb.AppendLine();

            foreach (var office in offices)
            {
                try
                {
                    var combined = await GetTopByOfficeLWLM(office.Id, int.MaxValue, reportDate, userId);
                    var officeName = office.Code ?? office.Name ?? $"Office#{office.Id}";
                    
                    sb.AppendLine($"🏢 **VP {officeName}** ({combined.Count} nhân viên)");
                    
                    if (combined.Count == 0)
                    {
                        sb.AppendLine("   _Không có dữ liệu_");
                    }
                    else
                    {
                        sb.AppendLine("```");
                        sb.AppendLine("┌─────┬───────────────────────────────┬──────────┬──────────┬──────────┬──────────┬──────────┬──────────┐");
                        sb.AppendLine("│ STT │           Họ và Tên           │ LW Total │ LW Office│  LW WFH  │ LM Total │ LM Office│  LM WFH  │");
                        sb.AppendLine("├─────┼───────────────────────────────┼──────────┼──────────┼──────────┼──────────┼──────────┼──────────┤");
                        
                        var topItems = combined.Take(actualLimit); // Hiển thị theo limit cho mỗi office
                        int idx = 1;
                        
                        foreach (var item in topItems)
                        {
                            var name = item.UserName ?? "N/A";
                            if (name.Length > 29) name = name.Substring(0, 26) + "...";
                            
                            sb.AppendLine($"│{idx,4} │ {name,-29}   {item.TotalAllLWHours,15:F1}   {item.OfficeLWHours,15:F1}   {item.WfhLWHours,15:F1}   {item.TotalAllLMHours,15:F1}   {item.OfficeLMHours,15:F1}  │{item.WfhLMHours,15:F1} │");
                            idx++;
                        }
                        
                        sb.AppendLine("└─────┴───────────────────────────────┴──────────┴──────────┴──────────┴──────────┴──────────┴──────────┘");
                        
                        if (combined.Count > actualLimit)
                        {
                            sb.AppendLine($"... và {combined.Count - actualLimit} người khác");
                        }
                        
                        sb.AppendLine("```");
                    }
                    
                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"🏢 **VP {office.Code ?? office.Name}** - ❌ Lỗi: {ex.Message}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine($"📈 **Tổng cộng: {offices.Count} văn phòng**");
            sb.AppendLine($"📅 **Thời gian: LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})**");
            
            return sb.ToString();
        }
    }
}