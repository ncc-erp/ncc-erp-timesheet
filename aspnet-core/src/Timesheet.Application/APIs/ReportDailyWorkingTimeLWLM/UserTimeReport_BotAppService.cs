using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.APIs.Reports.Dto;
using Timesheet.Entities;
using Timesheet.Services.Mezon;

namespace Timesheet.APIs.Reports
{
    public class UserTimeReport_BotAppService : ApplicationService
    {
        private readonly IWorkScope WorkScope;
        private readonly MezonService _mezonService;

        public UserTimeReport_BotAppService(IWorkScope workScope, MezonService mezonService)
        {
            WorkScope = workScope;
            _mezonService = mezonService;
            if (_mezonService != null)
            {
                var httpClientField = typeof(MezonService).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var httpClient = httpClientField?.GetValue(_mezonService) as System.Net.Http.HttpClient;
                if (httpClient != null)
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(300); //timemout
                }
            }
        }

        public UserTimeReport_BotAppService(IWorkScope workScope)
        {
            WorkScope = workScope;
        }

        [AbpAllowAnonymous]
        [HttpGet]
        public UserTimeReportResponseDto GetUserTimeReport(string branchName, DateTime reportDate, int? limit = null)
        {

            var currentWeekStart = reportDate.AddDays(-(int)reportDate.DayOfWeek + 1);
            var startLastWeek = currentWeekStart.AddDays(-7);
            var endLastWeek = startLastWeek.AddDays(6);

            var startLastMonth = new DateTime(reportDate.Year, reportDate.Month, 1).AddMonths(-1);
            var endLastMonth = startLastMonth.AddMonths(1).AddDays(-1);


            var dailyTimekeeping = (from tk in WorkScope.GetAll<Timekeeping>()
                                    where (tk.DateAt.Date >= startLastWeek && tk.DateAt.Date <= endLastWeek) ||
                                          (tk.DateAt.Date >= startLastMonth && tk.DateAt.Date <= endLastMonth)
                                    group tk by new { tk.UserId, Date = tk.DateAt.Date } into g
                                    select new
                                    {
                                        UserId = g.Key.UserId,
                                        Date = g.Key.Date,
                                        DailyHours = CalculateDailyHours(g.ToList())
                                    })
                                   .ToList();

            // Aggregate daily hours by time spans
            var timekeepingAggregated = dailyTimekeeping
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalTimeLW = g.Where(x => x.Date >= startLastWeek && x.Date <= endLastWeek)
                                  .Sum(x => x.DailyHours),
                    TotalTimeLM = g.Where(x => x.Date >= startLastMonth && x.Date <= endLastMonth)
                                  .Sum(x => x.DailyHours)
                })
                .ToList();

            //Aggregate WorkingTime from MyTimesheets
            var workingTimeAggregated = (from mt in WorkScope.GetAll<MyTimesheet>()
                                         where !mt.IsDeleted &&
                                               ((mt.DateAt.Date >= startLastWeek && mt.DateAt.Date <= endLastWeek) ||
                                                (mt.DateAt.Date >= startLastMonth && mt.DateAt.Date <= endLastMonth))
                                         group mt by mt.UserId into g
                                         select new
                                         {
                                             UserId = g.Key,
                                             TotalWorkingTimeLW = g.Where(x => x.DateAt.Date >= startLastWeek && x.DateAt.Date <= endLastWeek)
                                                                  .Sum(x => x.WorkingTime),
                                             TotalWorkingTimeLM = g.Where(x => x.DateAt.Date >= startLastMonth && x.DateAt.Date <= endLastMonth)
                                                                  .Sum(x => x.WorkingTime)
                                         })
                                        .ToList();

            var mainQuery = (from u in WorkScope.GetAll<User>()
                             join b in WorkScope.GetAll<Branch>() on u.BranchId equals b.Id
                             where u.IsActive && b.Name == branchName
                             select new
                             {
                                 UserId = u.Id,
                                 UserName = u.UserName,
                                 BranchName = b.Name,
                                 BranchId = u.BranchId
                             })
                             .ToList();

            // Join aggregated data
            var result = (from u in mainQuery
                          from tk in timekeepingAggregated.Where(x => x.UserId == u.UserId).DefaultIfEmpty()
                          from wt in workingTimeAggregated.Where(x => x.UserId == u.UserId).DefaultIfEmpty()
                          select new UserTimeReportByLWAndLMDto
                          {
                              UserName = u.UserName,
                              BranchName = u.BranchName,
                              TotalTimeLW = Math.Round(tk?.TotalTimeLW ?? 0, 2),
                              TotalTimeLM = Math.Round(tk?.TotalTimeLM ?? 0, 2),
                              TotalWorkingTimeLW = wt?.TotalWorkingTimeLW ?? 0,
                              TotalWorkingTimeLM = wt?.TotalWorkingTimeLM ?? 0
                          })
                          .ToList();

            var activeUsers = result.Where(x => x.TotalTimeLW > 0 || x.TotalTimeLM > 0).ToList();
            var inactiveUsers = result.Where(x => x.TotalTimeLW <= 0 && x.TotalTimeLM <= 0).ToList();
            var sortedActiveUsers = activeUsers
                .OrderByDescending(r => r.TotalTimeLW)
                .ThenByDescending(r => r.TotalTimeLM)
                .ThenBy(r => r.UserName);

            var finalUsers = limit.HasValue ? sortedActiveUsers.Take(limit.Value).ToList() : sortedActiveUsers.ToList();

            return new UserTimeReportResponseDto
            {
                ActiveCount = activeUsers.Count,
                InactiveCount = inactiveUsers.Count,
                Users = finalUsers
            };
        }

        private double CalculateDailyHours(List<Timekeeping> dayRecords)
        {
            var checkInTimes = dayRecords.Where(x => !string.IsNullOrEmpty(x.CheckIn))
                                        .Select(x => TimeSpan.TryParseExact(x.CheckIn, "hh\\:mm", null, out var time) ? (TimeSpan?)time : null)
                                        .Where(x => x.HasValue)
                                        .Select(x => x.Value)
                                        .ToList();

            var checkOutTimes = dayRecords.Where(x => !string.IsNullOrEmpty(x.CheckOut))
                                         .Select(x => TimeSpan.TryParseExact(x.CheckOut, "hh\\:mm", null, out var time) ? (TimeSpan?)time : null)
                                         .Where(x => x.HasValue)
                                         .Select(x => x.Value)
                                         .ToList();

            var trackerTimes = dayRecords.Where(x => !string.IsNullOrEmpty(x.TrackerTime))
                                        .Select(x => TimeSpan.TryParse(x.TrackerTime, out var time) ? (TimeSpan?)time : null)
                                        .Where(x => x.HasValue)
                                        .Select(x => x.Value)
                                        .ToList();

            if (checkInTimes.Any() && checkOutTimes.Any())
            {
                var minCheckIn = checkInTimes.Min();
                var maxCheckOut = checkOutTimes.Max();

                if (maxCheckOut >= minCheckIn)
                {
                    return (maxCheckOut - minCheckIn).TotalHours;
                }
            }

            if (trackerTimes.Any())
            {
                return trackerTimes.Max().TotalHours;
            }

            return 0;
        }

        [AbpAllowAnonymous]
        [HttpPost]
        public BotCommandEnvelopeDto ProcessBotCommand([FromBody] BotCommandRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Command))
                {
                    return Envelope("Invalid command");
                }

                var command = request.Command.Trim();

                if (command.StartsWith("*timesheet", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessTimesheetCommand(command, request.ReportDate);
                }

                return Envelope("Invalid command");
            }
            catch (Exception ex)
            {
                return Envelope(ex.Message);
            }
        }

        public BotCommandEnvelopeDto ProcessTimesheetCommand(string command, DateTime? reportDate)
        {
            try
            {
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    return Envelope("Missing BranchName. Example *timesheet HN1 10 or *timesheet HN1 all");
                }

                var branchFilter = parts[1].Trim().ToUpper();
                int? limitParam = 50; // default

                if (parts.Length > 2)
                {
                    var limitInput = parts[2].Trim().ToUpper();
                    if (limitInput == "ALL")
                        limitParam = null;
                    else if (int.TryParse(limitInput, out int parsedLimit))
                        limitParam = parsedLimit;
                }

                var dateToUse = reportDate ?? DateTime.Now;
                var timesheetResponse = GetUserTimeReport(branchFilter, dateToUse, limitParam);

                if (timesheetResponse.Users == null || !timesheetResponse.Users.Any())
                {
                    return Envelope($"Can't get data for branch '{branchFilter}'\n Active: {timesheetResponse.ActiveCount} | Inactive: {timesheetResponse.InactiveCount}");
                }

                var lines = new List<string>();
                int index = 1;
                foreach (var user in timesheetResponse.Users)
                {
                    lines.Add($"{index}. {user.UserName}: TotalTimeLastWeek: {user.TotalTimeLW}h, TotalTimeLastMonth: {user.TotalTimeLM}h");
                    index++;
                }

                var header = $"📊 Timesheet REPORT: branch '{branchFilter}'";
                var counts = $"👥 Active: {timesheetResponse.ActiveCount} | Inactive: {timesheetResponse.InactiveCount}";
                var fullText = $"{header}\n{string.Join("\n", lines)}\n{counts}";
                var mezonUrl = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportWebhookUrl);
                _mezonService.NotifyToChannel(mezonUrl, fullText);

                return new BotCommandEnvelopeDto
                {
                    Type = "DAILY_REPORT_BOT",
                    Message = new BotMessageDto
                    {
                        T = fullText,
                        Mentions = new List<MentionDto>()
                    }
                };
            }
            catch (Exception ex)
            {
                return Envelope("ERROR: " + ex.Message);
            }
        }

        public void NotifyFailure(string errorMessage)
        {
            try
            {
                Logger.Error($"NotifyFailure called: {errorMessage}");

                var mezonUrl = SettingManager.GetSettingValueForApplication(AppSettingNames.BotReportWebhookUrl);

                if (string.IsNullOrEmpty(mezonUrl))
                {
                    Logger.Error("Invalid Webhook");
                    return;
                }

                var alertMessage = $"🚨 **CRITICAL ALERT - BOT REPORT WORKER FAILED**\n\n" +
                    $"❌ **Error Details:**\n{errorMessage}\n\n" +
                    $"⏰ **Time:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $"🔄 **Status:** All retry attempts exhausted\n";


                if (_mezonService != null)
                {
                    _mezonService.NotifyToChannel(mezonUrl, alertMessage);
                    Logger.Info("Failure notification sent successfully to Mezon");
                }
                else
                {
                    Logger.Error("Cannot send failure notification: MezonService is null");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send failure notification: {ex.Message}", ex);
            }
        }

        private BotCommandEnvelopeDto Envelope(string text)
        {
            return new BotCommandEnvelopeDto
            {
                Type = "DAILY_REPORT_BOT",
                Message = new BotMessageDto
                {
                    T = text,
                    Mentions = new List<MentionDto>()
                }
            };
        }
    }

    public class BotCommandRequestDto
    {
        public string Command { get; set; }
        public DateTime? ReportDate { get; set; }
    }

    // Format for API POST
    public class BotCommandEnvelopeDto
    {
        public string Type { get; set; }
        public BotMessageDto Message { get; set; }
    }

    public class BotMessageDto
    {
        public string T { get; set; }
        public List<MentionDto> Mentions { get; set; }
    }

    public class MentionDto
    {
        public string User_Id { get; set; }
        public int S { get; set; }          // start index in T
        public int E { get; set; }
    }
}
