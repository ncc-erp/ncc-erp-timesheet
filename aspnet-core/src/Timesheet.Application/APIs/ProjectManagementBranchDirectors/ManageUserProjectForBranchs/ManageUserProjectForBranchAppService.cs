using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs.Dto;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs
{
    public class ManageUserProjectForBranchAppService : AppServiceBase
    {
        public ManageUserProjectForBranchAppService(IWorkScope workScope) : base(workScope) { }

        [HttpGet]
        public async Task<WorkTimeByProjectDto> GetAllValueOfUserInProjectByUserId(long userId, DateTime? startDate, DateTime? endDate)
        {
            var projectUsers = await WorkScope.GetAll<ProjectUser>()
                         .Where(s => !s.Project.isAllUserBelongTo)
                         .Where(s => s.UserId == userId)
                         .Select(s => new ValueOfUserInProjectDto
                         {
                             ProjectId = s.ProjectId,
                             ProjectName = s.Project.Name,
                             ProjectCode = s.Project.Code,
                             Status = s.Project.Status,
                             ProjectUserType = s.Type,
                             Effort = s.Effort,
                         })
                         .ToListAsync();

            var projectTotalWorkingTimeDict = await WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.UserId == userId)
                .Where(ts => ts.Status == TimesheetStatus.Approve)
                .WhereIf((startDate.HasValue && endDate.HasValue), s => (s.DateAt.Date >= startDate) && (s.DateAt.Date <= endDate))
                .Where(s => !s.ProjectTask.Project.isAllUserBelongTo)
                .GroupBy(s => s.ProjectTask.ProjectId, s => s.WorkingTime)
                .ToDictionaryAsync(g => g.Key, g => g.Sum());

            foreach (var pu in projectUsers)
            {
                pu.WorkingHours = pu.WorkingHours = projectTotalWorkingTimeDict.GetValueOrDefault(pu.ProjectId, 0);
            }

            var workTimeByProjectDto = new WorkTimeByProjectDto
            {
                AllValueOfUserInProjectDtos = projectUsers,
                TotalWorkingHours = projectTotalWorkingTimeDict.Values.Sum()
            };
            return workTimeByProjectDto;
        }

        [HttpPost]
        public async System.Threading.Tasks.Task UpdateProjectUserEffort(UpdateProjectUserEffortDto input)
        {
            var projectUser = await WorkScope.GetAll<ProjectUser>()
                .Where(e => e.UserId == input.UserId)
                .Where(e => e.ProjectId == input.ProjectId)
                .FirstOrDefaultAsync();

            if (projectUser == null)
            {
                throw new UserFriendlyException("Project user was not found");
            }

            projectUser.Effort = input.Effort;
            projectUser.Type = input.Type;
            await WorkScope.UpdateAsync(projectUser);
        }
    }
}
