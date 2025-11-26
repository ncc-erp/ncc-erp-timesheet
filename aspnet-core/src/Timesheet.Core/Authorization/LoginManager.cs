using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.UI;
using Abp.Zero.Configuration;
using Castle.Core.Logging;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Ncc.Authorization.Roles;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using Ncc.MultiTenancy;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Timesheet.Authorization.Users;
using Timesheet.Helper;
using Timesheet.Services.Mezon;
using Timesheet.Services.Mezon.Dto;
using Ncc.Authorization.Dto;

namespace Ncc.Authorization
{
    public class LogInManager : AbpLogInManager<Tenant, Role, User>
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private ILogger Logger { get; set; }
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
            IBackgroundJobManager backgroundJobManager,
            IWorkScope workScope,
            MezonService mezonService,
            IConfiguration configuration
           )
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
            _workScope = workScope;
            _mezonService = mezonService;
            Logger = NullLogger.Instance;
            Configuration = configuration;

        }
        private IConfiguration Configuration { get; }


        [UnitOfWork]
        public async Task<AbpLoginResult<Tenant, User>> LoginHashMezonAsnyc(MezonHashAuthDto hashAuthDto)
        {
            var result = await AuthMezonHashAsync(hashAuthDto);
            var user = result.User;
            await SaveLoginAttempt(result, hashAuthDto.TenancyName, user == null ? null : user.EmailAddress);
            return result;
        }

        private async Task<AbpLoginResult<Tenant, User>> AuthMezonHashAsync(MezonHashAuthDto hashAuthDto)
        {
            if (hashAuthDto.HashData.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(hashAuthDto.HashData));
            }
            try
            {
                var appToken = Configuration["MezonService:AppToken"] ?? throw new ArgumentNullException("Invalid AppToken");

                //separate payload to 2 part, userinfo and mezonHash
                var rawHashData = hashAuthDto.HashData.DecodeBase64();
                var delimiter = "&hash=";
                int index = rawHashData.IndexOf(delimiter);
                string queryId = rawHashData.Substring(0, index);
                string mezonHash = rawHashData.Substring(index + delimiter.Length);
                var hashData = HashParamsParser(queryId);

                //check user exist by email from query_id
                var mezonUser = JsonConvert.DeserializeObject<MezonUser>(hashData.user);
                var user = UserManager.Users.FirstOrDefault(x => !string.IsNullOrEmpty(x.MezonUserId) && x.MezonUserId == mezonUser.mezon_user_id) ??
                    UserManager.Users.FirstOrDefault(x => x.EmailAddress == mezonUser.mezon_id);
                if (user == null)
                {
                   Logger.Info($"Login fail with email: {mezonUser.mezon_id}");
                    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, null);
                }
                // appToken ==> MD5
                var appTokenMD5 = Hasher.MD5Hash(appToken);
                byte[] secretKey = Hasher.HMAC_SHA256(Encoding.UTF8.GetBytes(appTokenMD5), Encoding.UTF8.GetBytes("WebAppData"));
                var hashedData = Hasher.HEX(Hasher.HMAC_SHA256(secretKey, Encoding.UTF8.GetBytes(queryId)));

                if (mezonHash.Equals(hashedData) == false)
                {
                    Logger.Info("Authenticattion failed - Invalid hash key");
                    throw new UserFriendlyException("Authenticattion failed - Invalid hash key");
                }

                var loginResult = await HandleAuthWithEmail(
                    emailAddress: mezonUser.mezon_id,
                    mezonUserId: mezonUser.mezon_user_id,
                    tenancyName: hashAuthDto.TenancyName
                    );

                return loginResult;
            }
            catch (Exception e)
            {
                Logger.Info("Authenticattion failed - Can't authenticate with Mezon server");
                return new AbpLoginResult<Tenant, User>(AbpLoginResultType.UnknownExternalLogin, null);
            }
        }

        private async Task<AbpLoginResult<Tenant, User>> HandleAuthWithEmail(string emailAddress, string mezonUserId, string tenancyName, bool shouldLockout = true)
        {

            Tenant tenant = null;

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
                var tenantId = tenant?.Id;
                using (UnitOfWorkManager.Current.SetTenantId(tenantId))
                {
                    await UserManager.InitializeOptionsAsync(tenantId);
                    //var user = await UserManager.FindByNameOrEmailAsync(tenantId, emailAddress);
                    var user = UserManager.Users.FirstOrDefault(x => !string.IsNullOrEmpty(x.MezonUserId) && x.MezonUserId == mezonUserId) ??
                    UserManager.Users.FirstOrDefault(x => x.EmailAddress == emailAddress);
                    if (user == null)
                    {
                        return new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, tenant);
                    }
                    //if (await UserManager.IsLockedOutAsync(user))
                    //{
                    //    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                    //}
                    //if (shouldLockout && await TryLockOutAsync(tenantId, user.Id))
                    //{
                    //    return new AbpLoginResult<Tenant, User>(AbpLoginResultType.LockedOut, tenant, user);
                    //}
                    await UserManager.ResetAccessFailedCountAsync(user);
                    var loginResult = await CreateLoginResultAsync(user, tenant);
                    return loginResult;
                }
            }
        }


        private HashData HashParamsParser(string queryString)
        {
            var queryParams = HttpUtility.ParseQueryString(queryString);
            var hashData = new HashData
            {
                query_id = queryParams["query_id"],
                user = queryParams["user"],
                auth_date = long.Parse(queryParams["auth_date"]),
                signature = queryParams["signature"],
                hash = queryParams["hash"]
            };
            return hashData;
        }

        private string HashParamsStringify(object hashData)
        {
            var queryString = new StringBuilder();

            var properties = hashData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var value = property.GetValue(hashData);
                if (value != null)
                {
                    if (queryString.Length > 0)
                        queryString.Append("&");
                    queryString.AppendFormat($"{Uri.EscapeDataString(property.Name)}={Uri.EscapeDataString(value.ToString())}");
                }
            }

            return queryString.ToString();
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

                        if (user.GoogleId == null)
                        {
                            user.GoogleId = payload.Subject;
                            await _workScope.UpdateAsync(user);
                            //await UserManager.UpdateAsync(user);
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

        [UnitOfWork]
        public async Task<MezonLoginResult> LoginOAuth2Async(string token, string tenancyName = null, bool shouldLockout = true)
        {
            Logger.Info("LoginOAuth2");
            var mezonResult = await LoginInternalOAuth2Async(token, tenancyName, shouldLockout);
            var result = mezonResult.LoginResult;
            var user = result.User;
            SaveLoginAttempt(result, tenancyName, user?.EmailAddress);
            return mezonResult;
        }

        public async Task<MezonLoginResult> LoginInternalOAuth2Async(string token, string tenancyName, bool shouldLockout)
        {
            if (token.IsNullOrEmpty())
            {
                return new MezonLoginResult
                {
                    LoginResult = new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, null),
                    IdToken = null
                };
            }

            try
            {
                var mezonConfig = _mezonService.GetConfig();
                var tokenResponse = await _mezonService.GetTokenAsync(new OAuth2Request
                {
                    Code = token,
                    Scope = "openid offline"
                });

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    return new MezonLoginResult
                    {
                        LoginResult = new AbpLoginResult<Tenant, User>(AbpLoginResultType.UnknownExternalLogin, null),
                        IdToken = null
                    };
                }

                var userInfo = await _mezonService.GetUserInfoAsync(tokenResponse.AccessToken);
                if (userInfo == null)
                {
                    return new MezonLoginResult
                    {
                        LoginResult = new AbpLoginResult<Tenant, User>(AbpLoginResultType.UnknownExternalLogin, null),
                        IdToken = null
                    };
                }

                var correctAudience = userInfo.Audience.Any(s => s == mezonConfig.ClientId);
                var correctIssuer = userInfo.Issuer == "oauth2.mezon.ai" || userInfo.Issuer == "https://oauth2.mezon.ai";
                var correctSub = !string.IsNullOrEmpty(userInfo.Subject);
                var correctExpiryTime = tokenResponse.ExpiresIn != null && tokenResponse.ExpiresIn > 0;

                if (!correctAudience || !correctIssuer || !correctSub || !correctExpiryTime)
                {
                    return new MezonLoginResult
                    {
                        LoginResult = new AbpLoginResult<Tenant, User>(AbpLoginResultType.InvalidUserNameOrEmailAddress, null),
                        IdToken = null
                    };
                }

                var loginResult = await HandleAuthWithEmail(
                    emailAddress: userInfo.Subject,
                    mezonUserId: userInfo.MezonUserId,
                    tenancyName: tenancyName,
                    shouldLockout: shouldLockout
                    );

                return new MezonLoginResult
                {
                    LoginResult = loginResult,
                    IdToken = tokenResponse.IdToken
                };
            }
            catch (Exception)
            {
                return new MezonLoginResult
                {
                    LoginResult = new AbpLoginResult<Tenant, User>(AbpLoginResultType.UnknownExternalLogin, null),
                    IdToken = null
                };
            }
        }
    }
}