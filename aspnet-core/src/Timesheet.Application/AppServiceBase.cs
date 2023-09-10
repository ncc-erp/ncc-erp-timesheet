using Abp.Application.Services;
using Abp.Configuration;
using Abp.Dependency;
using Abp.IdentityFramework;
using Abp.ObjectMapping;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using Ncc.MultiTenancy;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.HRM;
using Timesheet.Services.Komu;
using Timesheet.Services.Project;
using Timesheet.Services.Tracker;

namespace Ncc
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class AppServiceBase : ApplicationService
    {
        protected TenantManager TenantManager { get; set; }
        protected UserManager UserManager { get; set; }


        protected IWorkScope WorkScope { get; set; }

        protected AppServiceBase(IWorkScope workScope)
        {
            LocalizationSourceName = TimesheetConsts.LocalizationSourceName;
            WorkScope = workScope;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = WorkScope.GetAll<User>()
                .Where(x => x.Id == AbpSession.UserId)
                .FirstOrDefault();

            if (user == null)
            {
                throw new Exception("There is no current user!");
            }
            if (!user.BranchId.HasValue)
            {
                user.BranchId = await WorkScope.GetAll<Timesheet.Entities.Branch>().Select(s => s.Id).FirstOrDefaultAsync();
            }
            return user;
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        protected NotifyUserInfoDto GetSessionUserInfoDto()
        {
            var userId = AbpSession.UserId.Value;

            return GetSessionUserInfoDtoByUserId(userId);
        }

        protected NotifyUserInfoDto GetSessionUserInfoDtoByUserId(long userId)
        {
            var user = WorkScope.GetAll<User>()
               .Where(s => s.Id == userId)
               .Select(s => new NotifyUserInfoDto
               {
                   Branch = s.BranchOld,
                   EmailAddress = s.EmailAddress,
                   FullName = s.FullName,
                   Type = s.Type,
                   KomuUserId = s.KomuUserId,
                   UserId = s.Id,
                   BranchDisplayName = s.Branch.DisplayName,
                   MorningWorking = s.MorningWorking.HasValue ? s.MorningWorking.Value : 3.5,
                   AfternoonWorking = s.AfternoonWorking.HasValue ? s.AfternoonWorking.Value : 4.5
               }).FirstOrDefault();

            return user;
        }

        public async Task<long> GetBranchIdByCode(string code)
        {
            var branchId = await WorkScope.GetAll<Branch>().Where(s => s.Code == code).Select(s => s.Id).FirstOrDefaultAsync();
            if (branchId == default)
            {
                branchId = await WorkScope.GetAll<Branch>().Select(s => s.Id).FirstOrDefaultAsync();
            }
            return branchId;
        }

        public long GetUserIdByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return default;
            }

            return WorkScope.GetAll<User>()
                .Where(s => s.EmailAddress.ToLower().Trim() == email.ToLower().Trim())
                .Select(s => s.Id)
                .FirstOrDefault();
        }

    }
}