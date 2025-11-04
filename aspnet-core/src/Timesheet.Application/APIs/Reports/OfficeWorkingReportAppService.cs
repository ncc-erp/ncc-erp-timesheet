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
using Timesheet.APIs.BotReportDaily.Dto;
using Timesheet.APIs.Reports.Dto;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using Timesheet.Timesheets.Projects.Dto;
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
                        var indexEnd = dotIndex + 1;
                        AddBold(mkList, currentPos, currentPos + indexEnd);

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
                int start = messageText.IndexOf("**VP ", current);
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

            var users = await WorkScope.GetAll<Ncc.Authorization.Users.User>()
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork && u.BranchId == officeId)
                    .Join(WorkScope.GetAll<Branch>(),
                          u => u.BranchId,
                          b => b.Id,
                          (u, b) => new { User = u, Branch = b })
                    .Select(x => new UserLite
                    {
                        Id = x.User.Id,
                        Name = !string.IsNullOrEmpty(x.User.UserName) ? x.User.UserName :
                               (!string.IsNullOrEmpty(x.User.Name) ? x.User.Name : "Unknown"),
                        Email = x.User.EmailAddress,
                        OfficeName = x.Branch != null ? x.Branch.Name : string.Empty,
                        OfficeCode = x.Branch != null ? x.Branch.Code : string.Empty
                    })
                    .AsNoTracking()
                    .ToListAsync();

            var mapUserId = users.ToDictionary(x => x.Id, x => x);
            var mapEmail = users.Where(x => !string.IsNullOrEmpty(x.Email)).GroupBy(x => x.Email.Trim().ToLower()).ToDictionary(g => g.Key, g => g.First());

            var tkList = await WorkScope.GetAll<Timekeeping>().Where(t => t.DateAt >= rangeStart && t.DateAt <= rangeEnd).Select(t => new
            {
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

        private List<List<OfficeWorkingTopLWLMDto>> SplitIntoChunks(List<OfficeWorkingTopLWLMDto> items, int batchSize)
        {
            var chunks = new List<List<OfficeWorkingTopLWLMDto>>();
            for (int i = 0; i < items.Count; i += batchSize)
            {
                var chunk = items.Skip(i).Take(batchSize).ToList();
                chunks.Add(chunk);
            }
            return chunks;
        }
        private async Task<string> SendTopOfficeUsersNotificationInternal(
          long? officeId = null, int limit = 20, DateTime? reportDate = null, string mezonUrl = null, long? userId = null, DateTime? startDate = null, DateTime? endDate = null, bool showAll = false, bool allOffices = false)
        {
            const int BATCH_SIZE = 5;
            const int MESSAGE_DELAY_MS = 1000;
            var actualLimit = showAll ? int.MaxValue : limit;
            var sb = new StringBuilder();
            var webhookUrl = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportWebhookUrl);
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
                    AddBoldForLabel(mkList, messageText, "Working Report");
                    AddBoldForLabel(mkList, messageText, "Total branches:");
                    AddBoldForLabel(mkList, messageText, "Time:");
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
                return "Please provide officeId or set allOffices=true";
            }

            var items = await ListTopOfficeWorkingTimeInternal(officeId.Value, actualLimit, reportDate, userId, startDate, endDate);

            var branchInfo = await WorkScope.GetAll<Branch>().Where(b => b.Id == officeId.Value).Select(b => new
            {
                b.Name,
                b.Code,
                b.DisplayName
            }).FirstOrDefaultAsync();

            var vpName = branchInfo?.Code ?? branchInfo?.Name ?? "Office#{officeId.Value}";
            var now = (reportDate?.Date ?? DateTimeUtils.GetNow().Date);
            DateTime s, e;

            var baseTime = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
            var combined = await ListTopOfficeWorkingTimeLWLMInternal(officeId.Value, actualLimit, reportDate, userId);
            var (lwStart, lwEnd) = GetLastWeekRange(baseTime);
            var lmStart = DateTimeUtils.FirstDayOfMonth(baseTime.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(baseTime.AddMonths(-1));
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
                var headerSb = new StringBuilder();
                headerSb.AppendLine($"{(actualLimit == int.MaxValue ? "All " : $"Top {actualLimit}")} Employees By Working Hours – Branch {vpName}");
                headerSb.AppendLine($"LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
                headerSb.AppendLine("═══════════════════════════");
                var headerMessageText = headerSb.ToString();
                var headerMkList = new List<object>();
                AddBoldForAllOccurrences(headerMkList, headerMessageText, $"{(actualLimit == int.MaxValue ? "All " : $"Top {actualLimit}")} Employees By Working Hours – Branch {vpName}");
                AddBoldForAllOccurrences(headerMkList, headerMessageText, $"LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
                _mezonService.Post(webhookUrl, new
                {
                    type = "hook",
                    message = new
                    {
                        t = headerMessageText,
                        mk = headerMkList
                    }
                });


                if (combined.Count == 0)
                {
                    sb.AppendLine("No data");
                }
                else
                {
                    int idx = 1;
                    var itemsToShow = showAll ? combined.ToList() : combined.Take(actualLimit).ToList();
                    var chunks = SplitIntoChunks(itemsToShow, BATCH_SIZE);
                    for (int i=0; i<chunks.Count; i++)
                    {
                        var chunk = chunks[i];
                        bool isLastChunk = i == chunks.Count - 1;

                        var chunkSb = new StringBuilder();
                        var chunkMkList = new List<object>();

                        foreach (var item in chunk)
                        {
                            var itemSb = new StringBuilder();
                            var username = item.UserName ?? "N/A";
                            var atIndex = username.IndexOf('@');
                            if (atIndex >= 0)
                            {
                                username = username.Substring(0, atIndex);
                            }

                            itemSb.AppendLine($"{idx}. {username}");
                            itemSb.AppendLine($"   - LW: Total {item.TotalAllLW:F1}h (Office {item.OfficeLW:F1}h, WFH {item.WfhLW:F1}h)");
                            itemSb.AppendLine($"   - LM: Total {item.TotalAllLM:F1}h (Office {item.OfficeLM:F1}h, WFH {item.WfhLM:F1}h)");
                            itemSb.AppendLine("═══════════════════════════");

                            string itemText = itemSb.ToString();
                            chunkSb.Append(itemText);

                            idx++;
                        }

                        if (isLastChunk)
                        {
                            chunkSb.AppendLine();
                            chunkSb.AppendLine($"Number of employees by working hours: {combined.Count}");
                            chunkSb.AppendLine($"Time: LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
                            chunkSb.AppendLine("═══════════════════════════");
                        }

                        var chunkMessageText = chunkSb.ToString();
                        AddBoldForLabel(chunkMkList, chunkMessageText, "Time:");
                        AddBoldForUsernames(chunkMkList, chunkMessageText);
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, "LW:");
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, "LM:");
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, $"Number of employees by working hours: {combined.Count}");
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, $"LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");


                        _mezonService.Post(webhookUrl, new
                        {
                            type = "hook",
                            message = new
                            {
                                t = chunkMessageText,
                                mk = chunkMkList
                            }
                        });

                        await Task.Delay(MESSAGE_DELAY_MS);
                    }

                    

                    if (combined.Count > itemsToShow.Count())
                    {
                        sb.AppendLine($"... and {combined.Count - itemsToShow.Count()} other emoployees");
                    }
                }
            }
            else
            {
                if (userId.HasValue)
                {
                    sb.AppendLine("Report Sorted By TotalTime – Branch {vpName} – UserId {userId.Value} ({s:dd/MM}–{e:dd/MM})");
                }
                else
                {
                    sb.AppendLine($"{(showAll ? "All " : $"Top {actualLimit}")} sorted by TotalTime – VP {vpName} ({s:dd/MM}–{e:dd/MM})");
                }
                if (items.Count == 0)
                {
                    sb.AppendLine("No data.");
                }
                else
                {
                    int idx = 1;
                    foreach (var i in items)
                    {
                        var officeShown = string.IsNullOrWhiteSpace(i.OfficeCode) ? i.OfficeName : i.OfficeCode;
                        sb.AppendLine("{idx}) {i.UserName} | {officeShown} | {i.TotalAllLW}h");
                        idx++;
                    }
                }
            }

            var url = mezonUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                url = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);
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
            var minStart = lwStart < lmStart ? lwStart : lmStart;
            var maxEnd = lwEnd > lmEnd ? lwEnd : lmEnd;
            var users = await (
                from u in WorkScope.GetAll<Ncc.Authorization.Users.User>()
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork && u.BranchId == officeId)
                join b in WorkScope.GetAll<Branch>() on u.BranchId equals b.Id
                select new UserLite
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Email = u.EmailAddress,
                    OfficeName = b.Name,
                    OfficeCode = b.Code
                })
                .AsNoTracking()
                .ToListAsync();

            if (!users.Any())
                return new List<OfficeWorkingTopLWLMDto>();

            var mapUserId = users.ToDictionary(x => x.Id);
            var mapEmail = users
                .Where(x => !string.IsNullOrEmpty(x.Email))
                .GroupBy(x => x.Email.Trim().ToLower())
                .ToDictionary(g => g.Key, g => g.First());
            var userIds = users.Select(u => u.Id).ToList();
            var userEmails = mapEmail.Keys.ToList();
            var tkList = await WorkScope.GetAll<Timekeeping>()
                .Where(t => t.DateAt >= minStart && t.DateAt <= maxEnd)
                .Where(t => (t.UserId.HasValue && userIds.Contains(t.UserId.Value)) ||
                            (!t.UserId.HasValue && !string.IsNullOrEmpty(t.UserEmail) &&
                             userEmails.Contains(t.UserEmail.Trim().ToLower())))
                .Select(t => new
                {
                    t.UserId,
                    UserEmail = t.UserEmail != null ? t.UserEmail.Trim().ToLower() : null,
                    t.DateAt,
                    t.CheckIn,
                    t.CheckOut,
                    t.TrackerTime
                })
                .AsNoTracking()
                .ToListAsync();
            var remoteDetails = await (
                from d in WorkScope.GetAll<AbsenceDayDetail>()
                    .Where(d => d.DateAt >= minStart && d.DateAt <= maxEnd)
                join r in WorkScope.GetAll<AbsenceDayRequest>()
                    .Where(r => r.Status == RequestStatus.Approved &&
                               r.Type == RequestType.Remote &&
                               userIds.Contains(r.UserId))
                    on d.RequestId equals r.Id
                select new
                {
                    d.DateAt,
                    d.DateType,
                    UserId = r.UserId
                })
                .AsNoTracking()
                .ToListAsync();
            var remoteMap = new Dictionary<(long, DateTime), RemoteFlags>();
            foreach (var r in remoteDetails)
            {
                var key = (r.UserId, r.DateAt.Date);
                var val = remoteMap.TryGetValue(key, out var existing) ? existing : new RemoteFlags(false, false);

                if (r.DateType == DayType.Fullday)
                    val = new RemoteFlags(true, true);
                else if (r.DateType == DayType.Morning)
                    val = new RemoteFlags(true, val.Afternoon);
                else if (r.DateType == DayType.Afternoon)
                    val = new RemoteFlags(val.Morning, true);

                remoteMap[key] = val;
            }

            var agg = new Dictionary<long, OfficeWorkingTopLWLMDto>();
            foreach (var t in tkList)
            {
                long? uid = t.UserId;
                UserLite uinfo = null;

                if (uid.HasValue)
                {
                    mapUserId.TryGetValue(uid.Value, out uinfo);
                }
                else if (!string.IsNullOrWhiteSpace(t.UserEmail))
                {
                    if (mapEmail.TryGetValue(t.UserEmail, out var u))
                    {
                        uid = u.Id;
                        uinfo = u;
                    }
                }
                if (uinfo == null || !uid.HasValue)
                    continue;
                if (userId.HasValue && uid.Value != userId.Value)
                    continue;

                var trackerMinutes = 0;
                if (!string.IsNullOrWhiteSpace(t.TrackerTime) && TimeSpan.TryParse(t.TrackerTime, out var ts))
                    trackerMinutes = (int)ts.TotalMinutes;

                var (mMin, aMin) = ComputeSplitWorkingMinutes(t.CheckIn, t.CheckOut);
                if (mMin + aMin + trackerMinutes <= 0)
                    continue;

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
                var rf = remoteMap.TryGetValue((uid.Value, t.DateAt.Date), out var flags)
                    ? flags
                    : new RemoteFlags(false, false);

                bool inLW = t.DateAt.Date >= lwStart && t.DateAt.Date <= lwEnd;
                bool inLM = t.DateAt.Date >= lmStart && t.DateAt.Date <= lmEnd;
                bool isRemote = rf.Morning || rf.Afternoon;

                if (inLW)
                {
                    int workingMin = isRemote
                        ? (trackerMinutes > 0 ? trackerMinutes : mMin + aMin)
                        : (mMin + aMin > 0 ? mMin + aMin : trackerMinutes);
                    var hours = Math.Round(workingMin / 60.0, 1);
                    row.TotalAllLW += hours;
                    if (isRemote)
                        row.WfhLW += hours;
                    else
                        row.OfficeLW += hours;
                }

                if (inLM)
                {
                    int workingMin = isRemote
                        ? (trackerMinutes > 0 ? trackerMinutes : mMin + aMin)
                        : (mMin + aMin > 0 ? mMin + aMin : trackerMinutes);
                    var hours = Math.Round(workingMin / 60.0, 1);
                    row.TotalAllLM += hours;
                    if (isRemote)
                        row.WfhLM += hours;
                    else
                        row.OfficeLM += hours;
                }
            }
            foreach (var user in users)
            {
                if (!agg.ContainsKey(user.Id))
                {
                    agg[user.Id] = new OfficeWorkingTopLWLMDto
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        OfficeName = user.OfficeName,
                        OfficeCode = user.OfficeCode
                    };
                }
            }
            var rs = agg.Values.OrderByDescending(x => x.TotalAllLW).ThenByDescending(x => x.TotalAllLM);
            return limit == int.MaxValue ? rs.ToList() : rs.Take(limit).ToList();
        }

        [AbpAuthorize]
        [HttpGet]
        public Task<List<OfficeWorkingTopLWLMDto>> GetListTopOfficeWorkingTimeLWLM(
        long officeId, int limit = int.MaxValue, DateTime? reportDate = null, long? userId = null)
        {
            return ListTopOfficeWorkingTimeLWLMInternal(officeId, limit, reportDate, userId);
        }
        private async Task<string> GetAllOfficesReport(
     int actualLimit, DateTime? reportDate, long? userId,
     DateTime? startDate, DateTime? endDate)
        {
            var sb = new StringBuilder();
            var baseTime = reportDate?.Date ?? DateTimeUtils.GetNow().Date;
            var (lwStart, lwEnd) = GetLastWeekRange(baseTime);
            var lmStart = DateTimeUtils.FirstDayOfMonth(baseTime.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(baseTime.AddMonths(-1));
            var minStart = lwStart < lmStart ? lwStart : lmStart;
            var maxEnd = lwEnd > lmEnd ? lwEnd : lmEnd;
            var offices = await WorkScope.GetAll<Branch>()
                .Where(b => b.Id > 0)
                .Select(b => new { b.Id, b.Name, b.Code })
                .OrderBy(b => b.Code ?? b.Name)
                .AsNoTracking()
                .ToListAsync();

            if (!offices.Any())
                return "No branch found";

            var officeIds = offices.Select(o => o.Id).ToList();
            var usersQuery = WorkScope.GetAll<Ncc.Authorization.Users.User>()
                .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork
                         && u.BranchId.HasValue && officeIds.Contains(u.BranchId.Value));
            if (userId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.Id == userId.Value);
            }

            var allUsers = await (
                from u in usersQuery
                join b in WorkScope.GetAll<Branch>() on u.BranchId equals b.Id
                select new
                {
                    UserId = u.Id,
                    u.FullName,
                    EmailAddress = u.EmailAddress ?? "",
                    BranchId = b.Id,
                    BranchName = b.Name,
                    BranchCode = b.Code
                })
                .AsNoTracking()
                .ToListAsync();

            if (!allUsers.Any())
            {
                sb.AppendLine("Employee Report");
                sb.AppendLine($"LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
                sb.AppendLine($"Total branches: {offices.Count}");
                sb.AppendLine("No data found");
                return sb.ToString();
            }

            var userIds = allUsers.Select(u => u.UserId).ToList();
            var userEmails = allUsers
                .Where(u => !string.IsNullOrEmpty(u.EmailAddress))
                .Select(u => u.EmailAddress.Trim().ToLower())
                .ToList();
            var allTks = await WorkScope.GetAll<Timekeeping>()
                .Where(t => t.DateAt >= minStart && t.DateAt <= maxEnd)
                .Where(t => userIds.Contains(t.UserId.Value) ||
                           (t.UserEmail != null && userEmails.Contains(t.UserEmail.Trim().ToLower())))
                .Select(t => new
                {
                    t.UserId,
                    UserEmail = t.UserEmail != null ? t.UserEmail.Trim().ToLower() : "",
                    t.DateAt,
                    t.CheckIn,
                    t.CheckOut,
                    t.TrackerTime
                })
                .AsNoTracking()
                .ToListAsync();
            var allRemotes = await (
                from d in WorkScope.GetAll<AbsenceDayDetail>()
                    .Where(d => d.DateAt >= minStart && d.DateAt <= maxEnd)
                join r in WorkScope.GetAll<AbsenceDayRequest>()
                    .Where(r => r.Status == RequestStatus.Approved
                             && r.Type == RequestType.Remote
                             && userIds.Contains(r.UserId))
                    on d.RequestId equals r.Id
                select new
                {
                    d.DateAt,
                    d.DateType,
                    r.UserId
                })
                .AsNoTracking()
                .ToListAsync();
            var usersByOffice = allUsers.ToLookup(u => u.BranchId);
            var userByEmail = allUsers
                .Where(u => !string.IsNullOrEmpty(u.EmailAddress))
                .GroupBy(u => u.EmailAddress.Trim().ToLower())
                .ToDictionary(g => g.Key, g => g.First());
            var enrichedTks = allTks
                .Select(t =>
                {
                    var uid = t.UserId;
                    if (!uid.HasValue && !string.IsNullOrEmpty(t.UserEmail))
                    {
                        if (userByEmail.TryGetValue(t.UserEmail, out var u))
                            uid = u.UserId;
                    }
                    return new { UserId = uid, Record = t };
                })
                .Where(x => x.UserId.HasValue)
                .ToLookup(x => x.UserId.Value, x => x.Record);            var remoteMap = new Dictionary<(long, DateTime), RemoteFlags>();
            foreach (var r in allRemotes)
            {
                var key = (r.UserId, r.DateAt.Date);
                var val = remoteMap.TryGetValue(key, out var v) ? v : new RemoteFlags(false, false);

                if (r.DateType == DayType.Fullday)
                    val = new RemoteFlags(true, true);
                else if (r.DateType == DayType.Morning)
                    val = new RemoteFlags(true, val.Afternoon);
                else if (r.DateType == DayType.Afternoon)
                    val = new RemoteFlags(val.Morning, true);

                remoteMap[key] = val;
            }
            sb.AppendLine("Employee Report");
            sb.AppendLine($"LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
            sb.AppendLine($"Total branches: {offices.Count}");
            sb.AppendLine();
            foreach (var office in offices)
            {
                try
                {
                    var officeUsers = usersByOffice[office.Id].ToList();
                    var combined = new List<OfficeWorkingTopLWLMDto>();

                    foreach (var user in officeUsers)
                    {
                        var dto = new OfficeWorkingTopLWLMDto
                        {
                            UserId = user.UserId,
                            UserName = user.FullName,
                            OfficeName = user.BranchName,
                            OfficeCode = user.BranchCode
                        };

                        foreach (var tk in enrichedTks[user.UserId])
                        {
                            var trackerMin = 0;
                            if (!string.IsNullOrWhiteSpace(tk.TrackerTime) &&
                                TimeSpan.TryParse(tk.TrackerTime, out var ts))
                                trackerMin = (int)ts.TotalMinutes;

                            var (mMin, aMin) = ComputeSplitWorkingMinutes(tk.CheckIn, tk.CheckOut);
                            if (mMin + aMin + trackerMin <= 0)
                                continue;

                            var rf = remoteMap.TryGetValue((user.UserId, tk.DateAt.Date), out var f)
                                ? f
                                : new RemoteFlags(false, false);

                            bool inLW = tk.DateAt.Date >= lwStart && tk.DateAt.Date <= lwEnd;
                            bool inLM = tk.DateAt.Date >= lmStart && tk.DateAt.Date <= lmEnd;
                            bool isRemote = rf.Morning || rf.Afternoon;

                            if (inLW)
                            {
                                int workingMin = isRemote
                                    ? (trackerMin > 0 ? trackerMin : mMin + aMin)
                                    : (mMin + aMin > 0 ? mMin + aMin : trackerMin);
                                var h = Math.Round(workingMin / 60.0, 1);
                                dto.TotalAllLW += h;
                                if (isRemote)
                                    dto.WfhLW += h;
                                else
                                    dto.OfficeLW += h;
                            }
                            if (inLM)
                            {
                                int workingMin = isRemote
                                    ? (trackerMin > 0 ? trackerMin : mMin + aMin)
                                    : (mMin + aMin > 0 ? mMin + aMin : trackerMin);
                                var h = Math.Round(workingMin / 60.0, 1);
                                dto.TotalAllLM += h;
                                if (isRemote)
                                    dto.WfhLM += h;
                                else
                                    dto.OfficeLM += h;
                            }
                        }
                        combined.Add(dto);
                    }

                    combined = combined
                        .OrderByDescending(x => x.TotalAllLW)
                        .ThenByDescending(x => x.TotalAllLM)
                        .ToList();

                    var officeName = office.Code ?? office.Name ?? $"Office#{office.Id}";
                    sb.AppendLine($"**VP {officeName}** ({combined.Count} employees)");

                    if (!combined.Any())
                    {
                        sb.AppendLine("No data");
                    }
                    else
                    {
                        int idx = 1;
                        foreach (var item in combined.Take(actualLimit))
                        {
                            var username = item.UserName ?? "N/A";
                            var atIdx = username.IndexOf('@');
                            if (atIdx >= 0)
                                username = username.Substring(0, atIdx);
                            sb.AppendLine($"{idx}. {username}");
                            sb.AppendLine($"   - LW: Total {item.TotalAllLW:F1}h (Office {item.OfficeLW:F1}h, WFH {item.WfhLW:F1}h)");
                            sb.AppendLine($"   - LM: Total {item.TotalAllLM:F1}h (Office {item.OfficeLM:F1}h, WFH {item.WfhLM:F1}h)");
                            sb.AppendLine("═══════════════════════════");
                            idx++;
                        }
                        if (combined.Count > actualLimit)
                            sb.AppendLine($"... and {combined.Count - actualLimit} other employees");
                    }
                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Branch {office.Code ?? office.Name} - Error: {ex.Message}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine($"Total branches: {offices.Count}");
            sb.AppendLine($"Time: LW ({lwStart:dd/MM}–{lwEnd:dd/MM}) | LM ({lmStart:dd/MM}–{lmEnd:dd/MM})");
            return sb.ToString();
        }
        public async Task<OfficeWorkingTimelogReportDto> GetOfficeWorkingTimelogReport(GetOfficeWorkingTimelogReportInput input)
        {
            var now = DateTimeUtils.GetNow().Date;
            var (lwStart, lwEnd) = GetLastWeekRange(now);
            var lmStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));
            var allBranchIds = await WorkScope.GetAll<Branch>()
                .AsNoTracking()
                .Select(b => b.Id)
                .ToListAsync();
            List<long> officeIds;
            if (input.BranchId == null || !input.BranchId.Any())
            {
                officeIds = allBranchIds;
            }
            else
            {
                officeIds = input.BranchId
                    .Where(id => allBranchIds.Contains(id))
                    .ToList();

                if (!officeIds.Any())
                {
                    throw new Abp.UI.UserFriendlyException("No valid branch codes provided.");
                }
            }
            var allUsers = new List<OfficeWorkingTopLWLMDto>();
            foreach (var officeId in officeIds)
            {
                var officeData = await ListTopOfficeWorkingTimeLWLMInternal(
                    officeId: officeId,
                    limit: int.MaxValue,
                    reportDate: now
                );
                allUsers.AddRange(officeData);
            }
            var topUsers = allUsers
                .OrderByDescending(u => u.TotalAllLW)
                .ThenByDescending(u => u.TotalAllLM)
                .ThenBy(u => u.UserName)
                .Take(input.Limit)
                .ToList();
            return new OfficeWorkingTimelogReportDto
            {
                LastWeekStart = lwStart.ToString("yyyy-MM-dd"),
                LastWeekEnd = lwEnd.ToString("yyyy-MM-dd"),
                LastMonth = lmStart.ToString("yyyy-MM"),
                OfficeWorkingData = topUsers
            };
        }
    }
}