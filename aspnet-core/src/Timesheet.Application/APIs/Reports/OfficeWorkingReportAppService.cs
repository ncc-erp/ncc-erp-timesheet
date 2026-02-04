using Abp.Application.Services.Dto;
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
using Timesheet.Extension;
using Timesheet.Paging;
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
            if (TimeSpan.TryParseExact(input, new[] { @"hh\:mm", @"h\:mm" }, CultureInfo.InvariantCulture, out var ts))
            {
                minutes = (int)ts.TotalMinutes;
                return true;
            }
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
            var noonStartMin = 12 * 60;
            var noonEndMin = 13 * 60;
            if (ciMin < noonStartMin && coMin > noonEndMin)
            {
                total -= 60;
            }
            return Math.Max(total, 0);
        }

        private async Task<List<OfficeWorkingTopLWLMDto>> ListTopOfficeWorkingTimeInternal(TopOfficeWorkingTimeDto input)
        {
            var now = input.ReportDate?.Date ?? DateTimeUtils.GetNow().Date;
            DateTime rangeStart;
            DateTime rangeEnd;
            if (input.StartDate.HasValue && input.EndDate.HasValue)
            {
                rangeStart = input.StartDate.Value.Date;
                rangeEnd = input.EndDate.Value.Date;
            }
            else
            {
                var lw = GetLastWeekRange(now);
                rangeStart = lw.start;
                rangeEnd = lw.end;
            }

            var users = await WorkScope.GetAll<Ncc.Authorization.Users.User>()
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork && u.BranchId == input.OfficeId)
                    .Select(x => new UserLiteDto
                    {
                        Id = x.Id,
                        Name = !string.IsNullOrEmpty(x.UserName) ? x.UserName :
                               (!string.IsNullOrEmpty(x.Name) ? x.Name : "Unknown"),
                        Email = x.EmailAddress,
                        BranchName = x.Branch != null ? x.Branch.Name : "",
                        BranchCode = x.Branch != null ? x.Branch.Code : "",
                        BranchColor = x.Branch != null ? x.Branch.Color : ""
                    })
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
            }).ToListAsync();

            var aggMap = new Dictionary<long, UserMapDto>();

            foreach (var t in tkList)
            {
                long? userIdResolved = t.UserId;
                UserLiteDto userInfo = null;
                if (userIdResolved.HasValue && mapUserId.TryGetValue(userIdResolved.Value, out var infoById))
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
                if (input.UserId.HasValue && userIdResolved.Value != input.UserId.Value)
                {
                    continue;
                }

                var minutes = ComputeWorkingMinutes(t.CheckIn, t.CheckOut);
                if (minutes <= 0) continue;

                if (!aggMap.TryGetValue(userIdResolved.Value, out var current))
                {
                    current = new UserMapDto
                    {
                        Name = userInfo.Name,
                        BranchName = userInfo.BranchName,
                        BranchCode = userInfo.BranchCode,
                        BranchColor = userInfo.BranchColor,
                        Minutes = 0
                    };
                }
                current.Minutes += minutes;
                aggMap[userIdResolved.Value] = current;
            }

            var result = aggMap.Select(kv => new OfficeWorkingTopLWLMDto
            {
                UserId = kv.Key,
                UserName = kv.Value.Name,
                BranchName = kv.Value.BranchName,
                BranchCode = kv.Value.BranchCode,
                TotalAllLW = kv.Value.Minutes / 60.0,
                OfficeLW = kv.Value.Minutes / 60.0
            }).OrderByDescending(x => x.TotalAllLW);

            if (input.Limit == int.MaxValue)
            {
                return result.ToList();
            }
            else
            {
                return result.Take(input.Limit).ToList();
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
        private async Task<string> SendTopOfficeUsersNotificationInternal(SendTopOfficeWorkingTimeNotificationDto input)
        {
            const int BATCH_SIZE = 5;
            const int MESSAGE_DELAY_MS = 1000;
            var actualLimit = input.ShowAll ? int.MaxValue : input.Limit;
            var sb = new StringBuilder();
            var webhookUrl = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportWebhookUrl);
            if (!input.OfficeId.HasValue)
            {
                return "Please provide officeId or set allOffices=true";
            }

            var inputDto = new TopOfficeWorkingTimeDto
            {
                OfficeId = input.OfficeId.Value,
                Limit = actualLimit,
                ReportDate = input.ReportDate,
                UserId = input.UserId,
                StartDate = input.StartDate,
                EndDate = input.EndDate
            };

            var items = await ListTopOfficeWorkingTimeInternal(inputDto);

            var branchInfo = await WorkScope.GetAll<Branch>().Where(b => b.Id == input.OfficeId.Value).Select(b => new
            {
                b.Name,
                b.Code,
                b.DisplayName
            }).FirstOrDefaultAsync();

            var vpName = branchInfo?.Code ?? branchInfo?.Name ?? "Office#{officeId.Value}";
            var now = (input.ReportDate?.Date ?? DateTimeUtils.GetNow().Date);
            DateTime s, e;

            var baseTime = input.ReportDate?.Date ?? DateTimeUtils.GetNow().Date;
            var combined = await ListTopOfficeWorkingTimeLWLMInternal(input.OfficeId.Value, actualLimit, input.ReportDate, input.UserId);
            var (lwStart, lwEnd) = GetLastWeekRange(baseTime);
            var lmStart = DateTimeUtils.FirstDayOfMonth(baseTime.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(baseTime.AddMonths(-1));
            if (input.StartDate.HasValue && input.EndDate.HasValue)
            {
                s = input.StartDate.Value.Date;
                e = input.EndDate.Value.Date;
            }
            else
            {
                var lw = GetLastWeekRange(now);
                s = lw.start;
                e = lw.end;
            }
            if (!input.StartDate.HasValue || !input.EndDate.HasValue)
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
                    var itemsToShow = input.ShowAll ? combined.ToList() : combined.Take(actualLimit).ToList();
                    var chunks = SplitIntoChunks(itemsToShow, BATCH_SIZE);
                    for (int i = 0; i < chunks.Count; i++)
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
                if (input.UserId.HasValue)
                {
                    sb.AppendLine("Report Sorted By TotalTime – Branch {vpName} – UserId {userId.Value} ({s:dd/MM}–{e:dd/MM})");
                }
                else
                {
                    sb.AppendLine($"{(input.ShowAll ? "All " : $"Top {actualLimit}")} sorted by TotalTime – VP {vpName} ({s:dd/MM}–{e:dd/MM})");
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
                        var officeShown = string.IsNullOrWhiteSpace(i.BranchCode) ? i.BranchName : i.BranchCode;
                        sb.AppendLine("{idx}) {i.UserName} | {officeShown} | {i.TotalAllLW}h");
                        idx++;
                    }
                }
            }

            var url = input.MezonUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                url = SettingManager.GetSettingValueForApplication(AppSettingNames.OfficeWorkingReportMezonUrl);
            }

            return sb.ToString();
        }

        public Task<string> SendTopOfficeUsersNotification(SendTopOfficeWorkingTimeNotificationDto input)
        {
            return SendTopOfficeUsersNotificationInternal(input);
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

            var users = await WorkScope.GetAll<Ncc.Authorization.Users.User>()
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork && u.BranchId == officeId)
                    .Join(WorkScope.GetAll<Branch>(),
                          u => u.BranchId,
                          b => b.Id,
                          (u, b) => new { User = u, Branch = b })
                    .Select(x => new UserLiteDto
                    {
                        Id = x.User.Id,
                        Name = x.User.FullName,
                        UserName = x.User.UserName,
                        Email = x.User.EmailAddress,
                        BranchName = x.Branch != null ? x.Branch.Name : "",
                        BranchCode = x.Branch != null ? x.Branch.Code : "",
                        BranchColor = x.Branch != null ? x.Branch.Color : ""
                    })
                    .ToListAsync();

            if (!users.Any())
                return new List<OfficeWorkingTopLWLMDto>();

            var userIds = new HashSet<long>(users.Select(u => u.Id));

            var userEmails = users
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .ToDictionary(u => u.Email.Trim().ToLower(), u => u.Id);

            var timekeepings = await WorkScope.GetAll<Timekeeping>()
                                    .Where(t => t.DateAt >= minStart && t.DateAt <= maxEnd)
                                    .Where(t => (t.UserId.HasValue && userIds.Contains(t.UserId.Value)) ||
                                               (!t.UserId.HasValue && !string.IsNullOrEmpty(t.UserEmail) &&
                                                userEmails.ContainsKey(t.UserEmail.Trim().ToLower())))
                                    .Select(t => new
                                    {
                                        UserId = t.UserId ?? userEmails[t.UserEmail.Trim().ToLower()],
                                        t.DateAt,
                                        t.CheckIn,
                                        t.CheckOut,
                                        t.TrackerTime
                                    })
                                    .ToListAsync();

            var remoteDetails = await (from d in WorkScope.GetAll<AbsenceDayDetail>()
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
                                           r.UserId
                                       })
                                     .ToListAsync();

            var remoteMap = remoteDetails
                .GroupBy(x => (x.UserId, x.DateAt.Date))
                .ToDictionary(
                    g => g.Key,
                    g => new RemoteFlags(
                        g.Any(x => x.DateType == DayType.Morning || x.DateType == DayType.Fullday),
                        g.Any(x => x.DateType == DayType.Afternoon || x.DateType == DayType.Fullday)
                    )
                );

            var result = new Dictionary<long, OfficeWorkingTopLWLMDto>();
            var userMap = users.ToDictionary(u => u.Id, u => u);
            foreach (var tk in timekeepings)
            {
                if (!userMap.TryGetValue(tk.UserId, out var userInfo))
                    continue;

                var workingMinutes = ComputeWorkingMinutes(tk.CheckIn, tk.CheckOut);
                if (workingMinutes <= 0 && string.IsNullOrEmpty(tk.TrackerTime))
                    continue;

                if (!result.TryGetValue(tk.UserId, out var userReport))
                {
                    userReport = new OfficeWorkingTopLWLMDto
                    {
                        UserId = userInfo.Id,
                        FullName = userInfo.Name,
                        UserName = userInfo.UserName,
                        BranchName = userInfo.BranchName,
                        BranchCode = userInfo.BranchCode,
                        BranchColor = userInfo.BranchColor
                    };
                    result[tk.UserId] = userReport;
                }
                var isInLW = tk.DateAt.Date >= lwStart && tk.DateAt.Date <= lwEnd;
                var isInLM = tk.DateAt.Date >= lmStart && tk.DateAt.Date <= lmEnd;
                var isRemote = remoteMap.TryGetValue((tk.UserId, tk.DateAt.Date), out var flags) &&
                              (flags.Morning || flags.Afternoon);

                var trackerMinutes = !string.IsNullOrWhiteSpace(tk.TrackerTime) &&
                                   TimeSpan.TryParse(tk.TrackerTime, out var ts)
                                   ? (int)ts.TotalMinutes
                                   : 0;

                var workingMin = isRemote
                    ? (trackerMinutes > 0 ? trackerMinutes : workingMinutes)
                    : (workingMinutes > 0 ? workingMinutes : trackerMinutes);

                var hours = Math.Round(workingMin / 60.0, 1);
                double wfhHours = isRemote ? hours : 0;
                double officeHours = isRemote ? 0 : hours;

                if (isInLW)
                {
                    userReport.TotalAllLW += hours;
                    userReport.WfhLW += wfhHours;
                    userReport.OfficeLW += officeHours;
                }

                if (isInLM)
                {
                    userReport.TotalAllLM += hours;
                    userReport.WfhLM += wfhHours;
                    userReport.OfficeLM += officeHours;
                }
            }

            foreach (var user in users.Where(u => !result.ContainsKey(u.Id)))
            {
                result[user.Id] = new OfficeWorkingTopLWLMDto
                {
                    UserId = user.Id,
                    FullName = user.Name,
                    UserName = user.UserName,
                    BranchName = user.BranchName,
                    BranchCode = user.BranchCode,
                    BranchColor = user.BranchColor
                };
            }
            return result.Values
                .OrderByDescending(x => x.TotalAllLW)
                .ThenByDescending(x => x.TotalAllLM)
                .Take(limit)
                .ToList();
        }


        public async Task<PagedResultDto<OfficeWorkingTopLWLMDto>> GetOfficeWorkingTimelogReport(GridParam param, GetOfficeWorkingTimelogReportInputDto input)
        {
            var now = DateTimeUtils.GetNow().Date;
            var (lwStart, lwEnd) = GetLastWeekRange(now);
            var lmStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
            var lmEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));
            var allBranchIds = await WorkScope.GetAll<Branch>()
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

            var result = allUsers
                .OrderByDescending(u => u.TotalAllLW)
                .ThenByDescending(u => u.TotalAllLM)
                .ThenBy(u => u.UserName)
                .Take(input.Limit)
                .ToList();

            var totalCount = result.Count();

            var pagedData = result
                .Skip(param.SkipCount)
                .Take(param.MaxResultCount)
                .ToList();


            return new PagedResultDto<OfficeWorkingTopLWLMDto>(totalCount, result);
        }
    }
}