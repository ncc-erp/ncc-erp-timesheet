using Microsoft.AspNetCore.Identity;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Zero.Configuration;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.MultiTenancy;
using System.Threading.Tasks;
using System;
using Abp.Extensions;
using Google.Apis;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Ncc.Configuration;
using Timesheet.Helper;
using Abp.BackgroundJobs;
using Timesheet.BackgroundJob;
using Abp.UI;

namespace Ncc.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        public LogInManager(
            UserManager userManager,
            IMultiTenancyConfig multiTenancyConfig,
            IRepository<Tenant> tenantRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ISettingManager settingManager,
            IRepository<UserLoginAttempt, long> userLoginAttemptRepository,
            IUserManagementConfig userManagementConfig,
            IIocResolver iocResolver,
            IPasswordHasher<User> passwordHasher,
            RoleManager roleManager,
            UserClaimsPrincipalFactory claimsPrincipalFactory,
            IBackgroundJobManager backgroundJobManager)
            : base(
                  userManager,
                  multiTenancyConfig,
                  tenantRepository,
                  unitOfWorkManager,
                  settingManager,
                  userLoginAttemptRepository,
                  userManagementConfig,
                  iocResolver,
                  passwordHasher,
                  roleManager,
                  claimsPrincipalFactory)
        {
            _backgroundJobManager = backgroundJobManager;
        }
        [UnitOfWork]
        public async Task<AbpLoginResult<Tenant, User>> LoginAsyncNoPass(string token, string secretCode = "", string tenancyName = null, bool shouldLockout = true)
        {
            var result = await LoginAsyncInternalNoPass(token, secretCode, tenancyName, shouldLockout);
            var user = result.User;
            await SaveLoginAttempt(result, tenancyName, user == null ? null : user.EmailAddress);
            return result;
        }

        public async Task<AbpLoginResult<Tenant, User>> LoginAsyncInternalNoPass(string token, string secretCode, string tenancyName, bool shouldLockout)
        {

            if (token.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(token));
            }
            try
            {
                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(token);
                var emailAddress = payload.Email;

                // checking
                var clientAppId = await SettingManager.GetSettingValueAsync(AppSettingNames.ClientAppId);//get clientAppId from setting
                var correctAudience = payload.AudienceAsList.Any(s => s == clientAppId);
                var correctIssuer = payload.Issuer == "accounts.google.com" || payload.Issuer == "https://accounts.google.com";
                var correctExpriryTime = payload.ExpirationTimeSeconds != null || payload.ExpirationTimeSeconds > 0;

                Tenant tenant = null;
                if (correctAudience && correctIssuer && correctExpriryTime)
                {
                    //Get and check tenant
                    using (UnitOfWorkManager.Current.SetTenantId(null))
                    {
                        if (!MultiTenancyConfig.IsEnabled)
                        {
                            tenant = await GetDefaultTenantAsync();
                        }
                        else if (!string.IsNullOrWhiteSpace(tenancyName))
                        {
                            tenant = await TenantRepository.FirstOrDefaultAsync(t => t.TenancyName == tenancyName);
                            if (tenant == null)
                            {
                                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidTenancyName);
                            }

                            if (!tenant.IsActive)
                            {
                                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.TenantIsNotActive, tenant);
                            }
                        }
                    }
                    var tenantId = tenant == null ? (int?)null : tenant.Id;
                    using (UnitOfWorkManager.Current.SetTenantId(tenantId))
                    {
                        await UserManager.InitializeOptionsAsync(tenantId);

                        var user = await UserManager.FindByNameOrEmailAsync(tenantId, emailAddress);
                        if (user == null)
                        {
                            //var userCreate = new User
                            //{
                            //    UserName = emailAddress,
                            //    EmailAddress = emailAddress,
                            //    Name = payload.GivenName,
                            //    Surname = payload.FamilyName,
                            //    IsActive = false
                            //};

                            //userCreate.TenantId = tenantId;
                            //userCreate.IsEmailConfirmed = true;

                            ////get secret code from setting

                            //var appSecretCode = await SettingManager.GetSettingValueAsync(AppSettingNames.SecretRegisterCode);

                            ////if correct secret code or email has domain of ncc.asia, set user active is true
                            //if (secretCode == appSecretCode || Regex.IsMatch(emailAddress, "^[a - z][a - z0 - 9_\\.]{ 5,32}@ncc.asia$"))
                            //{
                            //    userCreate.IsActive = true;
                            //}

                            //// create random password
                            //var randomPassword = RandomPasswordHelper.CreateRandomPassword(8);
                            //await UserManager.CreateAsync(userCreate, randomPassword);

                            //await UserManager.SetRoles(userCreate, new string[] { StaticRoleNames.Host.BasicUser });

                            //await UnitOfWorkManager.Current.SaveChangesAsync();

                            //// send newly created userpassword to email 

                            //await _backgroundJobManager.EnqueueAsync<EmailBackgroundJob, EmailBackgroundJobArgs>(new EmailBackgroundJobArgs
                            //{
                            //    TargetEmails = new List<string>() { userCreate.EmailAddress },
                            //    Body = $@"<div>Username: <b>{userCreate.UserName}</b></div>
                            //    <div>Password: <b>{randomPassword}</b></div>",
                            //    Subject = "Your account was created"
                            //}, BackgroundJobPriority.High, new TimeSpan(TimeSpan.TicksPerMinute)
                            //);

                            //await UserManager.ResetAccessFailedCountAsync(userCreate);
                            //return await CreateLoginResultAsync(userCreate, tenant);
                            throw new UserFriendlyException(string.Format("Login Fail - Account does not exist"));
                        }

                        if (await UserManager.IsLockedOutAsync(user))
                        {
                            return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                        }
                        if (shouldLockout)
                        {
                            if (await TryLockOutAsync(tenantId, user.Id))
                            {
                                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                            }
                        }

                        await UserManager.ResetAccessFailedCountAsync(user);
                        return await CreateLoginResultAsync(user, tenant);
                    }
                }
                else
                {
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, null);
                }
            }
            catch (InvalidJwtException e)
            {
                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, null);
            }
        }
    }
}
