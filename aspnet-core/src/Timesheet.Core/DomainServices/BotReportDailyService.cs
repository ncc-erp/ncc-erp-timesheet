using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Word;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Timesheet.Services.Mezon;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class BotReportDailyService : BaseDomainService, IBotReportDailyService, ITransientDependency
    {

        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;
        private readonly MezonService _mezonService;
        const double MinutesPerHour = 60.0;

        public BotReportDailyService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager) : base(workScope)
        {
            _workScope = workScope;
            _settingManager = settingManager;
            _mezonService = mezonService;
        }

        public async Task<List<TotalTimelogProjectDto>> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportInput input, string searchText)
        {
            try
            {
                var now = DateTimeUtils.GetNow().Date;
                var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);
                var lastMonthStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
                var lastMonthEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));
                var minStart = lastWeekStart < lastMonthStart ? lastWeekStart : lastMonthStart;
                var maxEnd = lastWeekEnd > lastMonthEnd ? lastWeekEnd : lastMonthEnd;
                var inputBranchIds = input.BranchIds ?? new List<long>();

                var officeUsers = await _workScope.GetAll<User>()
                    .WhereIf(!input.IsAllBranch && inputBranchIds.Any(), u => u.BranchId.HasValue && inputBranchIds.Contains(u.BranchId.Value))
                    .Where(u => !u.IsDeleted && u.IsActive && !u.IsStopWork && u.BranchId.HasValue)
                    .Select(u => new { u.Id, u.FullName })
                    .ToListAsync();

                var officeUserDict = officeUsers.ToDictionary(u => u.Id, u => u.FullName);

                if (!officeUserDict.Any())
                {
                    return new List<TotalTimelogProjectDto>();
                }

                var activeProjects = await _workScope.GetAll<Project>()
                    .WhereIf(!string.IsNullOrEmpty(searchText), p => p.Name != null && p.Name.ToLower().Contains(searchText))
                    .WhereIf(input.ProjectIds != null && input.ProjectIds.Any(), p => input.ProjectIds.Contains(p.Id))
                    .Where(p => !p.IsDeleted && p.Status == ProjectStatus.Active)
                    .Select(p => new { p.Id, p.Name, p.Code })
                    .ToListAsync();

                var activeProjectDict = activeProjects.ToDictionary(p => p.Id, p => new { p.Name, p.Code });

                var myTimesheetData = await _workScope.GetAll<MyTimesheet>()
                    .Where(mt => officeUsers.Select(u => u.Id).Contains(mt.UserId))
                    .Where(mt => mt.DateAt >= minStart && mt.DateAt <= maxEnd)
                    .Where(mt => mt.Status == TimesheetStatus.Approve)
                    .Where(mt => mt.ProjectTask != null)
                    .Where(mt => mt.ProjectTask.Project != null)
                    .Where(mt => !mt.ProjectTask.Project.IsDeleted)
                    .Where(mt => activeProjects.Select(p => p.Id).Contains(mt.ProjectTask.ProjectId))
                    .Select(mt => new
                    {
                        mt.UserId,
                        mt.DateAt,
                        mt.WorkingTime,
                        mt.ProjectTask.ProjectId
                    })
                    .ToListAsync();

                var result = myTimesheetData
                    .GroupBy(mt => mt.ProjectId)
                    .Select(g => 
                    {
                        var projectId = g.Key;
                        var projectName = "";
                        if (activeProjectDict.TryGetValue(projectId, out var projectInfo))
                        {
                            projectName = !string.IsNullOrEmpty(projectInfo.Name) ? projectInfo.Name : (!string.IsNullOrEmpty(projectInfo.Code) ? projectInfo.Code : "");
                        }
                        return new TotalTimelogProjectDto
                        {
                            Name = projectName,
                            Members = g.Select(mt => mt.UserId)
                                .Distinct()
                                .Select(userId => officeUserDict[userId])
                                .OrderBy(name => name)
                                .ToList(),
                            TotalTimelogLW = Math.Round(g.Sum(mt => mt.DateAt >= lastWeekStart && mt.DateAt <= lastWeekEnd ? mt.WorkingTime : 0) / 60.0, 2),
                            TotalTimelogLM = Math.Round(g.Sum(mt => mt.DateAt >= lastMonthStart && mt.DateAt <= lastMonthEnd ? mt.WorkingTime : 0) / 60.0, 2)
                        };
                    })
                    .Where(p => p.TotalTimelogLW >= (input.MinHours ?? 0))
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while generating the report: ", ex);
            }
        }

        public async Task<PagedResultDto<TotalTimelogProjectDto>> GetPagedDailyProjectTimelogReport(GridParam param, GetDailyProjectTimelogReportInput input)
        {
            try
            {
                string searchText = param?.SearchText?.Trim().ToLower();

                var allData = await GetDailyProjectTimelogReport(input, searchText);
                IEnumerable<TotalTimelogProjectDto> orderedQuery = allData;
                bool isDesc = (int) param.SortDirection == (int) ESortDirection.Desc;

                switch (param.Sort)
                {
                    case "ProjectName":
                        orderedQuery = isDesc
                            ? allData.OrderByDescending(p => p.Name)
                            : allData.OrderBy(p => p.Name);
                        break;

                    case "MemberCount":
                        orderedQuery = isDesc
                            ? allData.OrderByDescending(p => p.Members.Count)
                            : allData.OrderBy(p => p.Members.Count);
                        break;

                    case "TotalTimelogLW":
                        orderedQuery = isDesc
                            ? allData.OrderByDescending(p => p.TotalTimelogLW)
                            : allData.OrderBy(p => p.TotalTimelogLW);
                        break;

                    case "TotalTimelogLM":
                        orderedQuery = isDesc
                            ? allData.OrderByDescending(p => p.TotalTimelogLM)
                            : allData.OrderBy(p => p.TotalTimelogLM);
                        break;

                    default:
                        orderedQuery = allData.OrderByDescending(p => p.TotalTimelogLW).ThenByDescending(p => p.TotalTimelogLM);
                        break;
                }

                if (!string.IsNullOrEmpty(param.Sort) && param.Sort != "ProjectName")
                {
                    orderedQuery = ((IOrderedEnumerable<TotalTimelogProjectDto>)orderedQuery)
                                    .ThenBy(p => p.Name);
                }

                var limitedData = orderedQuery.ToList();
                var totalCount = limitedData.Count;
                var pagedData = limitedData
                    .Skip(param.SkipCount)
                    .Take(param.MaxResultCount)
                    .ToList();
                return new PagedResultDto<TotalTimelogProjectDto>(totalCount, pagedData);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while generating the paged report: ", ex);
            }
        }

        public async Task<bool> SendDailyProjectTimelogToMezon()
        {
            try
            {
                var branchCodesJson = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportBranchCodes);
                var minHoursStr = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportMinHours);
                var topNStr = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportTopN);
                var projectIdsJson = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportProjectIds);

                List<long> projectIds = null;
                if (!string.IsNullOrEmpty(projectIdsJson))
                {
                    try
                    {
                        projectIds = JsonConvert.DeserializeObject<List<long>>(projectIdsJson);
                    }
                    catch
                    {
                        Logger.Error($"Failed to deserialize project IDs: {projectIdsJson}");
                    }
                }

                List<string> branchCodes = null;
                if (!string.IsNullOrEmpty(branchCodesJson))
                {
                    try
                    {
                        branchCodes = JsonConvert.DeserializeObject<List<string>>(branchCodesJson);
                    }
                    catch
                    {
                        Logger.Error($"Failed to deserialize branch codes: {branchCodesJson}");
                    }
                }

                var input = new GetDailyProjectTimelogReportByBranchCodesInput
                {
                    BranchCodes = branchCodes ?? new List<string>(),
                    MinHours = double.TryParse(minHoursStr, out var minHours) ? (double?)minHours : null,
                    TopN = int.TryParse(topNStr, out var topN) ? (int?)topN : null,
                    ProjectIds = projectIds
                };

                if (input.BranchCodes == null || !input.BranchCodes.Any())
                {
                    Logger.Warn("No branch selection criteria specified for Bot Report. Report may be empty.");
                }

                return await SendDailyProjectTimelogToMezon(input);
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred while sending the daily project timelog report to Mezon: ", ex);
                return false;
            }
        }

        private static (DateTime start, DateTime end) GetLastWeekRange(DateTime reportDate)
        {
            var thisWeekStart = DateTimeUtils.FirstDayOfWeek(reportDate);
            var lastWeekStart = thisWeekStart.AddDays(-7).Date;
            var lastWeekEnd = thisWeekStart.AddDays(-1).Date.AddDays(1).AddTicks(-1);
            return (lastWeekStart, lastWeekEnd);
        }
        public async Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportByBranchCodesInput input)
        {
            var now = DateTimeUtils.GetNow().Date;
            var (lastWeekStart, lastWeekEnd) = GetLastWeekRange(now);
            var lastMonthStart = DateTimeUtils.FirstDayOfMonth(now.AddMonths(-1));
            var lastMonthEnd = DateTimeUtils.LastDayOfMonth(now.AddMonths(-1));

            List<long> branchIds = new List<long>();
            string branchInfo = "Unknown Branch";

            if (input.BranchCodes != null && input.BranchCodes.Any())
            {
                var validCodes = input.BranchCodes.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
                if (validCodes.Any())
                {
                    var totalBranchCount = await _workScope.GetAll<Timesheet.Entities.Branch>().CountAsync();

                    var branches = await _workScope.GetAll<Timesheet.Entities.Branch>()
                        .Where(b => validCodes.Contains(b.Code))
                        .Select(b => new { b.Id, b.DisplayName })
                        .ToListAsync();

                    branchIds = branches.Select(b => b.Id).ToList();

                    if (!branchIds.Any())
                    {
                        Logger.Warn($"No valid branch codes found: {string.Join(", ", validCodes)}. Report will include all branches.");
                        branchInfo = string.Join(", ", input.BranchCodes);
                    }
                    else
                    {
                        if (branches.Count == totalBranchCount)
                        {
                            branchInfo = "All Branches";
                        }
                        else
                        {
                            branchInfo = string.Join(", ", branches.Select(b => b.DisplayName));
                        }
                    }
                }
                else
                {
                    branchInfo = string.Join(", ", input.BranchCodes);
                }
            }

            var reportInput = new GetDailyProjectTimelogReportInput
            {
                BranchIds = branchIds,
                MinHours = input.MinHours,
                ProjectIds = input.ProjectIds ?? new List<long>()
            };

            var reportData = await GetDailyProjectTimelogReport(reportInput, null);

            var webhookUrl = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportWebhookUrl);

            if (string.IsNullOrEmpty(webhookUrl))
            {
                return false;
            }

            var projectData = reportData
                .OrderByDescending(x => x.TotalTimelogLW)
                .Take(input.TopN ?? 10)
                .Select(x => new
                {
                    Project = x.Name.Trim(),
                    Members = string.Join(", ", x.Members),
                    LastWeekHours = $"{x.TotalTimelogLW:0.0}h",
                    LastMonthHours = $"{x.TotalTimelogLM:0.0}h"
                })
                .Cast<dynamic>()
                .ToList();

            const int BATCH_SIZE = 5;
            const int MESSAGE_DELAY_MS = 1000;

            var headerSb = new StringBuilder();
            string lastWeekPeriod = $"{lastWeekStart:dd/MM/yyyy} - {lastWeekEnd:dd/MM/yyyy}";
            string lastMonthPeriod = $"{lastMonthStart:MM/yyyy}";

            headerSb.AppendLine("════════════ Top Projects Summary ════════════");
            headerSb.AppendLine($"Report Period:   Last Week  ({lastWeekPeriod}) |  Last Month  ({lastMonthPeriod})");
            headerSb.AppendLine($"Office: {branchInfo}");
            headerSb.AppendLine("════════════════════════════════════");

            var headerMessageText = headerSb.ToString();
            var headerMkList = CreateMarkupList(headerMessageText);

            _mezonService.Post(webhookUrl, new
            {
                type = "hook",
                message = new
                {
                    t = headerMessageText,
                    mk = headerMkList
                }
            });

            await System.Threading.Tasks.Task.Delay(MESSAGE_DELAY_MS);

            if (projectData.Any())
            {
                var chunks = CommonUtils.SplitIntoChunks(projectData.Cast<dynamic>().ToList(), BATCH_SIZE);
                int idx = 1;
                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    bool isLastChunk = i == chunks.Count - 1;

                    var chunkSb = new StringBuilder();
                    var chunkMkList = new List<object>();

                    foreach (var item in chunk)
                    {
                        var itemSb = new StringBuilder();
                        itemSb.AppendLine($"{idx}. Project:  {item.Project}");
                        itemSb.AppendLine($" Members:  {item.Members}");
                        itemSb.AppendLine($" Last Week:  {item.LastWeekHours}");
                        itemSb.AppendLine($" Last Month:  {item.LastMonthHours}");
                        itemSb.AppendLine("════════════════════════════════════");

                        string itemText = itemSb.ToString();
                        int itemStartPos = chunkSb.Length;
                        chunkSb.Append(itemText);

                        int projectPos = itemText.IndexOf($"{idx}. Project:", 0);
                        int membersPos = itemText.IndexOf("Members:", projectPos);
                        int lastWeekPos = itemText.IndexOf("Last Week:", membersPos);
                        int lastMonthPos = itemText.IndexOf("Last Month:", lastWeekPos);

                        chunkMkList.Add(new { type = "b", s = itemStartPos + projectPos, e = itemStartPos + projectPos + $"{idx}. Project:".Length });
                        chunkMkList.Add(new { type = "b", s = itemStartPos + membersPos, e = itemStartPos + membersPos + "Members:".Length });
                        chunkMkList.Add(new { type = "b", s = itemStartPos + lastWeekPos, e = itemStartPos + lastWeekPos + "Last Week:".Length });
                        chunkMkList.Add(new { type = "b", s = itemStartPos + lastMonthPos, e = itemStartPos + lastMonthPos + "Last Month:".Length });

                        idx++;
                    }

                    var chunkMessageText = chunkSb.ToString();
                    if (!string.IsNullOrEmpty(chunkMessageText))
                    {
                        _mezonService.Post(webhookUrl, new
                        {
                            type = "hook",
                            message = new
                            {
                                t = chunkMessageText,
                                mk = chunkMkList
                            }
                        });
                    }

                    await System.Threading.Tasks.Task.Delay(MESSAGE_DELAY_MS);
                }
            }
            else
            {
                var emptySb = new StringBuilder();
                emptySb.AppendLine("No projects found in the projects summary");
                emptySb.AppendLine("═══════════════════════════════════════");

                var emptyMessageText = emptySb.ToString();
                var emptyMkList = new List<object>();

                int titlePos = emptyMessageText.IndexOf("═══ Projects Summary ═══");
                if (titlePos >= 0)
                {
                    emptyMkList.Add(new { type = "b", s = titlePos, e = titlePos + "═══ Projects Summary ═══".Length });
                }

                _mezonService.Post(webhookUrl, new
                {
                    type = "hook",
                    message = new
                    {
                        t = emptyMessageText,
                        mk = emptyMkList
                    }
                });
            }

            return true;
        }

        private List<object> CreateMarkupList(string messageText)
        {
            var mkList = new List<object>();

            string titleToFind = "Top Projects Summary";
            int titlePos = messageText.IndexOf(titleToFind);
            if (titlePos >= 0)
            {
                mkList.Add(new { type = "b", s = titlePos, e = titlePos + titleToFind.Length });
            }

            string officeText = "Office:";
            int officePos = messageText.IndexOf(officeText);
            if (officePos >= 0)
            {
                mkList.Add(new { type = "b", s = officePos, e = officePos + officeText.Length });
            }

            string reportPeriodText = "Report Period:";
            int reportPeriodPos = messageText.IndexOf(reportPeriodText);
            if (reportPeriodPos >= 0)
            {
                mkList.Add(new { type = "b", s = reportPeriodPos, e = reportPeriodPos + reportPeriodText.Length });
            }

            string lastWeekText = "Last Week";
            int lastWeekPeriodPos = messageText.IndexOf(lastWeekText, reportPeriodPos);
            if (lastWeekPeriodPos >= 0)
            {
                mkList.Add(new { type = "b", s = lastWeekPeriodPos, e = lastWeekPeriodPos + lastWeekText.Length });
            }

            string lastMonthText = "Last Month";
            int lastMonthPeriodPos = messageText.IndexOf(lastMonthText, reportPeriodPos);
            if (lastMonthPeriodPos >= 0)
            {
                mkList.Add(new { type = "b", s = lastMonthPeriodPos, e = lastMonthPeriodPos + lastMonthText.Length });
            }

            return mkList;
        }

    }
}