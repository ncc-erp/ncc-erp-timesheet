using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Helper;
using Timesheet.Services.Komu;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public class UserServices : BaseDomainService, IUserServices, ITransientDependency
    {
        private readonly UserManager _userManager;
        private readonly IAbpSession _abpSession;
        private readonly KomuService _komuService;
        private readonly RoleManager _roleManager;
        private readonly IWorkScope _workScope;
        public UserServices(UserManager userManager, KomuService komuService, IAbpSession abpSession, RoleManager roleManager, IObjectMapper objectMapper, IWorkScope workScope) : base(workScope)
        {
            _userManager = userManager;
            _abpSession = abpSession;
            _roleManager = roleManager;
            _komuService = komuService;
        }
        public IQueryable<User> GetUserByRole(string roleName)
        {
            var quser =
                from u in WorkScope.GetAll<User, long>()
                join r in
                    from ur in WorkScope.GetAll<UserRole, long>()
                    join role in WorkScope.GetAll<Role, int>().Where(s => s.Name == roleName)
                    on ur.RoleId equals role.Id
                    select ur.UserId
                on u.Id equals r into roles
                where roles.Any()
                select u;

            return quser;
        }

        public bool UserHasRole(long userId, string roleName)
        {
            var quser =
                    from ur in WorkScope.GetAll<UserRole, long>().Where(u => u.UserId == userId)
                    join role in WorkScope.GetAll<Role, int>().Where(s => s.Name == roleName)
                    on ur.RoleId equals role.Id into roles
                    from r in roles
                    select r.Id;
            return quser.Any();

        }

        public IQueryable<string> UserAllRoles(long userId)
        {
            var userAllroles = from u in WorkScope.GetAll<UserRole, long>().Where(u => u.UserId == userId)
                               join r in WorkScope.GetAll<Role, int>()
                               on u.RoleId equals r.Id into roles
                               from r in roles
                               select r.Name;
            return userAllroles;
        }

        private async Task<Entities.Branch> GetBranchByCode(string code)
        {
            var branch = await WorkScope.GetAll<Entities.Branch>().Where(s => s.Code == code).FirstOrDefaultAsync();
            if (branch == default)
            {
                branch = await WorkScope.GetAll<Entities.Branch>().FirstOrDefaultAsync();
            }
            return branch;
        }


        public async Task<User> CreateUserAsync(CreateUserDto input)
        {
            var user = ObjectMapper.Map<User>(input);
            user.TenantId = _abpSession.TenantId;
            user.UserName = input.UserName?.Replace("@gmail.com", "");
            user.IsEmailConfirmed = true;
            user.PhoneNumber = input.PhoneNumber;
            user.isWorkingTimeDefault = true;
            user.NormalizedUserName = input.UserName.ToLower();
            user.NormalizedEmailAddress = input.EmailAddress.ToLower();


            if (input.BranchCode != null)
            {
                var branch = await GetBranchByCode(input.BranchCode);
                user.MorningStartAt = branch.MorningStartAt;
                user.MorningEndAt = branch.MorningEndAt;
                user.MorningWorking = branch.MorningWorking;
                user.AfternoonStartAt = branch.AfternoonStartAt;
                user.AfternoonEndAt = branch.AfternoonEndAt;
                user.AfternoonWorking = branch.AfternoonWorking;
            }
            else
            {
                user.MorningStartAt = input.MorningStartAt;
                user.MorningEndAt = input.MorningEndAt;
                user.MorningWorking = double.Parse(input.MorningWorking);
                user.AfternoonStartAt = input.AfternoonStartAt;
                user.AfternoonEndAt = input.AfternoonEndAt;
                user.AfternoonWorking = double.Parse(input.AfternoonWorking);
            }
            await _userManager.InitializeOptionsAsync(_abpSession.TenantId);
            await _userManager.CreateAsync(user, input.Password);
            //Default role BasicUser
            input.RoleNames = new string[] { StaticRoleNames.Host.BasicUser };
            await _userManager.SetRoles(user, input.RoleNames);
            //input.Id =  WorkScope.InsertAndGetId(user);
            CurrentUnitOfWork.SaveChanges();
            //if (input.RoleNames != null)
            //{
            //    //_userManager.SetRoles(user, input.RoleNames)
            //    var roleId = _roleManager.GetRoleByNameAsync(input.RoleNames.FirstOrDefault()).Result.Id;
            //    WorkScope.Insert<UserRole>(new UserRole
            //    {
            //        RoleId = roleId,
            //        UserId = input.Id
            //    });
            //}
            var q = WorkScope.GetAll<Project>()
                    .Where(p => p.isAllUserBelongTo && p.Status == ProjectStatus.Active)
                    .Select(p => p.Id).ToList();
            foreach (var projectId in q)
            {
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    Type = ProjectUserType.Member
                };
                await WorkScope.InsertAsync<ProjectUser>(projectUser);
            }

            return user;
        }

        public async Task<UserDto> UpdateUserAsync(UserDto input, bool isUpdateUserWorkingTime)
        {
            var user = _userManager.GetUserByIdAsync(input.Id).Result;

            if (!isUpdateUserWorkingTime)
            {
                //Check permission update working time 
                input.MorningStartAt = user.MorningStartAt;
                input.MorningEndAt = user.MorningEndAt;
                input.MorningWorking = user.MorningWorking.Value;
                input.AfternoonStartAt = user.AfternoonStartAt;
                input.AfternoonEndAt = user.AfternoonEndAt;
                input.AfternoonWorking = user.MorningWorking.Value;
            }
            if (string.IsNullOrEmpty(input.AvatarPath))
            {
                input.AvatarPath = user.AvatarPath;
            }


            ObjectMapper.Map(input, user);

            user.UserName = input.UserName?.Replace("@gmail.com", "").Replace("@ncc.asia", "");
            user.isWorkingTimeDefault = input.isWorkingTimeDefault.HasValue ? input.isWorkingTimeDefault.Value : user.isWorkingTimeDefault;
            user.EndDateAt = !input.IsActive ? input.EndDateAt.HasValue ? input.EndDateAt : DateTimeUtils.GetNow() : input.EndDateAt;

            if (user.isWorkingTimeDefault.Value)
            {

                user.MorningStartAt = input.MorningStartAt;
                user.MorningEndAt = input.MorningEndAt;
                user.MorningWorking = input.MorningWorking;
                user.AfternoonStartAt = input.AfternoonStartAt;
                user.AfternoonEndAt = input.AfternoonEndAt;
                user.AfternoonWorking = input.AfternoonWorking;
            }
            await _userManager.UpdateAsync(user);

            return input;
        }

        public async Task<ulong?> UpdateKomuUserId(long userId)
        {
            var user = await WorkScope.GetAsync<User>(userId);
            if (user == null)
                return null;

            if (user.KomuUserId.HasValue)
                return user.KomuUserId;

            var komuUsername = UserHelper.GetKomuUserName(user.EmailAddress);
            user.KomuUserId = await _komuService.GetKomuUserId(komuUsername ?? user.UserName);
            if (user.KomuUserId.HasValue)
            {
                await WorkScope.UpdateAsync(user);
                return user.KomuUserId;
            }

            return null;
        }

        public async Task<long?> GetUserIdByEmail(string email)
        {
            return await WorkScope.GetAll<User>()
                .Where(s => s.EmailAddress.ToLower().Trim() == email.ToLower().Trim())
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<NotifyUserInfoDto> getKomuUserInfo(long userId)
        {
            return await WorkScope.GetAll<User>()
                .Where(s => s.Id == userId)
                .Select(s => new NotifyUserInfoDto
                {
                    FullName = s.FullName,
                    UserId = s.Id,
                    KomuUserId = s.KomuUserId,
                    EmailAddress = s.EmailAddress,
                    Branch = s.BranchOld,
                    Type = s.Type
                }).FirstOrDefaultAsync();

        }

        public async Task<List<GetUserDto>> GetListUserByRoleId(long roleId)
        {

            var query = from u in WorkScope.GetAll<User>()
                        join ur in WorkScope.GetAll<UserRole>().Where(s => s.RoleId == roleId)
                        on u.Id equals ur.UserId

                        select new GetUserDto
                        {
                            Id = u.Id,
                            Name = u.FullName,
                            IsActive = u.IsActive,
                            Type = u.Type,
                            JobTitle = u.JobTitle,
                            Level = u.Level,
                            UserCode = u.UserCode,
                            Branch = u.BranchOld,
                            BranchColor = u.Branch.Color,
                            BranchDisplayName = u.Branch.DisplayName,
                            AvatarPath = u.AvatarPath != null ? u.AvatarPath : ""
                        };
            return await query.ToListAsync();
        }
        private long GetPositionIdByCode(string code)
        {
            return WorkScope.GetAll<Position>()
                .Where(x => x.Code.ToLower().Trim() == code.ToLower().Trim())
                .Select(s => s.Id)
                .FirstOrDefault();

        }
        public async Task<User> CreateUserFromHrmv2Async(CreateUpdateByHRMV2Dto input)
        {
            var hadUser = WorkScope.GetAll<User>()
                .Select(x => x.EmailAddress.ToLower().Trim())
                .Any(x => x == input.EmailAddress.ToLower().Trim());
            if (hadUser)
            {
                throw new UserFriendlyException($"failed to create user from HRM, user with email {input.EmailAddress} is already exist");
            }            

            var user = new User()
            {
                TenantId = _abpSession.TenantId,
                UserName = input.EmailAddress?.Replace("@ncc.asia", ""),
                IsEmailConfirmed = true,
                isWorkingTimeDefault = true,
                NormalizedUserName = input.EmailAddress?.Replace("@ncc.asia", "").ToLower(),
                NormalizedEmailAddress = input.EmailAddress.ToLower(),
                Sex = input.GetSex,
                Type = input.UserType,
                EmailAddress = input.EmailAddress,
                Surname = input.Surname,
                Name = input.Name,
                Level = input.Level,
                StartDateAt = input.WorkingStartDate,
                BeginLevel = input.Level,
            };

            var positionId = GetPositionIdByCode(input.PositionCode);
            if (positionId != default)
            {
                user.PositionId = positionId;
            }

            user.Branch = await GetBranchByCode(input.BranchCode);
            user.Password = CommonUtils.GenerateRandomPassword(12);

            if (user.Branch != null)
            {
                user.MorningStartAt = user.Branch.MorningStartAt;
                user.MorningEndAt = user.Branch.MorningEndAt;
                user.MorningWorking = user.Branch.MorningWorking;
                user.AfternoonStartAt = user.Branch.AfternoonStartAt;
                user.AfternoonEndAt = user.Branch.AfternoonEndAt;
                user.AfternoonWorking = user.Branch.AfternoonWorking;
            }
            await _userManager.InitializeOptionsAsync(_abpSession.TenantId);
            await _userManager.CreateAsync(user, user.Password);

            //Default role BasicUser
            var roleNames = new string[] { StaticRoleNames.Host.BasicUser };
            await _userManager.SetRoles(user, roleNames);

            CurrentUnitOfWork.SaveChanges();

            var q = WorkScope.GetAll<Project>()
                    .Where(p => p.isAllUserBelongTo && p.Status == ProjectStatus.Active)
                    .Select(p => p.Id).ToList();

            foreach (var projectId in q)
            {
                var projectUser = new ProjectUser
                {
                    ProjectId = projectId,
                    UserId = user.Id,
                    Type = ProjectUserType.Member
                };
                WorkScope.Insert<ProjectUser>(projectUser);
            }

            return user;
        }
        public async Task<User> UpdateUserFromHrmV2Async(CreateUpdateByHRMV2Dto input)
        {
            var user = await WorkScope.GetAll<User>()
                .Where(x => x.EmailAddress.ToLower().Trim() == input.EmailAddress.ToLower().Trim()).FirstOrDefaultAsync();

            user.Type = input.UserType;
            user.Sex = input.GetSex;
            user.Name = input.Name;
            user.Surname = input.Surname;
            user.Level = input.Level;
            user.Branch = await GetBranchByCode(input.BranchCode);
            user.UserName = input.EmailAddress?.Replace("@ncc.asia", "").Replace("@gmail.com", "");
            user.NormalizedUserName = input.EmailAddress?.Replace("@ncc.asia", "").Replace("@gmail.com", "").ToLower();

            var positionId = GetPositionIdByCode(input.PositionCode);
            if ( positionId != default)
            {
                user.PositionId = positionId;
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }
    }

}
