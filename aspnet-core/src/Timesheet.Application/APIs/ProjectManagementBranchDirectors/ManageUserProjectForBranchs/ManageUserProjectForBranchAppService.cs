using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Word;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs.Dto;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs
{
    public class ManageUserProjectForBranchAppService : AppServiceBase
    {
        public ManageUserProjectForBranchAppService(IWorkScope workScope) : base(workScope) { }

        [HttpGet]
        public WorkTimeByProjectDto GetAllValueOfUserInProjectByUserId(long userId, DateTime? startDate, DateTime? endDate)
        {
            var listProjectByUserId = WorkScope.GetAll<ProjectUser>()
                         .Where(s => s.UserId == userId)
                         .Select(s => new GetAllValueOfUserInProjectByUserIdDto
                         {
                             ProjectId = s.ProjectId,
                             ProjectName = s.Project.Name,
                             ProjectCode = s.Project.Code,
                             Status = s.Project.Status,
                             ProjectUserType = s.Type
                         })
                         .ToList();

            var employeeWorking = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.UserId == userId)
                .Where(ts => ts.Status == TimesheetStatus.Approve)
                .WhereIf((startDate.HasValue && endDate.HasValue), s => (s.DateAt.Date >= startDate) && (s.DateAt.Date <= endDate))
                //.Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
                .Select(s => new SumWorkingTimeByProjectDto
                {
                    DateAt = s.DateAt.Date,
                    TypeOfWork = s.TypeOfWork,
                    Status = s.Status,
                    ProjectId = s.ProjectTask.ProjectId,
                    WorkingTime = s.WorkingTime,
                })
                .GroupBy(s => s.ProjectId)
                .Select(x => new
                {
                    ProjectId = x.Key,
                    SumWorkingTime = x.Sum(s => s.WorkingTime),
                })
                .ToList();

            var qLastValueOfUserInProject = from l in listProjectByUserId
                                            join v in WorkScope.GetAll<ValueOfUserInProject>().Where(s => s.UserId == userId) on l.ProjectId equals v.ProjectId
                                            group v by v.ProjectId into grouped
                                            select grouped.OrderByDescending(v => v.CreationTime).First();

            var qresult = from lpbu in listProjectByUserId
                          join emp in employeeWorking on lpbu.ProjectId equals emp.ProjectId into empGroup
                          join vouip in qLastValueOfUserInProject on lpbu.ProjectId equals vouip.ProjectId into vouipGroup
                          from emp in empGroup.DefaultIfEmpty()
                          from vouip in vouipGroup.DefaultIfEmpty()
                          select new GetAllValueOfUserInProjectByUserIdDto
                          {
                              ProjectId = lpbu.ProjectId,
                              ProjectName = lpbu.ProjectName,
                              ProjectCode = lpbu.ProjectCode,
                              Status = lpbu.Status,
                              ProjectUserType = lpbu.ProjectUserType,
                              ShadowPercentage = vouip != null ? vouip.ShadowPercentage : 0,
                              ValueOfUserType = vouip != null ? vouip.Type : 0,
                              WorkingHours = emp != null ? emp.SumWorkingTime : 0
                          };

            var workTimeByProjectDto = new WorkTimeByProjectDto
            {
                GetAllValueOfUserInProjectByUserIdDtos = qresult.ToList(),
                TotalWorkingHours = employeeWorking.Sum(item => item.SumWorkingTime)
            };
            return workTimeByProjectDto;
        }

        [HttpPost]
        public async Task<CreateValueOfUserDto> CreateValueOfUser(CreateValueOfUserDto input)
        {
            var valueOfUser = ObjectMapper.Map<ValueOfUserInProject>(input);
            await WorkScope.InsertAsync(valueOfUser);
            return input;
        }
    }
}
