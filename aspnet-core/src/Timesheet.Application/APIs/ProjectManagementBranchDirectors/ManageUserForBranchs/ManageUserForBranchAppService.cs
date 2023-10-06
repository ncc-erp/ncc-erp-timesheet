using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Linq.Extensions;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Office.Interop.Word;
using Ncc;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Ncc.IoC;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranch.Dto;
using Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserForBranchs.Dto;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Extension;
using Timesheet.Paging;
using Timesheet.Uitls;
using Timesheet.Users.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagementBranchDirectors.ManageUserProjectForBranchs
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
            var currentUserBranch = WorkScope.GetAll<User>()
                                    .Where(s => s.Id == AbpSession.UserId)
                                    .Select(s => s.BranchId).FirstOrDefault();

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
                    .Where(s => s.BranchId == currentUserBranch)
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
            var currentUserBranch = WorkScope.GetAll<User>()
                                    .Where(s => s.Id == AbpSession.UserId)
                                    .Select(s => s.BranchId).FirstOrDefault();

            var qprojectUsers = from p in WorkScope.GetAll<Project>().Where(s => s.Status == ProjectStatus.Active)
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


            var qvalueUserProject = await (from v in WorkScope.GetAll<ValueOfUserInProject>()
                                    group v by new { v.UserId, v.ProjectId } into grouped
                                    select grouped.OrderByDescending(v => v.CreationTime).First()).ToListAsync();

            var qprojectUsersList = new List<UserValueProjectDto>();

            if (isViewAll)
            {
                qprojectUsersList = qprojectUsers.WhereIf(branchId != null, s => s.Users.Any(u => u.BranchId == branchId)).OrderByDescending(s => s.Id).ToList();
            }
            else
            {
                qprojectUsersList = qprojectUsers.Where(s => s.Users.Any(u => u.BranchId == currentUserBranch)).OrderByDescending(s => s.Id).ToList();
            }

            foreach (var userValueProject in qprojectUsersList)
            {
                foreach (var userValue in userValueProject.Users)
                {
                    var matchingValue = qvalueUserProject.FirstOrDefault(v =>
                        v.UserId == userValue.UserId && v.ProjectId == userValueProject.Id);

                    if (matchingValue != null)
                    {
                        userValue.ValueType = matchingValue.Type;
                    }
                    else
                    {
                        userValue.ValueType = ValueOfUserType.Member;
                    }
                }
            }

            var query = from p in qprojectUsersList
                        select new UserStatisticInProjectDto
                        {
                            ProjectId = p.Id,
                            ProjectCode = p.ProjectCode,
                            ProjectName = p.ProjectName,
                            TotalUser = p.Users.Count(),
                            MemberCount = p.Users.Count(s => s.ValueType == ValueOfUserType.Member),
                            ExposeCount = p.Users.Count(s => s.ValueType == ValueOfUserType.Expose),
                            ShadowCount = p.Users.Count(s => s.ValueType == ValueOfUserType.Shadow)
                        };

            //search
            if (!string.IsNullOrEmpty(input.SearchText))
            {
                query = query.Where(item => item.ProjectName.Trim().ToLowerInvariant().Contains(input.SearchText.Trim().ToLowerInvariant()));
            }
            //sort
            if (!string.IsNullOrEmpty(input.Sort))
            {
                switch (input.Sort)
                {
                    case "TotalUser":
                        query = (input.SortDirection == SortDirection.DESC)
                            ? query.OrderByDescending(item => item.TotalUser)
                            : query.OrderBy(item => item.TotalUser);
                        break;

                    case "ProjectName":
                        query = (input.SortDirection == SortDirection.DESC)
                            ? query.OrderByDescending(item => item.ProjectName)
                            : query.OrderBy(item => item.ProjectName);
                        break;

                    default:
                        break;
                }
            }
            //paging
            var pagedQuery = query.Skip((input.SkipCount - 1) * input.MaxResultCount).Take(input.MaxResultCount);
            //pagedQuery.ToList();
            return new PagedResultDto<UserStatisticInProjectDto>(pagedQuery.Count(), pagedQuery.ToList());
        }
    }
}
