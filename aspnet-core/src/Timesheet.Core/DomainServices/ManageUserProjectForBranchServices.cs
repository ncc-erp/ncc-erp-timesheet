using Abp.Dependency;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class ManageUserProjectForBranchServices : BaseDomainService, IManageUserProjectForBranchServices, ITransientDependency
    {
        private readonly IWorkScope _workScope;

        public ManageUserProjectForBranchServices(IWorkScope workScope)
        {
            _workScope = workScope;
        }

        public async Task<List<ProjectHistoryDto>> GetUserProjectHistory(long userId, DateTime? startDate, DateTime? endDate)
        {
            if (userId <= 0)
            {
                throw new UserFriendlyException("UserId is invalid");
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new UserFriendlyException("Start date cannot be later than end date");
            }

            try
            {
                var timesheets = await _workScope.GetAll<MyTimesheet>()
                    .Where(ts => ts.UserId == userId && ts.Status == TimesheetStatus.Approve)
                    .WhereIf(startDate.HasValue, ts => ts.DateAt >= startDate)
                    .WhereIf(endDate.HasValue, ts => ts.DateAt <= endDate)
                    .Where(ts => !ts.ProjectTask.Project.isAllUserBelongTo)
                    .Select(ts => new {
                        ts.ProjectTask.ProjectId,
                        ts.ProjectTask.Project.Name,
                        ts.ProjectTask.Project.Status,
                        ts.DateAt,
                        ts.WorkingTime
                    })
                    .OrderBy(ts => ts.DateAt)
                    .ToListAsync();

                var projectUserType = await _workScope.GetAll<ProjectUser>()
                    .Where(pu => pu.UserId == userId)
                    .Where(pu => !pu.Project.isAllUserBelongTo)
                    .ToDictionaryAsync(pu => pu.ProjectId, pu => pu.Type);

                var totalTimePerMonth = timesheets
                    .GroupBy(ts => new { ts.DateAt.Month, ts.DateAt.Year })
                    .ToDictionary(
                        g => $"{g.Key.Month:D2}/{g.Key.Year}",
                        g => g.Sum(ts => ts.WorkingTime)
                    );

                var history = timesheets.GroupBy(ts => ts.ProjectId).Select(group => {
                    var projectId = group.Key;
                    var projectName = group.First().Name;
                    var workingDates = group.Select(x => x.DateAt.Date).Distinct().OrderBy(d => d).ToList();

                    ProjectUserType userType = projectUserType.ContainsKey(projectId)
                                               ? projectUserType[projectId]
                                               : ProjectUserType.DeActive;

                    var intervals = new List<ProjectIntervalDto>();
                    if (workingDates.Any())
                    {
                        DateTime periodStartDate = workingDates[0];
                        DateTime periodEndDate = workingDates[0];

                        for (int i = 1; i < workingDates.Count; i++)
                        {
                            DateTime processedDate = workingDates[i];
                            int monthDifference = (processedDate.Year - periodEndDate.Year) * 12 + processedDate.Month - periodEndDate.Month;

                            if (monthDifference > 1)
                            {
                                intervals.Add(new ProjectIntervalDto { StartDate = periodStartDate, EndDate = periodEndDate });
                                periodStartDate = processedDate;
                            }
                            periodEndDate = processedDate;
                        }

                        bool isPresent = (DateTime.Now.Date - periodEndDate).TotalDays <= 1;
                        intervals.Add(new ProjectIntervalDto
                        {
                            StartDate = periodStartDate,
                            EndDate = isPresent ? null : (DateTime?)periodEndDate
                        });
                    }

                    var monthlyEfforts = group.GroupBy(ts => new { ts.DateAt.Month, ts.DateAt.Year })
                        .Select(m => {
                            string monthYearKey = $"{m.Key.Month:D2}/{m.Key.Year}";

                            double projectWorkingTimeInMonth = m.Sum(x => x.WorkingTime);
                            double totalWorkingTimeInMonth = totalTimePerMonth.ContainsKey(monthYearKey)
                                                             ? totalTimePerMonth[monthYearKey]
                                                             : 0;

                            double effortPercent = totalWorkingTimeInMonth > 0
                                                   ? Math.Round(projectWorkingTimeInMonth / totalWorkingTimeInMonth * 100, 1)
                                                   : 0;

                            return new MonthlyEffortDto
                            {
                                MonthYear = monthYearKey,
                                Effort = effortPercent
                            };
                        }).ToList();

                    return new ProjectHistoryDto
                    {
                        ProjectId = projectId,
                        ProjectName = projectName,
                        ProjectUserType = userType,
                        Intervals = intervals,
                        MonthlyEfforts = monthlyEfforts,
                        Status = group.First().Status
                    };
                }).ToList();

                return history;
            }
            catch (Exception ex) 
            {
                if (ex is UserFriendlyException)
                {
                    throw;
                }
                throw new UserFriendlyException("An error occurred while retrieving project history", ex);
            }
        }
    }
}
