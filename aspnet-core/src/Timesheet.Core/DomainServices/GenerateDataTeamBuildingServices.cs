using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Services;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Linq;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Project;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class GenerateDataTeamBuildingServices : DomainService, ITransientDependency
    {
        public static ProjectService _projectService;
        public readonly ILogger<GenerateDataTeamBuildingServices> _logger;
        public readonly IWorkScope _workScope;

        public GenerateDataTeamBuildingServices(ProjectService projectService, ILogger<GenerateDataTeamBuildingServices> logger, IWorkScope workScope)
        {
            _projectService = projectService;
            _logger = logger;
            _workScope = workScope;
        }
        public void AddDataToTeamBuildingDetail(InputGenerateDataTeamBuildingDto input)
        {
            double moneyDefault;
            bool parseResult = double.TryParse(SettingManager.GetSettingValueForApplication(AppSettingNames.TeamBuildingMoney), out moneyDefault);
            var isExist = _workScope.GetAll<TeamBuildingDetail>()
                .Where(s => s.ApplyMonth.Month == input.Month && s.ApplyMonth.Year == input.Year)
                .Select(s => s.Id)
                .ToList();

            if (!parseResult)
            {
                _logger.LogError($"{AppSettingNames.TeamBuildingMoney} is not found or invalid data");
                throw new UserFriendlyException("Something error occurs, please contact to administrator for more information");
            }

            if (input.Month == default || input.Year == default)
                throw new UserFriendlyException(String.Format("Selected month or year is empty!"));

            if (input.Month > DateTimeUtils.GetNow().Month && input.Year >= DateTimeUtils.GetNow().Year)
                throw new UserFriendlyException("The selected month cannot greater than the current month!");

            if (isExist.Count() > 0)
            {
                throw new UserFriendlyException(String.Format($"Records already exist for the {input.Month}/{input.Year}"));
            }

            var employeeWorking = _workScope.GetAll<MyTimesheet>()
                .Where(s => s.User.IsActive)
                .Where(s => !s.ProjectTask.Project.isAllUserBelongTo)
                .Select(s => new {
                    s.UserId,
                    DateAt = s.DateAt.Date,
                    s.TypeOfWork,
                    s.Status,
                    s.ProjectTask.ProjectId,
                    s.WorkingTime,
                    s.User.Level,
                    s.ProjectTask.Project.IsAllowTeamBuilding
                })
                .Where(ts => ts.Status == TimesheetStatus.Approve)
                .Where(ts => ts.DateAt.Year == input.Year && ts.DateAt.Month == input.Month)
                .Where(ts => ts.Level >= UserLevel.Intern_2)
                .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .ToList();

            var listEmployeeIntern2 = employeeWorking.Where(s => s.Level == UserLevel.Intern_2)
                .Select(s => new
                {
                    s.UserId,
                    s.WorkingTime,
                    s.IsAllowTeamBuilding
                })
                .GroupBy(s => s.UserId)
                .Select(s => new
                {
                    UserId = s.Key,
                    TimeWorkingIsAllowTeamBuilding = s.Where(x => x.IsAllowTeamBuilding).Sum(x => x.WorkingTime),
                    TimeWorkingNotAllowTeamBuilding = s.Where(x => !x.IsAllowTeamBuilding).Sum(x => x.WorkingTime)
                })
                .Where(s => s.TimeWorkingIsAllowTeamBuilding < s.TimeWorkingNotAllowTeamBuilding)
                .Select(s => s.UserId)
                .ToList();

            var mostProjectEmployeeWorking = employeeWorking
                .Where(s => s.IsAllowTeamBuilding)
                .Select(s => new
                {
                    s.UserId,
                    s.ProjectId,
                    s.WorkingTime
                })
                .GroupBy(s => s.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    WorkingTime = g.Sum(s => s.WorkingTime),
                    MaxSumWorkingTimeProjectId = g
                    .GroupBy(s => s.ProjectId)
                    .Select(x => new
                    {
                        ProjectId = x.Key,
                        SumWorkingTime = x.Sum(s => s.WorkingTime)
                    })
                    .OrderByDescending(x => x.SumWorkingTime)
                    .FirstOrDefault().ProjectId,
                });

            var employeeWorkingMaxDictionary = mostProjectEmployeeWorking
                .ToDictionary(e => e.UserId, e => e.MaxSumWorkingTimeProjectId);

            var requestOff = _workScope.GetAll<AbsenceDayDetail>()
                .Where(s => s.DateAt.Month == input.Month)
                .Where(s => s.DateAt.Year == input.Year)
                .Where(s => s.Request.Status == RequestStatus.Approved)
                .Where(s => s.Request.Type == RequestType.Off)
                .Where(s => s.DateType != DayType.Custom)
                .Select(s => new
                {
                    s.Request.UserId,
                    s.DateAt,
                    s.DateType,
                })
                .ToList()
                .GroupBy(s => s.UserId)
                .ToDictionary(s => s.Key,
                              s => s.GroupBy(x => x.DateAt)
                                    .ToDictionary(x => x.Key,
                                                  x => x.Select(z => z.DateType == DayType.Fullday ? 1 : 0.5)
                                                  .FirstOrDefault()
                                                  ));

            var dayWorking = employeeWorking
                .Where(s => s.IsAllowTeamBuilding)
                .Select(s => new
                {
                    s.UserId,
                    s.DateAt
                })
                .Distinct()
                .GroupBy(s => s.UserId)
                .Select(s => new
                {
                    UserId = s.Key,
                    DayWorking = s.Select(x =>
                    !requestOff.ContainsKey(s.Key) ? 1 :
                    !requestOff[s.Key].ContainsKey(x.DateAt) ? 1 :
                    (1 - requestOff[s.Key][x.DateAt]))
                    .Sum()
                })
                .ToList();

            var project = dayWorking.Select(s => new
            {
                s.UserId,
                s.DayWorking,
                ProjectId = employeeWorkingMaxDictionary[s.UserId]
            }).ToList();

            var listProject = project.Select(s => s.ProjectId).Distinct().ToList();

            var projectInfo = _workScope.GetAll<Project>()
                    .Where(s => s.IsAllowTeamBuilding)
                    .GroupBy(s => s.Id)
                    .ToDictionary(s => s.Key, s => s.Select(x => x.Code)
                    .FirstOrDefault());

            var projectIdUserIds = mostProjectEmployeeWorking
                .GroupBy(e => e.MaxSumWorkingTimeProjectId)
                .ToDictionary(g => g.Key, g => g
                    .Select(e => e.UserId)
                    .Distinct()
                    .Select(e => new
                    {
                        UserId = e,
                        DayWorking = project
                        .Where(s => s.UserId == e)
                        .Select(s => s.DayWorking)
                        .FirstOrDefault()
                    })
                    .ToList());

            var teamBuildingDetails = listProject.Select(s => new
            {
                ProjectId = s,
                ProjectCode = projectInfo[s],
                ListUser = projectIdUserIds[s]
                .Select(x => new
                {
                    x.UserId,
                    x.DayWorking
                })
                .ToList()
            });

            var standardDay = GetListStandardDay(input.Year, input.Month);

            foreach (var x in teamBuildingDetails)
            {
                foreach (var i in x.ListUser)
                {
                    if (listEmployeeIntern2.Contains(i.UserId)) continue;
                    var money = (float)Math.Floor(moneyDefault * i.DayWorking / standardDay);
                    var UpdateTeamBuildingDetails = new TeamBuildingDetail
                    {
                        ApplyMonth = new DateTime(input.Year, input.Month, input.Day.HasValue ? input.Day.Value : 1),
                        EmployeeId = i.UserId,
                        ProjectId = x.ProjectId,
                        Status = TeamBuildingStatus.Open,
                        Money = money > 100000 ? 100000 : money,
                    };
                    _workScope.Insert(UpdateTeamBuildingDetails);
                }
            }
        }
        public int GetListStandardDay(int year, int month)
        {
            DateTime startDate = new DateTime(year, month, 1).Date;
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var offDays = _workScope.GetAll<DayOffSetting>()
                                   .Where(s => s.DayOff.Year == year && s.DayOff.Month == month)
                                   .Select(s => s.DayOff.Date)
                                   .ToList();

            var standardDay = 0;
            var date = startDate;
            while (date <= endDate)
            {
                if (!offDays.Contains(date) && date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                {
                    standardDay++;
                }
                date = date.AddDays(1);
            }
            return standardDay;
        }
    }
}
