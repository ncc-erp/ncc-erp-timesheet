using Abp.Application.Services.Dto;
using Abp.Authorization;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using Ncc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs.Dto;
using Timesheet.Entities;
using Timesheet.Paging;
using Microsoft.EntityFrameworkCore;
using Abp.Linq.Extensions;
using Timesheet.Users.Dto;
using static Ncc.Entities.Enum.StatusEnum;
using Microsoft.AspNetCore.Mvc;
using Timesheet.Extension;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs
{
    public class ManageUserForBranchAppService : AppServiceBase
    {
        public ManageUserForBranchAppService(IWorkScope workScope) : base(workScope) { }

        private IQueryable<ProjectUser> QProjectUser(long? branchId)
        {
            var isViewAll = IsGranted(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs);

            IQueryable<UserProjectsDto> query;
            var predicate = PredicateBuilder.New<ProjectUser>();

            if (isViewAll)
            {
                predicate.And(s => branchId == null || s.User.BranchId == branchId);
            }
            else
            {
                predicate.And(s => s.User.BranchId == GetBranchByCurrentUser());
            }

            return WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Project.Status == ProjectStatus.Active && !s.Project.isAllUserBelongTo)
                .Where(s => s.User.IsActive)
                .Where(predicate)
                .AsQueryable();
        }

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<PagedResultDto<UserProjectsDto>> GetAllUserPagging(GridParam input, long? positionId, long? branchId)
        {
            var qProjectUser = QProjectUser(branchId);

            var query = qProjectUser
                .GroupBy(s => s.UserId)
                .Select(s => new UserProjectsDto
                {
                    Id = s.Key,
                    UserName = s.Select(x => x.User.Name).FirstOrDefault(),
                    FullName = s.Select(x => x.User.FullName).FirstOrDefault(),
                    Type = s.Select(x => x.User.Type).FirstOrDefault(),
                    Level = s.Select(x => x.User.Level).FirstOrDefault(),
                    EmailAddress = s.Select(x => x.User.EmailAddress).FirstOrDefault(),
                    ProjectUsers = s.Select(x => new PUDto
                    {
                        ProjectId = x.ProjectId,
                        ProjectName = x.Project.Name,
                        ProjectCode = x.Project.Code,
                    }).ToList(),
                    BranchDisplayName = s.Select(x => x.User.Branch.DisplayName).FirstOrDefault(),
                    BranchId = s.Select(x => x.User.BranchId).FirstOrDefault(),
                    AvatarPath = s.Select(x => x.User.AvatarPath).FirstOrDefault(),
                    PositionId = s.Select(x => x.User.PositionId).FirstOrDefault(),
                    PositionName = s.Select(x => x.User.Position.Name).FirstOrDefault(),
                    ProjectCount = s.Select(x => x.ProjectId).Count(),
                });

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

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<PagedResultDto<UserStatisticInProjectDto>> GetStatisticNumOfUsersInProject(GridParam input, long? branchId)
        {
            var qProjectUser = QProjectUser(branchId);

            var query = qProjectUser
                .GroupBy(s => s.ProjectId)
                .Select(s => new UserValueProjectDto
                {
                    Id = s.Key,
                    ProjectCode = s.Select(x => x.Project.Code).FirstOrDefault(),
                    ProjectName = s.Select(x => x.Project.Name).FirstOrDefault(),
                    Users = s.Select(x => new UserValueDto
                    {
                        UserId = x.UserId,
                        BranchId = x.User.BranchId,
                        Name = x.User.Name,
                        ValueType = WorkScope.GetAll<ValueOfUserInProject>()
                            .Where(p => p.UserId == x.UserId && p.ProjectId == s.Key).Select(p => p.Type)
                            .FirstOrDefault(),
                    }).ToList()
                });

            var result = (from voup in WorkScope.GetAll<ProjectUser>() 
                join p in query on voup.ProjectId equals p.Id
                        select new UserStatisticInProjectDto
                        {
                            ProjectId = p.Id,
                            ProjectCode = p.ProjectCode,
                            ProjectName = p.ProjectName,
                            TotalUser = p.Users.Count(),
                            MemberCount = p.Users.Count(s => s.ValueType == ValueOfUserType.Member),
                            ExposeCount = p.Users.Count(s => s.ValueType == ValueOfUserType.Expose),
                            ShadowCount = p.Users.Count(s => s.ValueType == ValueOfUserType.Shadow)
                        }).DistinctBy(s => s.ProjectId);

            var temp = await result.GetGridResult(result, input);
            return new PagedResultDto<UserStatisticInProjectDto>(temp.TotalCount, temp.Items);
        }
    }
}
