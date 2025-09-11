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

            var branch = await _workScope.GetAll<Timesheet.Entities.Branch>()
                .Where(b => b.Code == input.BranchCode)
                .FirstOrDefaultAsync();

            if (branch == null)
            {
                throw new UserFriendlyException($"Branch with code '{input.BranchCode}' not found");
            }

            var officeUsers = await _workScope.GetAll<User>()
                .Where(u => u.BranchId == branch.Id && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();

            if (!officeUsers.Any())
            {
                return new DailyProjectTimelogReportDto
                {
                    ReportDate = today.ToString("yyyy-MM-dd"),
                    Projects = new List<ProjectTimelogDto>()
                };
            }

            var projectQuery = _workScope.GetAll<Project>()
                .Where(p => !p.IsDeleted && p.Status == Ncc.Entities.Enum.StatusEnum.ProjectStatus.Active);

            if (input.ProjectIds != null && input.ProjectIds.Any())
            {
                projectQuery = projectQuery.Where(p => input.ProjectIds.Contains(p.Id));
            }

            var allProjects = await projectQuery
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            var activeProjectIds = allProjects.Select(p => p.Id).ToList();

            var timekeepingRecords = await _workScope.GetAll<Timekeeping>()
                .Where(t => officeUsers.Contains(t.UserId.Value) &&
                           ((t.DateAt >= lastWeekStart && t.DateAt <= lastWeekEnd) ||
                            (t.DateAt >= lastMonthStart && t.DateAt <= lastMonthEnd)))
                .ToListAsync();

            var projectUsers = await _workScope.GetAll<ProjectUser>()
                .Where(pu => !pu.IsDeleted &&
                       pu.Type != ProjectUserType.DeActive &&
                       activeProjectIds.Contains(pu.ProjectId))
                .ToListAsync();

            var allUsers = await _workScope.GetAll<User>()
                .Where(u => !u.IsDeleted)
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            var absenceRecords = await _workScope.GetAll<AbsenceDayDetail>()
                .Include(s => s.Request)
                .Where(s => officeUsers.Contains(s.Request.UserId) &&
                       ((s.DateAt >= lastWeekStart && s.DateAt <= lastWeekEnd) ||
                        (s.DateAt >= lastMonthStart && s.DateAt <= lastMonthEnd)) &&
                       s.Request.Status == Ncc.Entities.Enum.StatusEnum.RequestStatus.Approved)
                .Select(s => new
                {
                    UserId = s.Request.UserId,
                    DateAt = s.DateAt,
                    DateType = s.DateType,
                    AbsenceTime = s.AbsenceTime,
                    Type = s.Request.Type
                })
                .ToListAsync();

            var myTimesheetData = await _workScope.GetAll<MyTimesheet>()
                .Where(mt => officeUsers.Contains(mt.UserId) &&
                       ((mt.DateAt >= lastWeekStart && mt.DateAt <= lastWeekEnd) ||
                        (mt.DateAt >= lastMonthStart && mt.DateAt <= lastMonthEnd)) &&
                       mt.Status == Ncc.Entities.Enum.StatusEnum.TimesheetStatus.Approve)
                .Include(mt => mt.ProjectTask)
                .ToListAsync();

            var userProjectMap = myTimesheetData
                .Where(mt => mt.ProjectTask != null && activeProjectIds.Contains(mt.ProjectTask.ProjectId))
                .GroupBy(mt => new { mt.UserId, mt.DateAt })
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(mt => mt.ProjectTask.ProjectId)
                         .Select(pg => new
                         {
                             ProjectId = pg.Key,
                             TotalMinutes = pg.Sum(mt => mt.WorkingTime)
                         })
                         .OrderByDescending(p => p.TotalMinutes)
                         .FirstOrDefault()?.ProjectId
                );

            var timekeepingMinutes = new List<(long UserId, DateTime DateAt, long? ProjectId, double Minutes)>();

            foreach (var record in timekeepingRecords)
            {
                if (!record.UserId.HasValue) continue;

                var userDateKey = new { UserId = record.UserId.Value, DateAt = record.DateAt };
                var mostLoggedProjectId = userProjectMap.ContainsKey(userDateKey) ? userProjectMap[userDateKey] : null;

                if (mostLoggedProjectId == null)
                {
                    var userProject = projectUsers.FirstOrDefault(pu =>
                        pu.UserId == record.UserId.Value &&
                        record.DateAt >= pu.CreationTime &&
                        (pu.IsDeleted == false || record.DateAt <= pu.DeletionTime));

                    if (userProject != null)
                    {
                        mostLoggedProjectId = userProject.ProjectId;
                    }
                }

                if (mostLoggedProjectId != null)
                {
                    double minutes = 0;
                    bool isRemote = false;

                    var remoteRequests = absenceRecords
                        .Where(r => r.Type == RequestType.Remote && r.UserId == record.UserId.Value && r.DateAt.Date == record.DateAt.Date)
                        .ToList();
                    isRemote = remoteRequests.Any();

                    if (isRemote)
                    {
                        if (!string.IsNullOrEmpty(record.TrackerTime) && TimeSpan.TryParse(record.TrackerTime, out TimeSpan trackerTimeSpan))
                        {
                            minutes = trackerTimeSpan.TotalMinutes;
                        }
                    }
                    else if (string.IsNullOrEmpty(record.CheckOut) && !string.IsNullOrEmpty(record.CheckIn) &&
                             !string.IsNullOrEmpty(record.TrackerTime) && TimeSpan.TryParse(record.TrackerTime, out TimeSpan trackerTimeSpan))
                    {
                        minutes = trackerTimeSpan.TotalMinutes;
                    }
                    else if (!string.IsNullOrEmpty(record.CheckIn) && !string.IsNullOrEmpty(record.CheckOut))
                    {
                        if (TimeSpan.TryParse(record.CheckIn, out TimeSpan checkInTime) &&
                            TimeSpan.TryParse(record.CheckOut, out TimeSpan checkOutTime))
                        {
                            minutes = (checkOutTime - checkInTime).TotalMinutes;

                            if (minutes > 240)
                            {
                                minutes -= 60;
                            }
                        }
                    }

                    if (minutes > 0)
                    {
                        timekeepingMinutes.Add((record.UserId.Value, record.DateAt, mostLoggedProjectId, minutes));
                    }
                }
            }

            var projectUserMap = myTimesheetData
                .Where(mt => mt.ProjectTask != null && activeProjectIds.Contains(mt.ProjectTask.ProjectId))
                .GroupBy(mt => mt.ProjectTask.ProjectId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(mt => mt.UserId).Distinct().ToList()
                );

            var validUserDateProjectCombinations = new HashSet<(long UserId, DateTime DateAt, long ProjectId)>();
            foreach (var userDateKey in userProjectMap)
            {
                if (userDateKey.Value.HasValue)
                {
                    validUserDateProjectCombinations.Add((userDateKey.Key.UserId, userDateKey.Key.DateAt, userDateKey.Value.Value));
                }
            }

            var filteredTimekeepingMinutes = timekeepingMinutes
                .Where(t => t.ProjectId.HasValue && validUserDateProjectCombinations.Contains((t.UserId, t.DateAt, t.ProjectId.Value)))
                .ToList();

            var projectTimelogData = filteredTimekeepingMinutes
                .GroupBy(t => t.ProjectId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    TotalMinutesLW = g.Where(t => t.DateAt >= lastWeekStart && t.DateAt <= lastWeekEnd).Sum(t => t.Minutes),
                    TotalMinutesLM = g.Where(t => t.DateAt >= lastMonthStart && t.DateAt <= lastMonthEnd).Sum(t => t.Minutes),
                    UserIds = g.Key.HasValue && projectUserMap.ContainsKey(g.Key.Value) ?
                              projectUserMap[g.Key.Value].Intersect(g.Select(t => t.UserId).Distinct()).ToList() :
                              new List<long>()
                })
                .Where(p => p.ProjectId.HasValue)
                .ToList();

            var result = new DailyProjectTimelogReportDto
            {
                ReportDate = today.ToString("yyyy-MM-dd"),
                Projects = projectTimelogData
                    .Select(p => new ProjectTimelogDto
                    {
                        Name = allProjects.FirstOrDefault(proj => proj.Id == p.ProjectId)?.Name ?? "Unknown Project",
                        Members = allUsers
                            .Where(u => p.UserIds.Contains(u.Id))
                            .Select(u => u.UserName)
                            .OrderBy(name => name)
                            .ToList(),
                        TotalTimelogLW = Math.Round(p.TotalMinutesLW / 60, 2),
                        TotalTimelogLM = Math.Round(p.TotalMinutesLM / 60, 2)
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
            var branchCodeStr = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportBranchCode);
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

            var input = new GetDailyProjectTimelogReportInput
            {
                BranchCode = branchCodeStr,
                MinHours = double.TryParse(minHoursStr, out var minHours) ? (double?)minHours : null,
                TopN = int.TryParse(topNStr, out var topN) ? (int?)topN : null,
                ProjectIds = projectIds
            };
            
            if (string.IsNullOrEmpty(input.BranchCode))
            {
                Logger.Warn("No branch code specified for Bot Report. Report may be empty.");
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
            if (!string.IsNullOrEmpty(input.BranchCode))
            {
                var branch = await _workScope.GetAll<Timesheet.Entities.Branch>()
                    .Where(b => b.Code == input.BranchCode)
                    .FirstOrDefaultAsync();
                
                if (branch != null)
                {
                    branchInfo = $"{branch.DisplayName}";
                }
                else
                {
                    branchInfo = $"Branch Code: {input.BranchCode}";
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
