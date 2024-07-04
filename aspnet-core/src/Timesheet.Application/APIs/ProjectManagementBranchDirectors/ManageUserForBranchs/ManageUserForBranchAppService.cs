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
using Timesheet.Timesheets.MyTimesheets.Dto;
using Timesheet.Timesheets.Projects.Dto;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using DocumentFormat.OpenXml.Spreadsheet;

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
        public async Task<PagedResultDto<UserProjectsDto>> GetAllUserPagging(GridParam input, long? positionId, long? branchId, DateTime? startDate, DateTime? endDate)
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
            var query = from u in WorkScope.GetAll<User>().Where(s => s.IsActive).Where(s => userIds.Contains(s.Id))
                        join pu in qProjectUser.Where(s => projectIds.Contains(s.ProjectId)) on u.Id equals pu.UserId into puu
                        select new UserProjectsDto
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
                            ProjectUsers = puu.Select(s => new PUDto
                            {
                                ProjectId = s.ProjectId,
                                ProjectName = s.Name,
                                ProjectCode = s.Code,
                            }).ToList(),
                            ProjectCount = puu.Count(),
                        };
            query = query.OrderByDescending(u => u.ProjectCount);
            var temp = await query.GetGridResult(query, input);
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
                    string workingTimeUserProjectKey = pu.ProjectId.ToString() + "-" + user.Id.ToString();
                    long workingTimeProjectKey = user.Id;
                    if (workingTimeUserProject.ContainsKey(workingTimeUserProjectKey) && workingTimeProject.ContainsKey(workingTimeProjectKey))
                    {
                        pu.WorkingTimePercent = (int)Math.Round((double)workingTimeUserProject[workingTimeUserProjectKey] * 100 / workingTimeProject[workingTimeProjectKey]);
                    }
                }
                user.ProjectUsers = user.ProjectUsers.Where(s => s.WorkingTimePercent > 0).OrderByDescending(s => s.WorkingTimePercent).ToList();
                user.ProjectCount = user.ProjectUsers.Count();
            }

            return new PagedResultDto<UserProjectsDto>(temp.TotalCount, temp.Items);
        }

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<PagedResultDto<UserStatisticInProjectDto>> GetStatisticNumOfUsersInProject(GridParam input, long? branchId, DateTime? startDate, DateTime? endDate)
        {
            var qProjectUser = QProjectUser(branchId);

            var qUserProject = from th in WorkScope.GetAll<MyTimesheet>()
                               .WhereIf(startDate.HasValue && endDate.HasValue,s => startDate <= s.DateAt && s.DateAt <= endDate)
                               .Where(s => s.Status == TimesheetStatus.Approve)
                               group th by new { th.UserId, th.ProjectTask.ProjectId } into g
                               select new
                               {
                                   UserId = g.Key.UserId,
                                   ProjectId = g.Key.ProjectId
                               };
            var projectIds = new HashSet<long>();
            var user_project = new HashSet<string>();
            foreach (var up in qUserProject)
            {
                user_project.Add(up.UserId.ToString()+ "-" + up.ProjectId.ToString());
                projectIds.Add(up.ProjectId);
            }
            var query = qProjectUser
                .Where(s=> projectIds.Contains(s.ProjectId))
                .GroupBy(s => s.ProjectId)
                .Select(s => new UserValueProjectDto
                {
                    Id = s.Key,
                    ProjectCode = s.Select(x => x.Code).FirstOrDefault(),
                    ProjectName = s.Select(x => x.Name).FirstOrDefault(),
                    Users = s.Where(x=> user_project.Contains(x.UserId.ToString() + "-" +x.ProjectId.ToString())).Select(x => new UserValueDto
                    {
                        UserId = x.UserId,
                        BranchId = x.BranchId,
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

        [HttpPost]
        [AbpAuthorize]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewAllBranchs, Ncc.Authorization.PermissionNames.ProjectManagementBranchDirectors_ManageUserForBranchs_ViewMyBranch)]
        public async Task<List<UserInfoProjectDto>> GetAllUserInProject(long projectId, DateTime? startDate, DateTime? endDate)
        {
            var qMyTimeSheet = from th in WorkScope.GetAll<MyTimesheet>()
                               .WhereIf(startDate.HasValue && endDate.HasValue, s => startDate <= s.DateAt && s.DateAt <= endDate)
                               .Where(s => s.Status == TimesheetStatus.Approve)
                               .Where(s => s.ProjectTask.Project.Id == projectId)
                               select new
                               {
                                   UserId = th.UserId,
                                   WorkingTime = th.WorkingTime
                               };
            var workingTimeProject = qMyTimeSheet.GroupBy(s => s.UserId)
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));
            var query = from u in WorkScope.GetAll<User>().Where(s => s.IsActive).Where(s => workingTimeProject.ContainsKey(s.Id))
                        select new UserInfoProjectDto
                        {
                            FullName = u.FullName,
                            EmailAddress = u.EmailAddress,
                            WorkingTime = workingTimeProject[u.Id],
                            ValueType = WorkScope.GetAll<ValueOfUserInProject>()
                                                .Where(p => p.UserId == u.Id && p.ProjectId == projectId).Select(p => p.Type)
                                                .FirstOrDefault(),
                        };

            return query.ToList();
        }
    }
}
