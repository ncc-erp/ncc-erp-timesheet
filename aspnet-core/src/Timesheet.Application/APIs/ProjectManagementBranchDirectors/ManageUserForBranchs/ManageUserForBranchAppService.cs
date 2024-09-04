﻿using Abp.Application.Services.Dto;
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
using Timesheet.Uitls;

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
                               .Where(s => s.User.IsActive)
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
            var projectIds = qMyTimeSheet.Select(s => s.ProjectId).ToHashSet();
            var userIds = qMyTimeSheet.Select(s => s.UserId).ToHashSet();

            var qTimeKeeping = from th in WorkScope.GetAll<Timekeeping>()
                               .WhereIf(startDate.HasValue && endDate.HasValue, s => startDate <= s.DateAt && s.DateAt <= endDate)
                               .Where(s => s.UserId.HasValue && userIds.Contains(s.UserId.Value))
                               select new
                               {
                                   UserId = th.UserId,
                                   DateAt = th.DateAt,
                                   WorkingTime = String.IsNullOrEmpty(th.CheckIn) || String.IsNullOrEmpty(th.CheckOut) ? 0 : CommonUtils.SubtractHHmm(th.CheckOut,th.CheckIn),
                                   wawarningCheckInFromPersonalDevice = th.warningCheckInFromPersonalDevice
                               };

            var WOATime = qTimeKeeping.Where(s => s.wawarningCheckInFromPersonalDevice.HasValue && !s.wawarningCheckInFromPersonalDevice.Value).GroupBy(s => s.UserId)
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));
            var WFHTime = qTimeKeeping.Where(s => s.wawarningCheckInFromPersonalDevice.HasValue && s.wawarningCheckInFromPersonalDevice.Value).GroupBy(s => s.UserId)
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));
            var TotalWorkingTime = qTimeKeeping.Where(s => s.wawarningCheckInFromPersonalDevice.HasValue).GroupBy(s => s.UserId)
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));
            var projects = WorkScope.GetAll<ProjectUser>()
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
                            ProjectUsers = pud.Where(p => p.UserId == u.Id).Select(p => new PUDto
                            {
                                Pms = p.Pms,
                                ProjectId = p.ProjectId,
                                ProjectName = p.ProjectName,
                                ProjectCode = p.ProjectCode,
                                WorkingTimePercent = p.WorkingTimePercent
                            }).ToList(),
                            ProjectCount = pud.Where(p => p.UserId == u.Id).Count(),
                            WOAPercent = WOATime.ContainsKey(u.Id) && TotalWorkingTime[u.Id] != 0 ? WOATime[u.Id] * 100 / TotalWorkingTime[u.Id] : 0,
                            WFHPercent = WFHTime.ContainsKey(u.Id) && TotalWorkingTime[u.Id] != 0 ? WFHTime[u.Id] * 100 / TotalWorkingTime[u.Id] : 0,
                        };
            var temp = await query.GetGridResult(query, input);
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
                               .Where(s => s.User.IsActive)
                               .Where(s=>branchId == null || s.User.BranchId == branchId)
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
        public async Task<List<UserInfoProjectDto>> GetAllUserInProject(long projectId, long? branchId, DateTime? startDate, DateTime? endDate)
        {
            var qMyTimeSheet = from th in WorkScope.GetAll<MyTimesheet>()
                               .WhereIf(startDate.HasValue && endDate.HasValue, s => startDate <= s.DateAt && s.DateAt <= endDate)
                               .Where(s => s.User.IsActive)
                               .Where(s => s.Status == TimesheetStatus.Approve)
                               .Where(s => s.ProjectTask.Project.Id == projectId)
                               select new
                               {
                                   BranchId = th.User.BranchId,
                                   UserId = th.UserId,
                                   WorkingTime = th.WorkingTime
                               };
            var totalTime = qMyTimeSheet.Sum(i => i.WorkingTime);
            var workingTimeProject = qMyTimeSheet.Where(s=> branchId == null || s.BranchId == branchId).GroupBy(s => s.UserId)
                                    .ToDictionary(g => g.Key, g => g.Sum(i => i.WorkingTime));
            var query = from u in WorkScope.GetAll<User>().Where(s => s.IsActive).Where(s => workingTimeProject.ContainsKey(s.Id))
                        select new UserInfoProjectDto
                        {
                            FullName = u.FullName,
                            EmailAddress = u.EmailAddress,
                            WorkingPercent = (float)workingTimeProject[u.Id] * 100 / totalTime,
                            ValueType = WorkScope.GetAll<ValueOfUserInProject>()
                                                .Where(p => p.UserId == u.Id && p.ProjectId == projectId).Select(p => p.Type)
                                                .FirstOrDefault(),
                        };

            return query.ToList();
        }
    }
}
