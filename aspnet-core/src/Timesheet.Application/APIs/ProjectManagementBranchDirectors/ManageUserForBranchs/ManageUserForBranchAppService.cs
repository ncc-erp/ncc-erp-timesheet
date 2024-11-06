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
using System.Linq.Dynamic.Core;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs
{
    public class ManageUserForBranchAppService : AppServiceBase
    {
        public ManageUserForBranchAppService(IWorkScope workScope) : base(workScope) { }

        private IQueryable<ProjectUserInfoDto> QProjectUser(long? branchId)
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
                .Where(s => s.Project.Status == ProjectStatus.Active
                && !s.Project.isAllUserBelongTo)
                .Where(s => s.User.IsActive)
                .Where(predicate)
                .Select(s => new ProjectUserInfoDto
                {
                    ProjectId = s.ProjectId,
                    Code = s.Project.Code,
                    Name = s.Project.Name,
                    UserId = s.UserId,
                    BranchId = s.User.BranchId,
                });
        }

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<PagedResultDto<UserProjectsDto>> GetAllUserPagging(GridParam input, long? positionId, long? branchId, DateTime? startDate, DateTime? endDate, int sortType, int compare)
        {
            var qProjectUser = QProjectUser(branchId);
            var qMyTimeSheet = from th in WorkScope.GetAll<MyTimesheet>()
                               .WhereIf(startDate.HasValue && endDate.HasValue, s => startDate <= s.DateAt && s.DateAt <= endDate)
                               .Where(s => s.Status == TimesheetStatus.Approve)
                               .Where(s => !s.ProjectTask.Project.isAllUserBelongTo)
                               .Where(s => s.ProjectTask.Project.Status == ProjectStatus.Active)
                               select new
                               {
                                   UserId = th.UserId,
                                   ProjectId = th.ProjectTask.ProjectId,
                                   WorkingTime = th.WorkingTime
                               };
            var workingTimeProject = qMyTimeSheet.GroupBy(s => s.UserId)
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));
            var workingTimeUserProject = qMyTimeSheet.GroupBy(s => s.ProjectId.ToString() + "-" + s.UserId.ToString())
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));
            var projectIds = new HashSet<long>();
            var userIds = new HashSet<long>();
            foreach (var jq in qMyTimeSheet)
            {
                userIds.Add(jq.UserId);
                projectIds.Add(jq.ProjectId);
            }
            var projects = WorkScope.GetAll<ProjectUser>().AsNoTracking()
                            .Where(s => projectIds.Contains(s.ProjectId) && s.Type == ProjectUserType.PM)
                            .Select(s => new { s.ProjectId, s.User.FullName })
                            .GroupBy(s => s.ProjectId)
                            .Select(s => new { s.Key, pms = s.Select(f => f.FullName).ToList() }).ToList();
            var pud = qProjectUser.Where(s => projectIds.Contains(s.ProjectId)).Select(s => new
            {
                UserId = s.UserId,
                Pms = projects.Where(p => p.Key == s.ProjectId).Select(p => p.pms).FirstOrDefault(),
                ProjectId = s.ProjectId,
                ProjectName = s.Name,
                ProjectCode = s.Code,
                WorkingTimePercent = workingTimeUserProject.ContainsKey(s.ProjectId + "-" + s.UserId) ? (int)Math.Round((double)workingTimeUserProject[s.ProjectId + "-" + s.UserId] * 100 / workingTimeProject[s.UserId]) : 0
            }).Where(s => s.WorkingTimePercent > 0).OrderByDescending(s => s.WorkingTimePercent).ThenBy(s => s.ProjectName);
            var query = from u in WorkScope.GetAll<User>().Where(s => s.IsActive).Where(s => userIds.Contains(s.Id))
                        select new
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            FullName = u.FullName,
                            Type = u.Type,
                            Level = u.Level,
                            Sex = u.Sex,
                            EmailAddress = u.EmailAddress,
                            AvatarPath = u.AvatarPath,
                            BranchDisplayName = u.Branch.Name,
                            BranchId = u.BranchId,
                            PositionId = u.PositionId,
                            PositionName = u.Position.Name,
                            ProjectUsers = pud.Where(p => p.UserId == u.Id).Select(p => new PUDto
                            {
                                Pms = p.Pms,
                                ProjectId = p.ProjectId,
                                ProjectName = p.ProjectName,
                                ProjectCode = p.ProjectCode,
                                WorkingTimePercent = p.WorkingTimePercent
                            }).ToList(),
                            ProjectCount = pud.Where(p => p.UserId == u.Id).Count(),
                            SortProjectInfo = pud.Where(p => p.UserId == u.Id).FirstOrDefault()
                        };
            if (sortType == (int)ESortType.PROJECT)
            {
                switch (compare)
                {
                    case (int)ESortProjectUserNumber.UP_PROJECT:
                        query = query.OrderBy(user => user.SortProjectInfo.WorkingTimePercent).ThenBy(user => user.SortProjectInfo.ProjectName);
                        break;
                    case (int)ESortProjectUserNumber.DOWN_PROJECT:
                        query = query.OrderByDescending(user => user.SortProjectInfo.WorkingTimePercent).ThenBy(user => user.SortProjectInfo.ProjectName);
                        break;
                }
            }
            else if (sortType == (int)ESortType.NUMBER)
            {
                switch (compare)
                {
                    case (int)ESortProjectUserNumber.UP_NUMBER:
                        query = query.OrderBy(user => user.ProjectCount);
                        break;
                    case (int)ESortProjectUserNumber.DOWN_NUMBER:
                        query = query.OrderByDescending(user => user.ProjectCount);
                        break;
                }
            }
            else if (sortType == (int)ESortType.LEVEL)
            {
                switch (compare)
                {
                    case (int)ESortProjectUserNumber.UP_LEVEL:
                        query = query.OrderBy(user => user.Level);
                        break;
                    case (int)ESortProjectUserNumber.DOWN_LEVEL:
                        query = query.OrderByDescending(user => user.Level);
                        break;
                }
            }
            var queryRes = query.Select(u => new UserProjectsDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Type = u.Type,
                Level = u.Level,
                Sex = u.Sex,
                EmailAddress = u.EmailAddress,
                AvatarPath = u.AvatarPath,
                BranchDisplayName = u.BranchDisplayName,
                BranchId = u.BranchId,
                PositionId = u.PositionId,
                PositionName = u.PositionName,
                ProjectUsers = u.ProjectUsers,
                ProjectCount = u.ProjectCount,

            });
            var temp = await queryRes.GetGridResult(queryRes, input);
            return new PagedResultDto<UserProjectsDto>(temp.TotalCount, temp.Items);
        }

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<PagedResultDto<UserStatisticInProjectDto>> GetStatisticNumOfUsersInProject(GridParam input, long? branchId, DateTime? startDate, DateTime? endDate)
        {
            var isViewAll = IsGranted(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs);

            var qUserTimesheet = await WorkScope.GetAll<MyTimesheet>()
                               .WhereIf(startDate.HasValue && endDate.HasValue, s => startDate <= s.DateAt && s.DateAt <= endDate)
                               .Where(s => s.User.IsActive)
                               .WhereIf(isViewAll, s => branchId == null || s.User.BranchId == branchId) // If have ViewAll
                               .WhereIf(!isViewAll, s => s.User.BranchId == GetBranchByCurrentUser()) // If have no ViewAll
                               .Where(s => s.Status == TimesheetStatus.Approve)
                               .Select(ts => string.Format("{0}-{1}", ts.UserId, ts.ProjectTask.ProjectId))
                               .Distinct()
                               .ToListAsync();
                               

            // Query the ProjectUser table to get user types, project detail -> result
            var result = WorkScope.GetAll<ProjectUser>()
                               .Where(pu => qUserTimesheet.Contains(string.Format("{0}-{1}", pu.UserId, pu.ProjectId)))
                               .GroupBy(pu => new { pu.ProjectId, ProjectName = pu.Project.Name, ProjectCode = pu.Project.Code })
                               .Select(g => new UserStatisticInProjectDto
                               {
                                   ProjectId = g.Key.ProjectId,
                                   ProjectName = g.Key.ProjectName,
                                   ProjectCode = g.Key.ProjectCode,
                                   TotalUser = g.Count(),
                                   MemberCount = g.Count(e => e.Type == ProjectUserType.Member),
                                   DeactiveCount = g.Count(e => e.Type == ProjectUserType.DeActive),
                                   ShadowCount = g.Count(e => e.Type == ProjectUserType.Shadow),
                                   PmCount = g.Count(e => e.Type == ProjectUserType.PM)
                               })
                               .OrderByDescending(e => e.TotalUser);

            var temp = await result.GetGridResult(result, input);
            return new PagedResultDto<UserStatisticInProjectDto>(temp.TotalCount, temp.Items);
        }

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<List<UserInfoProjectDto>> GetAllUserInProject(long projectId, long? branchId, DateTime? startDate, DateTime? endDate)
        {
            var qMyTimeSheet = await WorkScope.GetAll<MyTimesheet>()
                               .WhereIf(startDate.HasValue && endDate.HasValue, s => startDate <= s.DateAt && s.DateAt <= endDate)
                               .Where(s => s.User.IsActive)
                               .Where(s => s.Status == TimesheetStatus.Approve)
                               .Where(s => s.ProjectTask.Project.Id == projectId)
                               .Select(th => new
                               {
                                   BranchId = th.User.BranchId,
                                   UserId = th.UserId,
                                   WorkingTime = th.WorkingTime
                               }).ToListAsync();

            var totalTime = qMyTimeSheet.Sum(i => i.WorkingTime);
            var workingTimeProject = qMyTimeSheet.Where(s => branchId == null || s.BranchId == branchId).GroupBy(s => s.UserId)
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));

            var projectUserType = await WorkScope.GetAll<ProjectUser>()
                .Where(pu => qMyTimeSheet.Select(e => e.UserId).Contains(pu.UserId))
                .Where(pu => pu.ProjectId == projectId)
                .Select(pu => new { pu.UserId, pu.Type, pu.Id })
                .ToDictionaryAsync(e => e.UserId, e => new { e.Type, ProjectUserId = e.Id });

            var query = from u in WorkScope.GetAll<User>().Where(s => s.IsActive).Where(s => workingTimeProject.ContainsKey(s.Id))
                        select new UserInfoProjectDto
                        {
                            FullName = u.FullName,
                            EmailAddress = u.EmailAddress,
                            WorkingPercent = (float)workingTimeProject[u.Id] * 100 / totalTime,
                            TotalWorkingTime = workingTimeProject[u.Id],
                            UserType = projectUserType[u.Id].Type,
                            ProjectUserId = projectUserType[u.Id].ProjectUserId
                        };

            return await query.ToListAsync();
        }

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(
            Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, 
            Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch,
            Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserProjectForBranchs)]
        public async System.Threading.Tasks.Task UpdateTypeOfUsersInProject([FromBody] UpdateTypeOfUsersInProjectDto dto)
        {
            var projectUserIds = dto.UserTypes.Select(e => e.ProjectUserId).ToList();
            
            // Get all ProjectUsers from given Id
            var projectUsers = await WorkScope.GetAll<ProjectUser>()
                .Where(e => projectUserIds.Contains(e.Id))
                .ToListAsync();

            // Generate map from dto and update project users
            var newProjectUserTypeMap = dto.UserTypes.ToDictionary(e => e.ProjectUserId, e => e.UserType);
            foreach (var pu in projectUsers)
            {
                pu.Type = newProjectUserTypeMap[pu.Id];
            }
            // Bulk update
            await WorkScope.UpdateRangeAsync<ProjectUser>(projectUsers);
        }
    }
}
