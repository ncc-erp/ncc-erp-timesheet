using Abp.Application.Services.Dto;
using Abp.Authorization;
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
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Uitls;
using Timesheet.Users.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs
{
    public class ManageUserProjectForBranchAppService : AppServiceBase
    {
        public ManageUserProjectForBranchAppService(IWorkScope workScope) : base(workScope) { }

        [HttpGet]
        public List<GetAllValueOfUserInProjectByUserIdDto> GetAllValueOfUserInProjectByUserId(long userId, DateTime? startDate, DateTime? endDate)
        {
            var listProjectByUserId = WorkScope.GetAll<ProjectUser>()
                         .Where(s => s.UserId == userId)
                         .Where(s => s.Project.Status == ProjectStatus.Active)
                         .Select(s => new GetAllValueOfUserInProjectByUserIdDto
                         {
                             ProjectId = s.ProjectId,
                             ProjectName = s.Project.Name,
                             ProjectCode = s.Project.Code,
                         })
                         .ToList();

            var dateTimeNow = DateTime.Now;

            // Tính thời gian user đóng góp cho dự án theo log timesheet đã được approve và trong tháng hiện tại(DateTime.Now)
            var employeeWorking = WorkScope.GetAll<MyTimesheet>()
                .Where(s => s.UserId == userId)
                .Where(ts => ts.Status == TimesheetStatus.Approve)
                .WhereIf((startDate.HasValue && endDate.HasValue), s => (s.DateAt.Date >= startDate) && (s.DateAt.Date <= endDate))
                //.WhereIf((!startDate.HasValue || !endDate.HasValue), s => (s.DateAt.Year == dateTimeNow.Year && s.DateAt.Month == dateTimeNow.Month))
                .Where(ts => ts.TypeOfWork == TypeOfWork.NormalWorkingHours)
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
                                            join v in WorkScope.GetAll<ValueOfUserInProject>() on l.ProjectId equals v.ProjectId
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
                              ShadowPercentage = vouip != null ? vouip.ShadowPercentage : 0,
                              ValueOfUserType = vouip != null ? vouip.Type : 0,
                              WorkingHours = emp != null ? emp.SumWorkingTime : 0
                          };


            return qresult.ToList();
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
