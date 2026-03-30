using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Amazon.Runtime.Internal;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Services.Mezon;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;
using static Sieve.Extensions.MethodInfoExtended;
using Branch = Timesheet.Entities.Branch;

namespace Timesheet.DomainServices
{
    public class OfficeWorkingReportServices : BaseDomainService, IOfficeWorkingReportServices, ITransientDependency
    {
        private readonly MezonService _mezonService;
        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;

        public OfficeWorkingReportServices(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager) : base(workScope)
        {
            _mezonService = mezonService;
            _workScope = workScope;
            _settingManager = settingManager;
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
                    AddBold(mkList, currentPos, currentPos + line.Length);
                }

                currentPos += line.Length + 1;
            }
        }

        private static (DateTime start, DateTime end) GetLastWeekRange(DateTime reportDate)
        {
            var thisWeekStart = DateTimeUtils.FirstDayOfWeek(reportDate);
            var lastWeekStart = thisWeekStart.AddDays(-7).Date;
            var lastWeekEnd = thisWeekStart.AddDays(-1).Date.AddDays(1).AddTicks(-1);
            return (lastWeekStart, lastWeekEnd);
        }

        private static TimeSpan? TryParseTime(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (TimeSpan.TryParseExact(input, new[] { @"hh\:mm", @"h\:mm" }, CultureInfo.InvariantCulture, out var ts))
            {
                return ts;
            }
            return null;
        }

        private static (double OfficeHours, double WfhHours) ComputeWorkingHours(ComputeWorkingHoursInputDto input)
        {
            double officeHours = 0;
            double wfhHours = 0;
            double faceIdHours = 0;

            if (input.CheckInTs.HasValue && input.CheckOutTs.HasValue && 
                input.MorningEndAtTs.HasValue && input.AfternoonStartAtTs.HasValue)
            {
                int checkInMinutes = (int)input.CheckInTs.Value.TotalMinutes;
                int checkOutMinutes = (int)input.CheckOutTs.Value.TotalMinutes;
                int morningEndAtMinutes = (int)input.MorningEndAtTs.Value.TotalMinutes;
                int afternoonStartAtMinutes = (int)input.AfternoonStartAtTs.Value.TotalMinutes;
                int officeWorkingMinutes = checkOutMinutes - checkInMinutes;

                int overlapStart = Math.Max(checkInMinutes, morningEndAtMinutes);
                int overlapEnd = Math.Min(checkOutMinutes, afternoonStartAtMinutes);

                int breakTimeOverlap = Math.Max(0, overlapEnd - overlapStart);

                officeWorkingMinutes -= breakTimeOverlap;
                faceIdHours = Math.Max(officeWorkingMinutes, 0) / 60.0;
            }

            double morningSessionHours = (input.CheckInTs.HasValue && input.MorningEndAtTs.HasValue)
                ? Math.Max(0, input.MorningEndAtTs.Value.TotalMinutes - input.CheckInTs.Value.TotalMinutes) / 60.0
                : 0;

            double afternoonSessionHours = (input.CheckOutTs.HasValue && input.AfternoonStartAtTs.HasValue)
                ? Math.Max(0, input.CheckOutTs.Value.TotalMinutes - input.AfternoonStartAtTs.Value.TotalMinutes) / 60.0
                : 0;

            if (input.IsRemoteRequest && input.RemoteDayType.HasValue)
            {
                if (input.RemoteDayType.Value == DayType.Fullday)
                {
                    officeHours = 0;
                    wfhHours = input.TrackerHours > 0 ? input.TrackerHours : faceIdHours;
                }
                else if (input.RemoteDayType.Value == DayType.Afternoon)
                {
                    officeHours = morningSessionHours;
                    wfhHours = input.TrackerHours > 0 ? input.TrackerHours : afternoonSessionHours;
                }
                else
                {
                    officeHours = afternoonSessionHours;
                    wfhHours = morningSessionHours;
                }
            }
            else
            {
                officeHours = faceIdHours > 0 ? faceIdHours : input.TrackerHours;
            }

            return (officeHours, wfhHours);
        }

        public async Task<string> SendTopOfficeUsersNotification(SendTopOfficeWorkingTimeNotificationDto input)
        {
            try
            {
                const int BATCH_SIZE = 5;
                const int MESSAGE_DELAY_MS = 1000;
                var actualLimit = input.Limit;
                var sb = new StringBuilder();
                var webhookUrl = await SettingManager.GetSettingValueAsync(AppSettingNames.OfficeWorkingReportMezonUrl);

                var branchInfo = await _workScope.GetAll<Branch>().Where(b => b.Id == input.OfficeId.Value)
                    .Select(b => new { b.Name, b.Code }).FirstOrDefaultAsync();

                var officeName = branchInfo?.Code ?? branchInfo?.Name ?? $"Office#{input.OfficeId.Value}";
                var now = DateTimeUtils.GetNow().Date;
                var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);
                var lastMonthStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
                var lastMonthEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));

                var officeWorkingData = await ListTopOfficeWorkingTimeLWLMInternal(new List<long> { input.OfficeId.Value }, null);
                
                var headerSb = new StringBuilder();
                headerSb.AppendLine($"Top {actualLimit} Employees By Working Hours – Branch {officeName}");
                headerSb.AppendLine($"LW ({lastWeekStart:dd/MM} – {lastWeekEnd:dd/MM}) | LM ({lastMonthStart:dd/MM} – {lastMonthEnd:dd/MM})");
                headerSb.AppendLine("═══════════════════════════");
                var headerMessageText = headerSb.ToString();
                var headerMkList = new List<object>();
                AddBoldForAllOccurrences(headerMkList, headerMessageText, $"Top {actualLimit} Employees By Working Hours – Branch {officeName}");
                AddBoldForAllOccurrences(headerMkList, headerMessageText, $"LW ({lastWeekStart:dd/MM} – {lastWeekEnd:dd/MM}) | LM ({lastMonthStart:dd/MM} – {lastMonthEnd:dd/MM})");
                _mezonService.Post(webhookUrl, new
                {
                    type = "hook",
                    message = new
                    {
                        t = headerMessageText,
                        mk = headerMkList
                    }
                });


                if (officeWorkingData.Count == 0)
                {
                    sb.AppendLine("No data");
                }
                else
                {
                    int idx = 1;
                    var itemsToShow = officeWorkingData.Take(actualLimit).ToList();
                    var chunks = CommonUtils.SplitIntoChunks(itemsToShow.Cast<dynamic>().ToList(), BATCH_SIZE);
                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var chunk = chunks[i];
                        bool isLastChunk = i == chunks.Count - 1;

                        var chunkSb = new StringBuilder();
                        var chunkMkList = new List<object>();

                        foreach (var item in chunk)
                        {
                            var itemSb = new StringBuilder();
                            var username = item.UserName ?? item.UserId;

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
                            chunkSb.AppendLine($"Number of employees by working hours: {itemsToShow.Count}");
                            chunkSb.AppendLine($"Time: LW ({lastWeekStart:dd/MM} – {lastWeekEnd:dd/MM}) | LM ({lastMonthStart:dd/MM} – {lastMonthEnd:dd/MM})");
                            chunkSb.AppendLine("═══════════════════════════");
                        }

                        var chunkMessageText = chunkSb.ToString();
                        AddBoldForUsernames(chunkMkList, chunkMessageText);
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, "Time:");
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, "LW:");
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, "LM:");
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, $"Number of employees by working hours: {itemsToShow.Count}");
                        AddBoldForAllOccurrences(chunkMkList, chunkMessageText, $"LW ({lastWeekStart:dd/MM} – {lastWeekEnd:dd/MM}) | LM ({lastMonthStart:dd/MM} – {lastMonthEnd:dd/MM})");

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
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while sending notification: " + ex.Message);
            }
        }

        private async Task<List<OfficeWorkingTopLWLMDto>> ListTopOfficeWorkingTimeLWLMInternal(List<long> officeIds, string searchText)
        {
            try
            {
                var now = DateTimeUtils.GetNow().Date;
                var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);
                var lastMonthStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
                var lastMonthEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));
                var minStart = lastWeekStart < lastMonthStart ? lastWeekStart : lastMonthStart;
                var maxEnd = lastWeekEnd > lastMonthEnd ? lastWeekEnd : lastMonthEnd;

                var users = await _workScope.GetAll<User>()
                    .WhereIf(officeIds != null && officeIds.Any(), u => u.BranchId.HasValue && officeIds.Contains(u.BranchId.Value))
                    .WhereIf(!string.IsNullOrEmpty(searchText), u =>
                        (u.FullName != null && u.FullName.ToLower().Contains(searchText)) ||
                        (u.UserName != null && u.UserName.ToLower().Contains(searchText)))
                    .Where(u => u.IsActive && !u.IsDeleted && !u.IsStopWork)
                    .Select(u => new UserLiteDto
                    {
                        Id = u.Id,
                        Name = u.FullName,
                        UserName = u.UserName,
                        MorningEndAt = u.MorningEndAt,
                        AfternoonStartAt = u.AfternoonStartAt,
                        Branch = new BranchToDisplayDto
                        {
                            Name = u.Branch.Name,
                            Code = u.Branch.Code,
                            Color = u.Branch.Color
                        }
                    })
                    .ToListAsync();

                if (!users.Any())
                    return new List<OfficeWorkingTopLWLMDto>();

                var userIds = users.Select(u => u.Id).ToList();

                var timekeepingData = await _workScope.GetAll<Timekeeping>()
                    .Where(t => t.DateAt >= minStart && t.DateAt <= maxEnd)
                    .Where(t => t.UserId.HasValue && userIds.Contains(t.UserId.Value))
                    .Select(t => new
                    {
                        UserId = t.UserId.Value,
                        t.DateAt,
                        t.CheckIn,
                        t.CheckOut,
                        t.TrackerTime
                    })
                    .ToListAsync();

                var remoteDetails = await _workScope.GetAll<AbsenceDayDetail>()
                    .Where(d => d.DateAt >= minStart && d.DateAt <= maxEnd)
                    .Join(_workScope.GetAll<AbsenceDayRequest>()
                        .Where(r => r.Status == RequestStatus.Approved
                            && r.Type == RequestType.Remote
                            && userIds.Contains(r.UserId)),
                        d => d.RequestId,
                        r => r.Id,
                        (d, r) => new
                        {
                            r.UserId,
                            d.DateAt,
                            d.DateType
                        })
                    .ToListAsync();

                var remoteMap = remoteDetails
                    .GroupBy(x => (x.UserId, x.DateAt.Date))
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().DateType
                    );

                var result = new Dictionary<long, OfficeWorkingTopLWLMDto>();
                var userMap = users.ToDictionary(u => u.Id, u => u);

                foreach (var tk in timekeepingData)
                {
                    if (!userMap.TryGetValue(tk.UserId, out var userInfo))
                        continue;

                    if (!result.TryGetValue(tk.UserId, out var userReport))
                    {
                        userReport = new OfficeWorkingTopLWLMDto
                        {
                            UserId = userInfo.Id,
                            FullName = userInfo.Name,
                            UserName = userInfo.UserName,
                            Branch = userInfo.Branch
                        };
                        result[tk.UserId] = userReport;
                    }

                    TimeSpan? checkInTs = TryParseTime(tk.CheckIn);
                    TimeSpan? checkOutTs = TryParseTime(tk.CheckOut);
                    TimeSpan? morningEndAtTs = TryParseTime(userInfo.MorningEndAt);
                    TimeSpan? afternoonStartAtTs = TryParseTime(userInfo.AfternoonStartAt);

                    var trackerMinutes = !string.IsNullOrWhiteSpace(tk.TrackerTime) &&
                                        TimeSpan.TryParse(tk.TrackerTime, out var ts)
                                        ? (int)ts.TotalMinutes
                                        : 0;
                    double trackerHours = trackerMinutes / 60.0;

                    var isRemoteRequest = remoteMap.TryGetValue((tk.UserId, tk.DateAt.Date), out var remoteDayType);

                    var computeInput = new ComputeWorkingHoursInputDto
                    {
                        CheckInTs = checkInTs,
                        CheckOutTs = checkOutTs,
                        MorningEndAtTs = morningEndAtTs,
                        AfternoonStartAtTs = afternoonStartAtTs,
                        TrackerHours = trackerHours,
                        IsRemoteRequest = isRemoteRequest,
                        RemoteDayType = isRemoteRequest ? (DayType?)remoteDayType : null
                    };

                    var (officeHours, wfhHours) = ComputeWorkingHours(computeInput);

                    double wfhThreshold = double.Parse(_settingManager.GetSettingValue(AppSettingNames.PercentOfTrackerOnWorking)) / 100;
                    double officeStandardHours = 8.0;
                    double targetWfhHours = officeStandardHours * wfhThreshold;

                    double totalHours = wfhHours > targetWfhHours
                                        ? wfhHours
                                        : (officeHours + wfhHours <= officeStandardHours
                                            ? (officeHours + wfhHours)
                                            : Math.Max(officeHours, wfhHours));

                    var isInLastWeek = tk.DateAt.Date >= lastWeekStart && tk.DateAt.Date <= lastWeekEnd;
                    var isInLastMonth = tk.DateAt.Date >= lastMonthStart && tk.DateAt.Date <= lastMonthEnd;

                    if (isInLastWeek)
                    {
                        userReport.TotalAllLW += totalHours;
                        userReport.WfhLW += wfhHours;
                        userReport.OfficeLW += officeHours;
                    }

                    if (isInLastMonth)
                    {
                        userReport.TotalAllLM += totalHours;
                        userReport.WfhLM += wfhHours;
                        userReport.OfficeLM += officeHours;
                    }
                }

                return result.Values
                    .OrderByDescending(x => x.TotalAllLW)
                    .ThenByDescending(x => x.TotalAllLM)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while generating the report: " + ex.Message);
            }
        }

        public async Task<PagedResultDto<OfficeWorkingTopLWLMDto>> GetOfficeWorkingTimelogReport(GridParam param, GetOfficeWorkingTimelogReportInputDto input)
        {
            try
            {
                string searchText = param?.SearchText?.Trim().ToLower();

                var officeWorkingData = await ListTopOfficeWorkingTimeLWLMInternal(input.BranchIds, searchText);

                IEnumerable<OfficeWorkingTopLWLMDto> orderedQuery;
                bool isDesc = (int) param.SortDirection == (int) ESortDirection.Desc;

                switch (param.Sort)
                {
                    case "FullName":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.FullName)
                            : officeWorkingData.OrderBy(u => u.FullName);
                        break;

                    case "BranchName":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.Branch.Name)
                            : officeWorkingData.OrderBy(u => u.Branch.Name);
                        break;

                    case "TotalAllLW":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.TotalAllLW)
                            : officeWorkingData.OrderBy(u => u.TotalAllLW);
                        break;

                    case "OfficeLW":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.OfficeLW)
                            : officeWorkingData.OrderBy(u => u.OfficeLW);
                        break;

                    case "WfhLW":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.WfhLW)
                            : officeWorkingData.OrderBy(u => u.WfhLW);
                        break;

                    case "TotalAllLM":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.TotalAllLM)
                            : officeWorkingData.OrderBy(u => u.TotalAllLM);
                        break;

                    case "OfficeLM":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.OfficeLM)
                            : officeWorkingData.OrderBy(u => u.OfficeLM);
                        break;

                    case "WfhLM":
                        orderedQuery = isDesc
                            ? officeWorkingData.OrderByDescending(u => u.WfhLM)
                            : officeWorkingData.OrderBy(u => u.WfhLM);
                        break;

                    default:
                        orderedQuery = officeWorkingData
                            .OrderByDescending(u => u.TotalAllLW)
                            .ThenByDescending(u => u.TotalAllLM)
                            .ThenBy(u => u.UserName);
                        break;
                }

                if (!string.IsNullOrEmpty(param.Sort) && param.Sort != "FullName")
                {
                    orderedQuery = ((IOrderedEnumerable<OfficeWorkingTopLWLMDto>)orderedQuery)
                                    .ThenBy(u => u.UserName);
                }

                var result = orderedQuery.ToList();

                var totalCount = result.Count;

                var pagedData = result
                    .Skip(param.SkipCount)
                    .Take(param.MaxResultCount)
                    .ToList();

                return new PagedResultDto<OfficeWorkingTopLWLMDto>(totalCount, pagedData);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while fetching the report: " + ex.Message);
            }
        }
    }
}