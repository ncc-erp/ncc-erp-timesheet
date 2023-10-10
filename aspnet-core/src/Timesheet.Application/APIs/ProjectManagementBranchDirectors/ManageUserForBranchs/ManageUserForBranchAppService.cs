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


        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<PagedResultDto<UserProjectsDto>> GetAllUserPagging(GridParam input, long? positionId, long? branchId)
        {
            var isViewAll = await IsGrantedAsync(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs);

            var qprojectUsers = from pu in WorkScope.GetAll<ProjectUser>().Where(s => s.User.IsActive == true)
                                join p in WorkScope.GetAll<Project>().Where(s => s.Status == ProjectStatus.Active) on pu.ProjectId equals p.Id
                                //where pu.Type != ProjectUserType.DeActive
                                select new
                                {
                                    pu.ProjectId,
                                    p.Code,
                                    p.Name,
                                    pu.UserId,
                                    pu.Type
                                };

            var qUserFilterBranch = WorkScope.GetAll<User>()
                    .WhereIf(positionId != null, s => s.PositionId == positionId);

            if (!isViewAll)
            {
                qUserFilterBranch = qUserFilterBranch
                    .Where(s => s.BranchId == GetBranchByCurrentUser())
                    .OrderByDescending(s => s.CreationTime);
            }
            else
            {
                qUserFilterBranch = qUserFilterBranch
                    .WhereIf(branchId != null, s => s.BranchId == branchId)
                    .OrderByDescending(s => s.CreationTime);
            }

            var query = from u in qUserFilterBranch
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
                            Level = u.Level,
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
            var isViewAll = await IsGrantedAsync(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs);

            var qprojectUsers = from p in WorkScope.GetAll<Project>().Where(s => s.Status == ProjectStatus.Active && !s.isAllUserBelongTo)
                                join pu in WorkScope.GetAll<ProjectUser>().Where(s => s.User.IsActive == true) on p.Id equals pu.ProjectId into ppu
                                select new UserValueProjectDto
                                {
                                    Id = p.Id,
                                    ProjectCode = p.Code,
                                    ProjectName = p.Name,
                                    Users = ppu.Select(s => new UserValueDto
                                    {
                                        UserId = s.UserId,
                                        BranchId = s.User.BranchId,
                                        Name = s.User.Name,
                                    }).ToList(),
                                };

            IQueryable<UserValueProjectDto> query;
            var predicate = PredicateBuilder.New<ProjectUser>();

            if (isViewAll)
            {
                predicate.And(s => branchId == null || s.User.BranchId == branchId);
            }
            else
            {
                predicate.And(s => s.User.BranchId == GetBranchByCurrentUser());
            }

             query = WorkScope.GetAll<ProjectUser>()
                .Where(s => s.Project.Status == ProjectStatus.Active && !s.Project.isAllUserBelongTo)
                .Where(s => s.User.IsActive)
                .Where(predicate)
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
