using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.Entities.Enum;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.BotReportDaily
{
    [AbpAuthorize]
    public class BotReportDailyAppService : ApplicationService//, IBotReportDailyAppService
    {
        private readonly IWorkScope _workScope;
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly MezonService _mezonService;
        private readonly ISettingManager _settingManager;
        private readonly IBotReportDailyService _botReportDailyService;

        public BotReportDailyAppService(IWorkScope workScope, IGoogleSheetsService googleSheetsService, MezonService mezonService, ISettingManager settingManager, DomainServices.BotReportDailyService botReportDailyService)
        {
            _workScope = workScope;
            _googleSheetsService = googleSheetsService;
            _mezonService = mezonService;
            _settingManager = settingManager;
            _botReportDailyService = botReportDailyService;
        }

        //[HttpGet]
        //public async Task<DailyProjectTimelogReportDto> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportInput input)
        //{
        //    if (input.OfficeId <= 0)
        //    {
        //        throw new UserFriendlyException("Office ID is required and must be greater than 0");
        //    }

        //    var today = DateTime.Now.Date;

        //    var lastWeekEnd = today.AddDays(-(int)today.DayOfWeek);
        //    if (lastWeekEnd == today)
        //    {
        //        lastWeekEnd = today.AddDays(-7);
        //    }
        //    var lastWeekStart = lastWeekEnd.AddDays(-6);

        //    var lastMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);
        //    var lastMonthStart = new DateTime(lastMonthEnd.Year, lastMonthEnd.Month, 1);

        //    var officeUsers = await _workScope.GetAll<User>()
        //        .Where(u => u.BranchId == input.OfficeId && !u.IsDeleted)
        //        .Select(u => u.Id)
        //        .ToListAsync();

        //    if (!officeUsers.Any())
        //    {
        //        return new DailyProjectTimelogReportDto
        //        {
        //            ReportDate = today.ToString("yyyy-MM-dd"),
        //            Projects = new List<ProjectTimelogDto>()
        //        };
        //    }

        //    var allProjects = await _workScope.GetAll<Project>()
        //        .Where(p => !p.IsDeleted && p.Status == Ncc.Entities.Enum.StatusEnum.ProjectStatus.Active)
        //        .Select(p => new { p.Id, p.Name })
        //        .ToListAsync();

        //    var activeProjectIds = allProjects.Select(p => p.Id).ToList();

        //    var timekeepingRecords = await _workScope.GetAll<Timekeeping>()
        //        .Where(t => officeUsers.Contains(t.UserId.Value) &&
        //                   ((t.DateAt >= lastWeekStart && t.DateAt <= lastWeekEnd) ||
        //                    (t.DateAt >= lastMonthStart && t.DateAt <= lastMonthEnd)))
        //        .ToListAsync();

        //    var projectUsers = await _workScope.GetAll<ProjectUser>()
        //        .Where(pu => !pu.IsDeleted &&
        //               pu.Type != ProjectUserType.DeActive &&
        //               activeProjectIds.Contains(pu.ProjectId))
        //        .ToListAsync();

        //    var allUsers = await _workScope.GetAll<User>()
        //        .Where(u => !u.IsDeleted)
        //        .Select(u => new { u.Id, u.UserName })
        //        .ToListAsync();

        //    var absenceRecords = await _workScope.GetAll<AbsenceDayDetail>()
        //        .Include(s => s.Request)
        //        .Where(s => officeUsers.Contains(s.Request.UserId) &&
        //               ((s.DateAt >= lastWeekStart && s.DateAt <= lastWeekEnd) ||
        //                (s.DateAt >= lastMonthStart && s.DateAt <= lastMonthEnd)) &&
        //               s.Request.Status == Ncc.Entities.Enum.StatusEnum.RequestStatus.Approved)
        //        .Select(s => new
        //        {
        //            UserId = s.Request.UserId,
        //            DateAt = s.DateAt,
        //            DateType = s.DateType,
        //            AbsenceTime = s.AbsenceTime,
        //            Type = s.Request.Type
        //        })
        //        .ToListAsync();

        //    var myTimesheetData = await _workScope.GetAll<MyTimesheet>()
        //        .Where(mt => officeUsers.Contains(mt.UserId) &&
        //               ((mt.DateAt >= lastWeekStart && mt.DateAt <= lastWeekEnd) ||
        //                (mt.DateAt >= lastMonthStart && mt.DateAt <= lastMonthEnd)) &&
        //               mt.Status == Ncc.Entities.Enum.StatusEnum.TimesheetStatus.Approve)
        //        .Include(mt => mt.ProjectTask)
        //        .ToListAsync();

        //    var userProjectMap = myTimesheetData
        //        .Where(mt => mt.ProjectTask != null && activeProjectIds.Contains(mt.ProjectTask.ProjectId))
        //        .GroupBy(mt => new { mt.UserId, mt.DateAt })
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.GroupBy(mt => mt.ProjectTask.ProjectId)
        //                 .Select(pg => new
        //                 {
        //                     ProjectId = pg.Key,
        //                     TotalMinutes = pg.Sum(mt => mt.WorkingTime)
        //                 })
        //                 .OrderByDescending(p => p.TotalMinutes)
        //                 .FirstOrDefault()?.ProjectId
        //        );

        //    var timekeepingMinutes = new List<(long UserId, DateTime DateAt, long? ProjectId, double Minutes)>();

        //    foreach (var record in timekeepingRecords)
        //    {
        //        if (!record.UserId.HasValue) continue;

        //        var userDateKey = new { UserId = record.UserId.Value, DateAt = record.DateAt };
        //        var mostLoggedProjectId = userProjectMap.ContainsKey(userDateKey) ? userProjectMap[userDateKey] : null;

        //        if (mostLoggedProjectId == null)
        //        {
        //            var userProject = projectUsers.FirstOrDefault(pu =>
        //                pu.UserId == record.UserId.Value &&
        //                record.DateAt >= pu.CreationTime &&
        //                (pu.IsDeleted == false || record.DateAt <= pu.DeletionTime));

        //            if (userProject != null)
        //            {
        //                mostLoggedProjectId = userProject.ProjectId;
        //            }
        //        }

        //        if (mostLoggedProjectId != null)
        //        {
        //            double minutes = 0;
        //            bool isRemote = false;

        //            var remoteRequests = absenceRecords
        //                .Where(r => r.Type == RequestType.Remote && r.UserId == record.UserId.Value && r.DateAt.Date == record.DateAt.Date)
        //                .ToList();
        //            isRemote = remoteRequests.Any();

        //            if (isRemote)
        //            {
        //                if (!string.IsNullOrEmpty(record.TrackerTime) && TimeSpan.TryParse(record.TrackerTime, out TimeSpan trackerTimeSpan))
        //                {
        //                    minutes = trackerTimeSpan.TotalMinutes;
        //                }
        //            }
        //            else if (string.IsNullOrEmpty(record.CheckOut) && !string.IsNullOrEmpty(record.CheckIn) &&
        //                     !string.IsNullOrEmpty(record.TrackerTime) && TimeSpan.TryParse(record.TrackerTime, out TimeSpan trackerTimeSpan))
        //            {
        //                minutes = trackerTimeSpan.TotalMinutes;
        //            }
        //            else if (!string.IsNullOrEmpty(record.CheckIn) && !string.IsNullOrEmpty(record.CheckOut))
        //            {
        //                if (TimeSpan.TryParse(record.CheckIn, out TimeSpan checkInTime) &&
        //                    TimeSpan.TryParse(record.CheckOut, out TimeSpan checkOutTime))
        //                {
        //                    minutes = (checkOutTime - checkInTime).TotalMinutes;

        //                    if (minutes > 240)
        //                    {
        //                        minutes -= 60;
        //                    }
        //                }
        //            }

        //            if (minutes > 0)
        //            {
        //                timekeepingMinutes.Add((record.UserId.Value, record.DateAt, mostLoggedProjectId, minutes));
        //            }
        //        }
        //    }

        //    var projectUserMap = myTimesheetData
        //        .Where(mt => mt.ProjectTask != null && activeProjectIds.Contains(mt.ProjectTask.ProjectId))
        //        .GroupBy(mt => mt.ProjectTask.ProjectId)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Select(mt => mt.UserId).Distinct().ToList()
        //        );

        //    var validUserDateProjectCombinations = new HashSet<(long UserId, DateTime DateAt, long ProjectId)>();
        //    foreach (var userDateKey in userProjectMap)
        //    {
        //        if (userDateKey.Value.HasValue)
        //        {
        //            validUserDateProjectCombinations.Add((userDateKey.Key.UserId, userDateKey.Key.DateAt, userDateKey.Value.Value));
        //        }
        //    }

        //    var filteredTimekeepingMinutes = timekeepingMinutes
        //        .Where(t => t.ProjectId.HasValue && validUserDateProjectCombinations.Contains((t.UserId, t.DateAt, t.ProjectId.Value)))
        //        .ToList();

        //    var projectTimelogData = filteredTimekeepingMinutes
        //        .GroupBy(t => t.ProjectId)
        //        .Select(g => new
        //        {
        //            ProjectId = g.Key,
        //            TotalMinutesLW = g.Where(t => t.DateAt >= lastWeekStart && t.DateAt <= lastWeekEnd).Sum(t => t.Minutes),
        //            TotalMinutesLM = g.Where(t => t.DateAt >= lastMonthStart && t.DateAt <= lastMonthEnd).Sum(t => t.Minutes),
        //            UserIds = g.Key.HasValue && projectUserMap.ContainsKey(g.Key.Value) ?
        //                      projectUserMap[g.Key.Value].Intersect(g.Select(t => t.UserId).Distinct()).ToList() :
        //                      new List<long>()
        //        })
        //        .Where(p => p.ProjectId.HasValue)
        //        .ToList();

        //    var result = new DailyProjectTimelogReportDto
        //    {
        //        ReportDate = today.ToString("yyyy-MM-dd"),
        //        Projects = projectTimelogData
        //            .Select(p => new ProjectTimelogDto
        //            {
        //                Name = allProjects.FirstOrDefault(proj => proj.Id == p.ProjectId)?.Name ?? "Unknown Project",
        //                Members = allUsers
        //                    .Where(u => p.UserIds.Contains(u.Id))
        //                    .Select(u => u.UserName)
        //                    .OrderBy(name => name)
        //                    .ToList(),
        //                TotalTimelogLW = Math.Round(p.TotalMinutesLW / 60, 2),
        //                TotalTimelogLM = Math.Round(p.TotalMinutesLM / 60, 2)
        //            })
        //            .Where(p => p.TotalTimelogLW >= (input.MinHours ?? 0))
        //            .OrderByDescending(p => p.TotalTimelogLW)
        //            .ThenByDescending(p => p.TotalTimelogLM)
        //            .ThenBy(p => p.Name)
        //            .ToList()
        //    };

        //    if (input.TopN.HasValue && input.TopN.Value > 0 && result.Projects.Count > input.TopN.Value)
        //    {
        //        result.Projects = result.Projects.Take(input.TopN.Value).ToList();
        //    }

        //    return result;
        //}

        //[HttpGet]
        //public async Task<GoogleSheetsReportDto> GetDailyProjectTimelogGoogleSheetsReport(GetGoogleSheetsReportInput input)
        //{
        //    var today = DateTime.Now.Date;

        //    var lastWeekEnd = today.AddDays(-(int)today.DayOfWeek);
        //    if (lastWeekEnd == today)
        //    {
        //        lastWeekEnd = today.AddDays(-7);
        //    }
        //    var lastWeekStart = lastWeekEnd.AddDays(-6);

        //    var lastMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);
        //    var lastMonthStart = new DateTime(lastMonthEnd.Year, lastMonthEnd.Month, 1);

        //    var reportData = await GetDailyProjectTimelogReport(new GetDailyProjectTimelogReportInput
        //    {
        //        OfficeId = input.OfficeId,
        //        MinHours = input.MinHours,
        //        TopN = input.TopN
        //    });

        //    var sheetUrl = await _googleSheetsService.CreateDailyReportSheetAsync(reportData);

        //    var webhookUrl = await SettingManager.GetSettingValueAsync(AppSettingNames.BotReportWebhookUrl);

        //    var reportName = $"Daily Project Timelog Report - {reportData.ReportDate}";
        //    var message = $"📊 {reportName}\n\nGenerated at: {DateTime.Now}\nOffice: {input.OfficeId}\n\n🔗 Report URL: {sheetUrl}";

        //    if (!string.IsNullOrEmpty(webhookUrl))
        //    {
        //        // Option 1: Send with Google Sheets link (original)
        //        //_mezonService.NotifyToChannelWithLink(webhookUrl, message, sheetUrl);

        //        var projectData = reportData.Projects
        //            .OrderByDescending(x => x.TotalTimelogLW)
        //            .Take(input.TopN ?? 10)
        //            .Select(x => new
        //            {
        //                Project = x.Name.Trim(),
        //                Members = string.Join(", ", x.Members),
        //                LastWeekHours = $"{x.TotalTimelogLW:0.0}h",
        //                LastMonthHours = $"{x.TotalTimelogLM:0.0}h"
        //            })
        //            .ToList();

        //        var sb = new System.Text.StringBuilder();

        //        string lastWeekPeriod = $"{lastWeekStart:dd/MM/yyyy} - {lastWeekEnd:dd/MM/yyyy}";
        //        string lastMonthPeriod = $"{lastMonthStart:MM/yyyy}";

        //        sb.AppendLine("📊 Top Projects Summary");
        //        sb.AppendLine($"Report Period: Last Week ({lastWeekPeriod}) | Last Month ({lastMonthPeriod})");
        //        sb.AppendLine("");

        //        foreach (var item in projectData)
        //        {
        //            sb.AppendLine($"Project: {item.Project}");
        //            sb.AppendLine($"Members: {item.Members}");
        //            sb.AppendLine($"Last Week: {item.LastWeekHours}");
        //            sb.AppendLine($"Last Month: {item.LastMonthHours}");
        //            sb.AppendLine("-------------------------------------");
        //        }

        //        if (projectData.Any())
        //        {
        //            sb.Length -= "-------------------------------------\r\n".Length;
        //        }

        //        var messageText = sb.ToString();

        //        Console.WriteLine(messageText);

        //        var mkList = new List<object>();

        //        string titleToFind = "Top Projects Summary";
        //        int titlePos = messageText.IndexOf(titleToFind);

        //        if (titlePos >= 0)
        //        {
        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = titlePos,
        //                e = titlePos + titleToFind.Length
        //            });
        //        }

        //        string reportPeriodText = "Report Period:";
        //        int reportPeriodPos = messageText.IndexOf(reportPeriodText);
        //        if (reportPeriodPos >= 0)
        //        {
        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = reportPeriodPos,
        //                e = reportPeriodPos + reportPeriodText.Length
        //            });
        //        }

        //        string lastWeekText = "Last Week";
        //        int lastWeekPeriodPos = messageText.IndexOf(lastWeekText, reportPeriodPos);
        //        if (lastWeekPeriodPos >= 0)
        //        {
        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = lastWeekPeriodPos,
        //                e = lastWeekPeriodPos + lastWeekText.Length
        //            });
        //        }

        //        string lastMonthText = "Last Month";
        //        int lastMonthPeriodPos = messageText.IndexOf(lastMonthText, reportPeriodPos);
        //        if (lastMonthPeriodPos >= 0)
        //        {
        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = lastMonthPeriodPos,
        //                e = lastMonthPeriodPos + lastMonthText.Length
        //            });
        //        }

        //        int currentPos = 0;
        //        while (true)
        //        {
        //            int projectPos = messageText.IndexOf("Project:", currentPos);
        //            int membersPos = messageText.IndexOf("Members:", currentPos);
        //            int lastWeekPos = messageText.IndexOf("Last Week:", currentPos);
        //            int lastMonthPos = messageText.IndexOf("Last Month:", currentPos);

        //            if (projectPos == -1) break;

        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = projectPos,
        //                e = projectPos + "Project:".Length
        //            });

        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = membersPos,
        //                e = membersPos + "Members:".Length
        //            });

        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = lastWeekPos,
        //                e = lastWeekPos + "Last Week:".Length
        //            });

        //            mkList.Add(new
        //            {
        //                type = "b",
        //                s = lastMonthPos,
        //                e = lastMonthPos + "Last Month:".Length
        //            });

        //            currentPos = lastMonthPos + "Last Month:".Length;
        //        }

        //        _mezonService.Post(webhookUrl, new
        //        {
        //            type = "hook",
        //            message = new
        //            {
        //                t = messageText,
        //                mk = mkList
        //            }
        //        });
        //    }

        //    return new GoogleSheetsReportDto
        //    {
        //        ReportUrl = sheetUrl,
        //        GeneratedAt = DateTime.Now,
        //        ReportName = reportName
        //    };
        //}

        [HttpGet]
        public async Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportInput input)
        {
            return await _botReportDailyService.SendDailyProjectTimelogToMezon(input);
        }

        // ===== Helpers để tính độ rộng hiển thị (display width) =====
        //public static class StringDisplayExtensions
        //{
        //    public static int GetDisplayWidth(this string input)
        //    {
        //        if (string.IsNullOrEmpty(input)) return 0;

        //        int width = 0;
        //        foreach (char c in input)
        //        {
        //            // ASCII cơ bản → width = 1
        //            if (c <= 0x1f || (c >= 0x7f && c <= 0x9f))
        //                continue;

        //            if (c <= 0x7f)
        //                width += 1;
        //            else
        //                width += 2; // Unicode wide characters (đa số chữ có dấu)
        //        }
        //        return width;
        //    }

        //    public static string PadRightDisplay(this string input, int totalWidth)
        //    {
        //        int displayWidth = input.GetDisplayWidth();
        //        if (displayWidth >= totalWidth) return input;
        //        return input + new string(' ', totalWidth - displayWidth);
        //    }

        //    public static string PadLeftDisplay(this string input, int totalWidth)
        //    {
        //        int displayWidth = input.GetDisplayWidth();
        //        if (displayWidth >= totalWidth) return input;
        //        return new string(' ', totalWidth - displayWidth) + input;
        //    }
        //}


        //public static class StringExtensions
        //{
        //    public static string TruncateWithEllipsis(this string value, int maxLength)
        //    {
        //        if (string.IsNullOrEmpty(value)) return value;
        //        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        //    }
        //}
    }
}
