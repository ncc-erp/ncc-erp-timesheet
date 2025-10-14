using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Uow;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Timesheet.APIs.Reports.Dto;
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
        private class UserLite
        {
            public long Id
            {
                get;
                set;
            }
            public string Name
            {
                get;
                set;
            }
            public string Email
            {
                get;
                set;
            }
            public string OfficeName
            {
                get;
                set;
            }
            public string OfficeCode
            {
                get;
                set;
            }
        }
        public struct RemoteFlags
        {
            public bool Morning
            {
                get;
                set;
            }
            public bool Afternoon
            {
                get;
                set;
            }

            public RemoteFlags(bool morning, bool afternoon)
            {
                Morning = morning;
                Afternoon = afternoon;
            }
        }

        private static void AddBold(List<object> mkList, int start, int end)
        {
            if (start >= 0 && end > start)
            {
                mkList.Add(new
                {
                    type = "b",
                    s = start,
                    e = end
                });
            }
        }

        private static void AddBoldForLabel(List<object> mkList, string messageText, string label)
        {
            var pos = messageText.IndexOf(label);
            if (pos >= 0)
            {
                AddBold(mkList, pos, pos + label.Length);
            }
        }

        private static void AddBoldForAllOccurrences(List<object> mkList, string messageText, string token)
        {
            int current = 0;
            while (true)
            {
                int idx = messageText.IndexOf(token, current);
                if (idx == -1) break;
                AddBold(mkList, idx, idx + token.Length);
                current = idx + token.Length;
            }
        }

        private static void AddBoldForUsernames(List<object> mkList, string messageText)
        {
            var lines = messageText.Split('\n');
            int currentPos = 0;

            foreach (var line in lines)
            {

                if (Regex.IsMatch(line.Trim(), @"^\d+\.\s"))
                {
                    var dotIndex = line.IndexOf('.');
                    if (dotIndex >= 0)
                    {
                        var startPos = currentPos + dotIndex + 2;
                        var endPos = line.Length;
                        if (startPos < currentPos + line.Length)
                        {
                            AddBold(mkList, startPos, currentPos + endPos);
                        }
                    }
                }
                currentPos += line.Length + 1;
            }
        }

        private static void AddBoldForOfficeTitles(List<object> mkList, string messageText)
        {
            int current = 0;
            while (true)
            {
                int start = messageText.IndexOf("🏢 **VP ", current);
                if (start == -1) break;
                int end = messageText.IndexOf("**", start + 8);
                if (end == -1) break;
                AddBold(mkList, start, end + 2);
                current = end + 2;
            }
        }

        private static (DateTime start, DateTime end) GetLastWeekRange(DateTime reportDate)
        {
            var thisWeekStart = DateTimeUtils.FirstDayOfWeek(reportDate);
            var lwStart = thisWeekStart.AddDays(-7).Date;
            var lwEnd = thisWeekStart.AddDays(-1).Date.AddDays(1).AddTicks(-1);
            return (lwStart, lwEnd);
        }

        private static bool TryParseMinutes(string input, out int minutes)
        {
            minutes = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (TimeSpan.TryParseExact(input, new[] {
            @"hh\:mm",
              @"h\:mm"
          },
                CultureInfo.InvariantCulture, out
                var ts))
            {
                minutes = (int)ts.TotalMinutes;
                return true;
            }
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out
                var dt))
            {
                minutes = dt.Hour * 60 + dt.Minute;
                return true;
            }
            if (input.Length >= 5 && input[2] == ':')
            {
                if (int.TryParse(input.Substring(0, 2), out
                    var h) && int.TryParse(input.Substring(3, 2), out
                    var m))
                {
                    minutes = h * 60 + m;
                    return true;
                }
            }
            return false;
        }

        private static int ComputeWorkingMinutes(string checkIn, string checkOut)
        {
            if (!TryParseMinutes(checkIn, out
                var ciMin) || !TryParseMinutes(checkOut, out
                var coMin))
            {
                return 0;
            }
            if (coMin <= ciMin) return 0;

            var total = coMin - ciMin;
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
            if (!TryParseMinutes(checkIn, out
                var ciMin) || !TryParseMinutes(checkOut, out
                var coMin))
            {
                return (0, 0);
            }
            if (coMin <= ciMin) return (0, 0);
            int noonStart = 12 * 60,
              noonEnd = 13 * 60;
            int morning = 0,
              afternoon = 0;
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

        private async Task<List<OfficeWorkingTopLWLMDto>> ListTopOfficeWorkingTimeInternal(
          long officeId, int limit = 20, DateTime? reportDate = null, long? userId = null, DateTime? startDate = null, DateTime? endDate = null)
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

            var users = await (from u in WorkScope.GetAll<Ncc.Authorization.Users.User>()
                               join b in WorkScope.GetAll<Branch>() on u.BranchId equals b.Id into bj
                               from b in bj.DefaultIfEmpty()
                               where u.IsActive && u.BranchId == officeId
                               select new UserLite
                               {
                                   Id = u.Id,
                                   Name = !string.IsNullOrEmpty(u.UserName) ? u.UserName : (!string.IsNullOrEmpty(u.Name) ? u.Name : "Unknown"),
                                   Email = u.EmailAddress,
                                   OfficeName = b != null ? b.Name : string.Empty,
                                   OfficeCode = b != null ? b.Code : string.Empty
                               }).AsNoTracking().ToListAsync();

            var mapUserId = users.ToDictionary(x => x.Id, x => x);
            var mapEmail = users.Where(x => !string.IsNullOrEmpty(x.Email)).GroupBy(x => x.Email.Trim().ToLower()).ToDictionary(g => g.Key, g => g.First());

            var tkList = await WorkScope.GetAll<Timekeeping>().Where(t => t.DateAt >= rangeStart && t.DateAt <= rangeEnd).Select(t => new {
                t.UserId,
                t.UserEmail,
                t.DateAt,
                t.CheckIn,
                t.CheckOut
            }).AsNoTracking().ToListAsync();

            var aggMap = new Dictionary<long,
              (string name, string officeName, string officeCode, int minutes)>();

            foreach (var t in tkList)
            {
                long? userIdResolved = t.UserId;
                UserLite userInfo = null;
                if (userIdResolved.HasValue && mapUserId.TryGetValue(userIdResolved.Value, out
                    var infoById))
                {
                    userInfo = infoById;
                }
                else if (!string.IsNullOrWhiteSpace(t.UserEmail))
                {
                    var key = t.UserEmail.Trim().ToLower();
                    if (mapEmail.TryGetValue(key, out
                        var infoByEmail))
                    {
                        userIdResolved = infoByEmail.Id;
                        userInfo = infoByEmail;
                    }
                }

                if (userInfo == null || !userIdResolved.HasValue)
                {
                    continue;
                }
                if (userId.HasValue && userIdResolved.Value != userId.Value)
                {
                    continue;
                }

                var minutes = ComputeWorkingMinutes(t.CheckIn, t.CheckOut);
                if (minutes <= 0) continue;

                if (!aggMap.TryGetValue(userIdResolved.Value, out
                    var current))
                {
                    current = (userInfo.Name, userInfo.OfficeName, userInfo.OfficeCode, 0);
                }
                current.minutes += minutes;
                aggMap[userIdResolved.Value] = current;
            }

            var result = aggMap.Select(kv => new OfficeWorkingTopLWLMDto
            {
                UserId = kv.Key,
                UserName = kv.Value.name,
                OfficeName = kv.Value.officeName,
                OfficeCode = kv.Value.officeCode,
                TotalAllLW = kv.Value.minutes / 60.0,
                OfficeLW = kv.Value.minutes / 60.0
            }).OrderByDescending(x => x.TotalAllLW);

            if (limit == int.MaxValue)
            {
                return result.ToList();
            }
            else
            {
                return result.Take(limit).ToList();
            }
        }
        private async Task<string> SendTopOfficeUsersNotificationInternal(
          long? officeId = null, int limit = 20, DateTime? reportDate = null, string mezonUrl = null, long? userId = null, DateTime? startDate = null, DateTime? endDate = null, bool showAll = false, bool allOffices = false)
        {

            var actualLimit = showAll ? int.MaxValue : limit;
            var sb = new StringBuilder();
            if (allOffices)
            {
                var allOfficesReport = await GetAllOfficesReport(actualLimit, reportDate, userId, startDate, endDate);
                var notificationUrl = mezonUrl;
                if (string.IsNullOrWhiteSpace(notificationUrl))
                {
                    notificationUrl = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);
                }

                if (!string.IsNullOrWhiteSpace(notificationUrl))
                {
                    var messageText = allOfficesReport;
                    var mkList = new List<object>();
                    AddBoldForLabel(mkList, messageText, "📊 **BÁO CÁO TẤT CẢ VĂN PHÒNG**");
                    AddBoldForLabel(mkList, messageText, "📈 **Tổng số văn phòng:**");
                    AddBoldForLabel(mkList, messageText, "📅 **Thời gian:**");
                    AddBoldForOfficeTitles(mkList, messageText);
                    AddBoldForUsernames(mkList, messageText);
                    AddBoldForAllOccurrences(mkList, messageText, "LW:");
                    AddBoldForAllOccurrences(mkList, messageText, "LM:");

                    _mezonService.Post(notificationUrl, new
                    {
                        type = "hook",
                        message = new
                        {
                            t = messageText,
                            mk = mkList
                        }
                    });
                }

                return allOfficesReport;
            }
            if (!officeId.HasValue)
            {
                return "❌ Vui lòng cung cấp officeId hoặc sử dụng allOffices=true";
            }

            var items = await ListTopOfficeWorkingTimeInternal(officeId.Value, actualLimit, reportDate, userId, startDate, endDate);

            var branchInfo = await WorkScope.GetAll<Branch>().Where(b => b.Id == officeId.Value).Select(b => new {
                b.Name,
                b.Code,
                b.DisplayName
            }).FirstOrDefaultAsync();

            var vpName = branchInfo?.Code ?? branchInfo?.Name ?? "Office#{officeId.Value}";
            var now = (reportDate?.Date ?? DateTimeUtils.GetNow().Date);
            DateTime s,
            e;
            if (startDate.HasValue && endDate.HasValue)
            {
                s = startDate.Value.Date;
                e = endDate.Value.Date;
            }
            else
            {
                var lw = GetLastWeekRange(now);
                s = lw.start;
                e = lw.end;
            }
            if (!startDate.HasValue || !endDate.HasValue)
            {

                var baseTime = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
                var (lwStart, lwEnd) = GetLastWeekRange(baseTime);
                var lmStart = DateTimeUtils.FirstDayOfMonth(baseTime.AddMonths(-1));
                var lmEnd = DateTimeUtils.LastDayOfMonth(baseTime.AddMonths(-1));
                var combined = await ListTopOfficeWorkingTimeLWLMInternal(officeId.Value, actualLimit, reportDate, userId);
                sb.AppendLine($"**{(actualLimit == int.MaxValue ? "Tất cả " : $"Top {actualLimit}")} – VP {vpName}**");
                sb.AppendLine($"**LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})**");
                sb.AppendLine();

        if (combined.Count == 0) {
          sb.AppendLine("Không có dữ liệu.");
        } else {
          int idx = 1;
          var itemsToShow = showAll ? combined : combined.Take(actualLimit);

          foreach(var item in itemsToShow) {
            var username = item.UserName ?? "N/A";
            var atIndex = username.IndexOf('@');
            if (atIndex >= 0) {
              username = username.Substring(0, atIndex);
            }

            sb.AppendLine($"{idx}. {username}");
            sb.AppendLine($"   - LW: Total {item.TotalAllLW:F1}h (Office {item.OfficeLW:F1}h, WFH {item.WfhLW:F1}h)");
            sb.AppendLine($"   - LM: Total {item.TotalAllLM:F1}h (Office {item.OfficeLM:F1}h, WFH {item.WfhLM:F1}h)");
            sb.AppendLine("   -------------------------");
            idx++;
          }

          if (combined.Count > itemsToShow.Count()) {
            sb.AppendLine($"... và {combined.Count - itemsToShow.Count()} người khác");
          }
          sb.AppendLine();
          sb.AppendLine($"📈 **Tổng số nhân viên:** {combined.Count}");
          sb.AppendLine($"📅 **Thời gian:** LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
        }
      } else {
        
        if (userId.HasValue) {
          sb.AppendLine("Báo cáo theo TotalTime – VP {vpName} – UserId {userId.Value} ({s:dd/MM}–{e:dd/MM})");
        } else {
          sb.AppendLine($"{(showAll ? "Tất cả " : $"Top {actualLimit}")} theo TotalTime – VP {vpName} ({s:dd/MM}–{e:dd/MM})");
        }
        if (items.Count == 0) {
          sb.AppendLine("Không có dữ liệu.");
        } else {
          int idx = 1;
          foreach(var i in items) {
            var officeShown = string.IsNullOrWhiteSpace(i.OfficeCode) ? i.OfficeName : i.OfficeCode;
            sb.AppendLine("{idx}) {i.UserName} | {officeShown} | {i.TotalAllLW}h");
            idx++;
          }
        }
      }

      var url = mezonUrl;
      if (string.IsNullOrWhiteSpace(url)) {
        url = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);
      }

      if (!string.IsNullOrWhiteSpace(url)) {
        var messageText = sb.ToString();
        var mkList = new List < object > ();
        AddBoldForLabel(mkList, messageText, "📊 **Top Office Working Report * *");
        
                AddBoldForLabel(mkList, messageText, "📈 **Tổng số nhân viên:**");
                AddBoldForLabel(mkList, messageText, "📅 **Thời gian:**");
                AddBoldForOfficeTitles(mkList, messageText);
                AddBoldForUsernames(mkList, messageText);
                AddBoldForAllOccurrences(mkList, messageText, "LW:");
                AddBoldForAllOccurrences(mkList, messageText, "LM:");

                _mezonService.Post(url, new
                {
                    type = "hook",
                    message = new
                    {
                        t = messageText,
                        mk = mkList
                    }
                });
            }

            return sb.ToString();
        }

        public Task<string> SendTopOfficeUsersNotification(
          long? officeId = null, int limit = 20, DateTime? reportDate = null, string mezonUrl = null, long? userId = null, DateTime? startDate = null, DateTime? endDate = null, bool showAll = false, bool allOffices = false)
        {
            return SendTopOfficeUsersNotificationInternal(officeId, limit, reportDate, mezonUrl, userId, startDate, endDate, showAll, allOffices);
        }
        private async Task<List<OfficeWorkingTopLWLMDto>> ListTopOfficeWorkingTimeLWLMInternal(
            long officeId, int limit = int.MaxValue, DateTime? reportDate = null, long? userId = null)
        {
            var now = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
            var (lwStart, lwEnd) = GetLastWeekRange(now);
            var lmStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));

            var users = await (from u in WorkScope.GetAll<Ncc.Authorization.Users.User>()
                               join b in WorkScope.GetAll<Branch>() on u.BranchId equals b.Id into bj
                               from b in bj.DefaultIfEmpty()
                               where u.IsActive && u.BranchId == officeId
                               select new UserLite
                               {
                                   Id = u.Id,
                                   Name = !string.IsNullOrEmpty(u.UserName) ? u.UserName : (!string.IsNullOrEmpty(u.Name) ? u.Name : "Unknown"),
                                   Email = u.EmailAddress,
                                   OfficeName = b != null ? b.Name : string.Empty,
                                   OfficeCode = b != null ? b.Code : string.Empty
                               }).AsNoTracking().ToListAsync();

            var mapUserId = users.ToDictionary(x => x.Id, x => x);
            var mapEmail = users.Where(x => !string.IsNullOrEmpty(x.Email)).GroupBy(x => x.Email.Trim().ToLower()).ToDictionary(g => g.Key, g => g.First());

            var minStart = lwStart < lmStart ? lwStart : lmStart;
            var maxEnd = lwEnd > lmEnd ? lwEnd : lmEnd;

            var tkList = await WorkScope.GetAll<Timekeeping>().Where(t => t.DateAt >= minStart && t.DateAt <= maxEnd).Select(t => new {
                t.UserId,
                t.UserEmail,
                t.DateAt,
                t.CheckIn,
                t.CheckOut,
                t.TrackerTime
            }).AsNoTracking().ToListAsync();
            var remoteDetails = await WorkScope.GetAll<AbsenceDayDetail>().Include(d => d.Request).Where(d => d.DateAt >= minStart && d.DateAt <= maxEnd).Where(d => d.Request.Status == RequestStatus.Approved).Where(d => d.Request.Type == RequestType.Remote).Select(d => new {
                d.DateAt,
                d.DateType,
                UserId = d.Request.UserId
            }).AsNoTracking().ToListAsync();

            var remoteMap = new Dictionary<(long userId, DateTime date),
              RemoteFlags>();
            foreach (var d in remoteDetails)
            {
                var key = (d.UserId, d.DateAt.Date);
                remoteMap.TryGetValue(key, out
                  var val);

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

            var agg = new Dictionary<long,
              OfficeWorkingTopLWLMDto>();

            foreach (var t in tkList)
            {
                long? uid = t.UserId;
                UserLite uinfo = null;
                if (uid.HasValue && mapUserId.TryGetValue(uid.Value, out
                    var i1))
                {
                    uinfo = i1;
                }
                else if (!uid.HasValue && !string.IsNullOrWhiteSpace(t.UserEmail))
                {
                    var k = t.UserEmail.Trim().ToLower();
                    if (mapEmail.TryGetValue(k, out
                        var i2))
                    {
                        uid = i2.Id;
                        uinfo = i2;
                    }
                }
                if (uinfo == null || !uid.HasValue) continue;
                if (userId.HasValue && uid.Value != userId.Value) continue;

                var trackerMinutes = 0;
                if (!string.IsNullOrWhiteSpace(t.TrackerTime) && TimeSpan.TryParse(t.TrackerTime, out
                    var trackerSpan))
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

                var remoteFlags = remoteMap.TryGetValue((uid.Value, t.DateAt.Date), out
                  var rf) ? rf : new RemoteFlags(false, false);

                bool inLW = t.DateAt.Date >= lwStart && t.DateAt.Date <= lwEnd;
                bool inLM = t.DateAt.Date >= lmStart && t.DateAt.Date <= lmEnd;

                if (inLW)
                {
                    if (remoteFlags.Morning || remoteFlags.Afternoon)
                    {
                        row.TotalAllLW += (trackerMinutes / 60.0);
                        row.WfhLW += (trackerMinutes / 60.0);
                    }
                    else
                    {
                        int officeTime = mMin + aMin;
                        var officeHours = (officeTime + trackerMinutes) / 60.0;
                        row.TotalAllLW += officeHours;
                        row.OfficeLW += officeHours;
                    }
                }
                if (inLM)
                {
                    if (remoteFlags.Morning || remoteFlags.Afternoon)
                    {
                        row.TotalAllLM += (trackerMinutes / 60.0);
                        row.WfhLM += (trackerMinutes / 60.0);
                    }
                    else
                    {
                        int officeTime = mMin + aMin;
                        var officeHoursLM = (officeTime + trackerMinutes) / 60.0;
                        row.TotalAllLM += officeHoursLM;
                        row.OfficeLM += officeHoursLM;
                    }
                }
            }

            Logger.Error("DEBUG: Final aggregated users: {agg.Count}");
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

            var rs = agg.Values.OrderByDescending(x => x.TotalAllLW).ThenByDescending(x => x.TotalAllLM);

            var result = limit == int.MaxValue ? rs.ToList() : rs.Take(limit).ToList();
            Logger.Error("DEBUG: Returning {result.Count} results");

            return result;
        }
        [AbpAuthorize]
        [HttpGet]
        public Task<List<OfficeWorkingTopLWLMDto>> GetListTopOfficeWorkingTimeLWLM(
        long officeId, int limit = int.MaxValue, DateTime? reportDate = null, long? userId = null)
        {
            return ListTopOfficeWorkingTimeLWLMInternal(officeId, limit, reportDate, userId);
        }
        private async Task<string> GetAllOfficesReport(int actualLimit, DateTime? reportDate, long? userId, DateTime? startDate, DateTime? endDate)
        {
            var sb = new StringBuilder();

            var offices = await WorkScope.GetAll<Branch>().Where(b => b.Id > 0).Select(b => new {
                b.Id,
                b.Name,
                b.Code,
                b.DisplayName
            }).OrderBy(b => b.Code ?? b.Name).AsNoTracking().ToListAsync();

            if (offices.Count == 0)
            {
                return "❌ Không tìm thấy văn phòng nào";
            }

            var baseTime = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
            var (lwStart, lwEnd) = GetLastWeekRange(baseTime);
            var lmStart = DateTimeUtils.FirstDayOfMonth(baseTime.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(baseTime.AddMonths(-1));

            sb.AppendLine("📊 **BÁO CÁO TẤT CẢ VĂN PHÒNG**");
            sb.AppendLine("**LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})**");
            sb.AppendLine("**Tổng số văn phòng: {offices.Count}**");
            sb.AppendLine();

            foreach (var office in offices)
            {
                try
                {
                    var combined = await ListTopOfficeWorkingTimeLWLMInternal(office.Id, int.MaxValue, reportDate, userId);
                    var officeName = office.Code ?? office.Name ?? "Office#{office.Id}";

                    sb.AppendLine("🏢 **VP {officeName} ({combined.Count} nhân viên)**");

                    if (combined.Count == 0)
                    {
                        sb.AppendLine("   _Không có dữ liệu_");
                    }
                    else
                    {
                        var topItems = combined.Take(actualLimit);
                        int idx = 1;

                        foreach (var item in topItems)
                        {
                            var username = item.UserName ?? "N/A";
                            var atIndex = username.IndexOf('@');
                            if (atIndex >= 0)
                            {
                                username = username.Substring(0, atIndex);
                            }

                            sb.AppendLine("{idx}. {username}");
                            sb.AppendLine("   - LW: Total {item.TotalAllLWHours:F1}h (Office {item.OfficeLWHours:F1}h, WFH {item.WfhLWHours:F1}h)");
                            sb.AppendLine("   - LM: Total {item.TotalAllLMHours:F1}h (Office {item.OfficeLMHours:F1}h, WFH {item.WfhLMHours:F1}h)");
                            sb.AppendLine("   -------------------------");
                            idx++;
                        }

                        if (combined.Count > actualLimit)
                        {
                            sb.AppendLine("... và {combined.Count - actualLimit} người khác");
                        }
                    }

                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    sb.AppendLine("🏢 **VP {office.Code ?? office.Name}** - ❌ Lỗi: {ex.Message}");
                    sb.AppendLine();
                }
            }
            sb.AppendLine("📈 **Tổng cộng: {offices.Count} văn phòng**");
            sb.AppendLine("📅 **Thời gian: LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})**");
            return sb.ToString();
        }
    }
}