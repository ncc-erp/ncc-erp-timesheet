using Abp.Configuration;
using Abp.Dependency;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class BotReportDailyService : BaseDomainService, IBotReportDailyService, ITransientDependency
    {

        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;
        private readonly MezonService _mezonService;

        public BotReportDailyService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager) : base(workScope)
        {
            _workScope = workScope;
            _settingManager = settingManager;
            _mezonService = mezonService;
        }

        public async Task<DailyProjectTimelogReportDto> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportInput input)
        {
            var today = DateTime.Now.Date;

            var lastWeekEnd = today.AddDays(-(int)today.DayOfWeek);
            if (lastWeekEnd == today)
            {
                lastWeekEnd = today.AddDays(-7);
            }
            var lastWeekStart = lastWeekEnd.AddDays(-6);

            var lastMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);
            var lastMonthStart = new DateTime(lastMonthEnd.Year, lastMonthEnd.Month, 1);

            var allBranches = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Select(b => new { b.Id, b.Code, b.DisplayName })
                .ToListAsync();

            var allUsers = await _workScope.GetAll<User>()
                .Where(u => !u.IsDeleted)
                .Select(u => new { u.Id, u.UserName, u.BranchId, u.IsActive })
                .ToListAsync();

            var allProjects = await _workScope.GetAll<Project>()
                .Where(p => !p.IsDeleted && p.Status == Ncc.Entities.Enum.StatusEnum.ProjectStatus.Active)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            var branchNames = new List<string>();
            List<long> officeUsers;

            bool isAllBranchesSelected = input.BranchCodes.Count == allBranches.Count &&
                                         input.BranchCodes.All(code => allBranches.Select(b => b.Code).Contains(code));

            if (isAllBranchesSelected)
            {
                officeUsers = allUsers
                    .Where(u => u.IsActive && u.BranchId.HasValue)
                    .Select(u => u.Id)
                    .ToList();
                branchNames.Add("All Branches");
            }
            else
            {
                var branchDict = allBranches
                    .Where(b => input.BranchCodes.Contains(b.Code))
                    .ToDictionary(b => b.Id, b => b.DisplayName);

                if (!branchDict.Any())
                {
                    throw new UserFriendlyException("Invalid branch codes provided. None of the specified branch codes exist in the system.");
                }

                officeUsers = allUsers
                    .Where(u => u.IsActive && u.BranchId.HasValue && branchDict.ContainsKey(u.BranchId.Value))
                    .Select(u => u.Id)
                    .ToList();

                branchNames.AddRange(branchDict.Values);
            }

            if (!officeUsers.Any())
            {
                return new DailyProjectTimelogReportDto
                {
                    ReportDate = today.ToString("yyyy-MM-dd"),
                    Projects = new List<ProjectTimelogDto>()
                };
            }

            var activeProjectIds = (input.ProjectIds != null && input.ProjectIds.Any())
                ? allProjects.Where(p => input.ProjectIds.Contains(p.Id)).Select(p => p.Id).ToList()
                : allProjects.Select(p => p.Id).ToList();

            var myTimesheetData = await _workScope.GetAll<MyTimesheet>()
                .Where(mt => officeUsers.Contains(mt.UserId) &&
                       ((mt.DateAt >= lastWeekStart && mt.DateAt <= lastWeekEnd) ||
                        (mt.DateAt >= lastMonthStart && mt.DateAt <= lastMonthEnd)) &&
                       mt.Status == Ncc.Entities.Enum.StatusEnum.TimesheetStatus.Approve &&
                       mt.ProjectTask != null &&
                       mt.ProjectTask.Project != null &&
                       !mt.ProjectTask.Project.IsDeleted &&
                       activeProjectIds.Contains(mt.ProjectTask.ProjectId))
                .Select(mt => new
                {
                    mt.UserId,
                    mt.DateAt,
                    mt.WorkingTime,
                    ProjectId = mt.ProjectTask.ProjectId
                })
                .ToListAsync();

            var projectTimesheets = myTimesheetData
                .GroupBy(mt => mt.ProjectId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    TotalMinutesLW = g.Where(mt => mt.DateAt >= lastWeekStart && mt.DateAt <= lastWeekEnd).Sum(mt => mt.WorkingTime),
                    TotalMinutesLM = g.Where(mt => mt.DateAt >= lastMonthStart && mt.DateAt <= lastMonthEnd).Sum(mt => mt.WorkingTime),
                    UserIds = g.Select(mt => mt.UserId).Distinct().ToList()
                })
                .ToList();

            var result = new DailyProjectTimelogReportDto
            {
                ReportDate = today.ToString("yyyy-MM-dd"),
                Projects = projectTimesheets
                    .Select(p => new ProjectTimelogDto
                    {
                        Name = allProjects.FirstOrDefault(proj => proj.Id == p.ProjectId)?.Name ?? "Unknown Project",
                        Members = allUsers
                            .Where(u => p.UserIds.Contains(u.Id))
                            .Select(u => u.UserName)
                            .OrderBy(name => name)
                            .ToList(),
                        TotalTimelogLW = Math.Round(p.TotalMinutesLW / 60.0, 2),
                        TotalTimelogLM = Math.Round(p.TotalMinutesLM / 60.0, 2)
                    })
                    .Where(p => p.TotalTimelogLW >= (input.MinHours ?? 0))
                    .OrderByDescending(p => p.TotalTimelogLW)
                    .ThenByDescending(p => p.TotalTimelogLM)
                    .ThenBy(p => p.Name)
                    .ToList()
            };

            if (input.TopN.HasValue && input.TopN.Value > 0 && result.Projects.Count > input.TopN.Value)
            {
                result.Projects = result.Projects.Take(input.TopN.Value).ToList();
            }

            return result;
        }

        public async Task<bool> SendDailyProjectTimelogToMezon()
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

            var input = new GetDailyProjectTimelogReportInput
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

        public async Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportInput input)
        {
            var today = DateTime.Now.Date;

            var lastWeekEnd = today.AddDays(-(int)today.DayOfWeek);
            if (lastWeekEnd == today)
            {
                lastWeekEnd = today.AddDays(-7);
            }
            var lastWeekStart = lastWeekEnd.AddDays(-6);

            var lastMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);
            var lastMonthStart = new DateTime(lastMonthEnd.Year, lastMonthEnd.Month, 1);

            var reportData = await GetDailyProjectTimelogReport(input);

            var webhookUrl = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportWebhookUrl);

            if (string.IsNullOrEmpty(webhookUrl))
            {
                return false;
            }

            var projectData = reportData.Projects
                .OrderByDescending(x => x.TotalTimelogLW)
                .Take(input.TopN ?? 10)
                .Select(x => new
                {
                    Project = x.Name.Trim(),
                    Members = string.Join(", ", x.Members),
                    LastWeekHours = $"{x.TotalTimelogLW:0.0}h",
                    LastMonthHours = $"{x.TotalTimelogLM:0.0}h"
                })
                .ToList();

            var sb = new System.Text.StringBuilder();

            string lastWeekPeriod = $"{lastWeekStart:dd/MM/yyyy} - {lastWeekEnd:dd/MM/yyyy}";
            string lastMonthPeriod = $"{lastMonthStart:MM/yyyy}";

            string branchInfo;

            if (input.BranchCodes != null && input.BranchCodes.Any())
            {
                var allBranches = await _workScope.GetAll<Timesheet.Entities.Branch>()
                    .Select(b => new { b.Code, b.DisplayName })
                    .ToListAsync();

                var allBranchCodes = new HashSet<string>(allBranches.Select(b => b.Code));
                var branchDict = allBranches.ToDictionary(b => b.Code, b => b.DisplayName);

                bool isAllBranchesSelected =
                    input.BranchCodes.Count == allBranchCodes.Count &&
                    input.BranchCodes.All(code => allBranchCodes.Contains(code));

                if (isAllBranchesSelected)
                {
                    branchInfo = "All Branches";
                }
                else
                {
                    var matchedBranches = input.BranchCodes
                        .Where(code => branchDict.ContainsKey(code))
                        .Select(code => branchDict[code])
                        .ToList();

                    branchInfo = matchedBranches.Any()
                        ? string.Join(", ", matchedBranches)
                        : string.Join(", ", input.BranchCodes);
                }
            }
            else
            {
                branchInfo = "Unknown Branch";
            }

            sb.AppendLine("📊 Top Projects Summary");
            sb.AppendLine($"Report Period:   Last Week  ({lastWeekStart:MM/dd/yyyy} - {lastWeekEnd:MM/dd/yyyy}) |  Last Month  ({lastMonthStart:MM/yyyy})");
            sb.AppendLine($"Office: {branchInfo}");
            sb.AppendLine("-------------------------------------");

            foreach (var item in projectData)
            {
                sb.AppendLine($" Project:  {item.Project}");
                sb.AppendLine($" Members:  {item.Members}");
                sb.AppendLine($" Last Week:  {item.LastWeekHours}");
                sb.AppendLine($" Last Month:  {item.LastMonthHours}");
                sb.AppendLine("-------------------------------------");
            }

            if (projectData.Any())
            {
                sb.Length -= "-------------------------------------\r\n".Length;
            }

            var messageText = sb.ToString();

            Console.WriteLine(messageText);

            var mkList = new List<object>();

            string titleToFind = "Top Projects Summary";
            int titlePos = messageText.IndexOf(titleToFind);

            if (titlePos >= 0)
            {
                mkList.Add(new
                {
                    type = "b",
                    s = titlePos,
                    e = titlePos + titleToFind.Length
                });
            }

            string officeText = "Office:";
            int officePos = messageText.IndexOf(officeText);
            if (officePos >= 0)
            {
                mkList.Add(new
                {
                    type = "b",
                    s = officePos,
                    e = officePos + officeText.Length
                });
            }

            string reportPeriodText = "Report Period:";
            int reportPeriodPos = messageText.IndexOf(reportPeriodText);
            if (reportPeriodPos >= 0)
            {
                mkList.Add(new
                {
                    type = "b",
                    s = reportPeriodPos,
                    e = reportPeriodPos + reportPeriodText.Length
                });
            }

            string lastWeekText = "Last Week";
            int lastWeekPeriodPos = messageText.IndexOf(lastWeekText, reportPeriodPos);
            if (lastWeekPeriodPos >= 0)
            {
                mkList.Add(new
                {
                    type = "b",
                    s = lastWeekPeriodPos,
                    e = lastWeekPeriodPos + lastWeekText.Length
                });
            }

            string lastMonthText = "Last Month";
            int lastMonthPeriodPos = messageText.IndexOf(lastMonthText, reportPeriodPos);
            if (lastMonthPeriodPos >= 0)
            {
                mkList.Add(new
                {
                    type = "b",
                    s = lastMonthPeriodPos,
                    e = lastMonthPeriodPos + lastMonthText.Length
                });
            }

            int currentPos = 0;
            while (true)
            {
                int projectPos = messageText.IndexOf("Project:", currentPos);
                int membersPos = messageText.IndexOf("Members:", currentPos);
                int lastWeekPos = messageText.IndexOf("Last Week:", currentPos);
                int lastMonthPos = messageText.IndexOf("Last Month:", currentPos);

                if (projectPos == -1) break;

                mkList.Add(new
                {
                    type = "b",
                    s = projectPos,
                    e = projectPos + "Project:".Length
                });

                mkList.Add(new
                {
                    type = "b",
                    s = membersPos,
                    e = membersPos + "Members:".Length
                });

                mkList.Add(new
                {
                    type = "b",
                    s = lastWeekPos,
                    e = lastWeekPos + "Last Week:".Length
                });

                mkList.Add(new
                {
                    type = "b",
                    s = lastMonthPos,
                    e = lastMonthPos + "Last Month:".Length
                });

                currentPos = lastMonthPos + "Last Month:".Length;
            }

            _mezonService.Post(webhookUrl, new
            {
                type = "hook",
                message = new
                {
                    t = messageText,
                    mk = mkList
                }
            });

            return true;
        }
    }
}
