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
using Timesheet.APIs.BranchManager.Dto;
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
        public List<GetAllValueOfUserInProjectByUserIdDto> GetAllValueOfUserInProjectByUserId(long userId)
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
                .Where(ts => ts.DateAt.Year == dateTimeNow.Year && ts.DateAt.Month == dateTimeNow.Month)
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

            var qresult = from lpbu in listProjectByUserId 
                          join emp in employeeWorking on lpbu.ProjectId equals emp.ProjectId into empGroup
                          join vouip in WorkScope.GetAll<ValueOfUserInProject>() on lpbu.ProjectId equals vouip.ProjectId into vouipGroup
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

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserProjectForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserProjectForBranchs_ViewMyBranch)]
        public async Task<PagedResultDto<UserProjectsDto>> GetAllUserPagging(GridParam input, long? positionId, long? branchId)
        {
            var isViewAll = await IsGrantedAsync(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserProjectForBranchs_ViewAllBranchs);
            var currentUserBranch = WorkScope.GetAll<User>()
                                    .Where(s => s.Id == AbpSession.UserId)
                                    .Select(s => s.BranchId).FirstOrDefault();

            var qprojectUsers = from pu in WorkScope.GetAll<ProjectUser>().Where(s => s.User.IsActive == true)
                                join p in WorkScope.GetAll<Project>().Where(s => s.Status == ProjectStatus.Active) on pu.ProjectId equals p.Id
                                where pu.Type != ProjectUserType.DeActive
                                select new
                                {
                                    pu.ProjectId,
                                    p.Code,
                                    p.Name,
                                    pu.UserId,
                                    pu.Type
                                };

            var query = from u in WorkScope.GetAll<User>()
                        join pu in qprojectUsers on u.Id equals pu.UserId into pusers
                        join mu in WorkScope.GetAll<User>() on u.ManagerId equals mu.Id into muu
                        select new UserProjectsDto
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Name = u.Name,
                            Surname = u.Surname,
                            FullName = u.FullName,
                            Address = u.Address,
                            IsActive = u.IsActive,
                            EmailAddress = u.EmailAddress,
                            PhoneNumber = u.PhoneNumber,
                            ProjectUsers = pusers.Select(s => new PUDto
                            {
                                ProjectId = s.ProjectId,
                                ProjectName = s.Name,
                                ProjectCode = s.Code,
                                ProjectUserType = s.Type
                            }).ToList(),
                            Type = u.Type,
                            UserCode = u.UserCode,
                            BranchDisplayName = u.Branch.DisplayName,
                            BranchId = u.BranchId,
                            AvatarPath = u.AvatarPath,
                            ManagerId = u.ManagerId,
                            Sex = u.Sex,
                            CreationTime = u.CreationTime,
                            ManagerName = muu.FirstOrDefault() != null ? muu.FirstOrDefault().FullName : "",
                            ManagerAvatarPath = muu.FirstOrDefault() != null ? muu.FirstOrDefault().AvatarPath : "",
                            PositionId = u.Position.Id,
                            PositionName = u.Position.Name,
                            ProjectCount = pusers.Count(),
                        };
            if (!isViewAll)
            {
                query = query.WhereIf(positionId != null, s => s.PositionId == positionId)
                             .Where(s => s.BranchId == currentUserBranch)
                             .OrderByDescending(s => s.CreationTime);
            }
            else
            {
                query = query.WhereIf(positionId != null, s => s.PositionId == positionId)
                             .WhereIf(branchId != null, s => s.BranchId == branchId)
                             .OrderByDescending(s => s.CreationTime);
            }
            var temp = await query.GetGridResult(query, input);

            var projectIds = new HashSet<long>();
            foreach (var user in temp.Items)
            {
                projectIds.UnionWith(user.ProjectUsers.Select(s => s.ProjectId));
            }

            var projects = (WorkScope.GetAll<ProjectUser>()
                    .Where(s => projectIds.Contains(s.ProjectId) && s.Type == ProjectUserType.PM)
                    .Select(s => new { s.ProjectId, s.User.FullName })
                    .GroupBy(s => s.ProjectId))
                    .Select(s => new { s.Key, pms = s.Select(f => f.FullName).ToList() }).ToList();

            foreach (var user in temp.Items)
            {
                foreach (var pu in user.ProjectUsers)
                {
                    pu.Pms = projects.Where(s => s.Key == pu.ProjectId).Select(s => s.pms).FirstOrDefault();
                }
            }
            return new PagedResultDto<UserProjectsDto>(temp.TotalCount, temp.Items);
        }


    }
}
