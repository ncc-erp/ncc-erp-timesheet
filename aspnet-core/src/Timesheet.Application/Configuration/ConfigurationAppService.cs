using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ncc.Authorization.Users;
using Ncc.Configuration.Dto;
using Ncc.IoC;
using Timesheet.Configuration.Dto;
using Timesheet.DomainServices.Dto;
using Timesheet.Services.HRMv2;
using Timesheet.Services.Project;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Configuration
{

    public class ConfigurationAppService : AppServiceBase, IConfigurationAppService
    {
        private readonly ProjectService _projectService;
        private readonly HRMv2Service _hRMv2Service;
        private readonly IConfiguration _configuration;

        public ConfigurationAppService(ProjectService projectService, IConfiguration configuration,  HRMv2Service hRMv2Service, IWorkScope workScope) : base(workScope)
        {
            _projectService = projectService;
            _hRMv2Service = hRMv2Service;
            _configuration= configuration;
        }

        [AbpAuthorize]
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }

        public async Task<string> GetGoogleClientAppId()
        {
            return await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ClientAppId);
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_WorkingDay_View)]
        public async Task<NormalWorkingDto> Get()
        {
            return new NormalWorkingDto
            {
                MorningHNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNStartAt),
                MorningHNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNEndAt),
                AfternoonHNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNStartAt),
                AfternoonHNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNEndAt),
                MorningDNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNStartAt),
                MorningDNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNEndAt),
                AfternoonDNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNStartAt),
                AfternoonDNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNEndAt),
                MorningHCMStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMStartAt),
                MorningHCMEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMEndAt),
                AfternoonHCMStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMStartAt),
                AfternoonHCMEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMEndAt),
                MorningVinhStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhStartAt),
                MorningVinhEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhEndAt),
                AfternoonVinhStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhStartAt),
                AfternoonVinhEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhEndAt),

                MorningDNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNWorking),
                MorningHNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNWorking),
                MorningHCMWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMWorking),
                MorningVinhWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhWorking),
                AfternoonDNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNWorking),
                AfternoonHNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNWorking),
                AfternoonHCMWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMWorking),
                AfternoonVinhWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhWorking),
                EmailHR = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHR),
                EmailHRDN = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRDN),
                EmailHRHCM = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRHCM),
                EmailHRVinh = await SettingManager.GetSettingValueAsync(AppSettingNames.EmailHRVinh),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_WorkingDay_View)]
        public async Task<Dictionary<long, BranchWorkingTimeDto>> GetWorkingTimeConfigAllBranch()
        {
            Dictionary<long, BranchWorkingTimeDto> rs = new Dictionary<long, BranchWorkingTimeDto>();
            rs.Add((long)Branch.HaNoi, new BranchWorkingTimeDto
            {
                MorningStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNStartAt),
                MorningEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNEndAt),
                MorningWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNWorking),
                AfternoonStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNStartAt),
                AfternoonEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNEndAt),
                AfternoonWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNWorking),
            });
            rs.Add((long)Branch.DaNang, new BranchWorkingTimeDto
            {
                MorningStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNStartAt),
                MorningEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNEndAt),
                MorningWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNWorking),
                AfternoonStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNStartAt),
                AfternoonEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNEndAt),
                AfternoonWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNWorking),
            });
            rs.Add((long)Branch.HoChiMinh, new BranchWorkingTimeDto
            {
                MorningStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMStartAt),
                MorningEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMEndAt),
                MorningWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMWorking),
                AfternoonStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMStartAt),
                AfternoonEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMEndAt),
                AfternoonWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMWorking),
            });
            rs.Add((long)Branch.Vinh, new BranchWorkingTimeDto
            {
                MorningStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhStartAt),
                MorningEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhEndAt),
                MorningWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhWorking),
                AfternoonStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhStartAt),
                AfternoonEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhEndAt),
                AfternoonWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhWorking),
            });
            return rs;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CheckInSetting_View)]
        public async Task<ConfigCheckinDto> GetCheckInSetting()
        {
            return new ConfigCheckinDto
            {
                CheckInInternalUrl = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.CheckInInternalUrl),
                CheckInInternalAccount = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.CheckInInternalAccount),
                CheckInInternalXSecretKey = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.CheckInInternalXSecretKey)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CheckInSetting_Update)]
        public async Task<ConfigCheckinDto> UpdateCheckInSetting(ConfigCheckinDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInInternalUrl, input.CheckInInternalUrl);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInInternalAccount, input.CheckInInternalAccount);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInInternalXSecretKey, input.CheckInInternalXSecretKey);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_WorkingDay_Edit)]
        public async Task<NormalWorkingDto> Change(NormalWorkingDto input)
        {
            if (string.IsNullOrEmpty(input.MorningHNStartAt) ||
                string.IsNullOrEmpty(input.MorningHNEndAt) ||
                string.IsNullOrEmpty(input.AfternoonHNStartAt) ||
                string.IsNullOrEmpty(input.AfternoonHNEndAt) ||
                string.IsNullOrEmpty(input.MorningDNStartAt) ||
                string.IsNullOrEmpty(input.MorningDNEndAt) ||
                string.IsNullOrEmpty(input.AfternoonDNStartAt) ||
                string.IsNullOrEmpty(input.AfternoonDNEndAt) ||
                string.IsNullOrEmpty(input.MorningHCMStartAt) ||
                string.IsNullOrEmpty(input.MorningHCMEndAt) ||
                string.IsNullOrEmpty(input.AfternoonHCMStartAt) ||
                string.IsNullOrEmpty(input.AfternoonHCMEndAt) ||
                string.IsNullOrEmpty(input.MorningVinhStartAt) ||
                string.IsNullOrEmpty(input.MorningVinhEndAt) ||
                string.IsNullOrEmpty(input.AfternoonVinhStartAt) ||
                string.IsNullOrEmpty(input.AfternoonVinhEndAt) ||

                string.IsNullOrEmpty(input.MorningHNWorking) ||
                string.IsNullOrEmpty(input.MorningDNWorking) ||
                string.IsNullOrEmpty(input.MorningHCMWorking) ||
                string.IsNullOrEmpty(input.MorningVinhWorking) ||
                string.IsNullOrEmpty(input.AfternoonDNWorking) ||
                string.IsNullOrEmpty(input.AfternoonHNWorking) ||
                string.IsNullOrEmpty(input.AfternoonHNWorking) ||
                string.IsNullOrEmpty(input.AfternoonVinhWorking))
            {
                throw new UserFriendlyException("Working time need to be completed");
            }

            var obj = new NormalWorkingDto
            {
                MorningHNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNStartAt),
                MorningHNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNEndAt),
                AfternoonHNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNStartAt),
                AfternoonHNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNEndAt),
                MorningDNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNStartAt),
                MorningDNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNEndAt),
                AfternoonDNStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNStartAt),
                AfternoonDNEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNEndAt),
                MorningHCMStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMStartAt),
                MorningHCMEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMEndAt),
                AfternoonHCMStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMStartAt),
                AfternoonHCMEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMEndAt),
                MorningVinhStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhStartAt),
                MorningVinhEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhEndAt),
                AfternoonVinhStartAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhStartAt),
                AfternoonVinhEndAt = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhEndAt),

                MorningHNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHNWorking),
                MorningDNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningDNWorking),
                MorningHCMWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningHCMWorking),
                MorningVinhWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MorningVinhWorking),
                AfternoonDNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonDNWorking),
                AfternoonHNWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHNWorking),
                AfternoonHCMWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonHCMWorking),
                AfternoonVinhWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AfternoonVinhWorking),
            };

            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningHNStartAt, input.MorningHNStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningHNEndAt, input.MorningHNEndAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonHNStartAt, input.AfternoonHNStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonHNEndAt, input.AfternoonHNEndAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningDNStartAt, input.MorningDNStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningDNEndAt, input.MorningDNEndAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonDNStartAt, input.AfternoonDNStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonDNEndAt, input.AfternoonDNEndAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningHCMStartAt, input.MorningHCMStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningHCMEndAt, input.MorningHCMEndAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonHCMStartAt, input.AfternoonHCMStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonHCMEndAt, input.AfternoonHCMEndAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningVinhStartAt, input.MorningVinhStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningVinhEndAt, input.MorningVinhEndAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonVinhStartAt, input.AfternoonVinhStartAt);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonVinhEndAt, input.AfternoonVinhEndAt);

            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningHNWorking, input.MorningHNWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningDNWorking, input.MorningDNWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningHCMWorking, input.MorningHCMWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MorningVinhWorking, input.MorningVinhWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonDNWorking, input.AfternoonDNWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonHNWorking, input.AfternoonHNWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonHCMWorking, input.AfternoonHCMWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AfternoonVinhWorking, input.AfternoonVinhWorking);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.EmailHR, input.EmailHR);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.EmailHRDN, input.EmailHRDN);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.EmailHRHCM, input.EmailHRHCM);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.EmailHRVinh, input.EmailHRVinh);

            var isHNChange = obj.MorningHNStartAt != input.MorningHNStartAt
                || obj.MorningHNEndAt != input.MorningHNEndAt
                || obj.AfternoonHNStartAt != input.AfternoonHNStartAt
                || obj.AfternoonHNEndAt != input.AfternoonHNEndAt;

            var isDNChange = obj.MorningDNStartAt != input.MorningDNStartAt
                || obj.MorningDNEndAt != input.MorningDNEndAt
                || obj.AfternoonDNStartAt != input.AfternoonDNStartAt
                || obj.AfternoonDNEndAt != input.AfternoonDNEndAt;

            var isHCMChange = obj.MorningHCMStartAt != input.MorningHCMStartAt
               || obj.MorningHCMEndAt != input.MorningHCMEndAt
               || obj.AfternoonHCMStartAt != input.AfternoonHCMStartAt
               || obj.AfternoonHCMEndAt != input.AfternoonHCMEndAt;

            var isVinhChange = obj.MorningVinhStartAt != input.MorningVinhStartAt
              || obj.MorningVinhEndAt != input.MorningVinhEndAt
              || obj.AfternoonVinhStartAt != input.AfternoonVinhStartAt
              || obj.AfternoonVinhEndAt != input.AfternoonVinhEndAt;

            if (isHNChange)
            {
                var users = await WorkScope.GetAll<User>()
                .Where(s => s.BranchOld == Branch.HaNoi)
                .ToListAsync();

                foreach (var user in users)
                {
                    if (user.isWorkingTimeDefault.HasValue && !user.isWorkingTimeDefault.Value)
                    {
                        continue;
                    }
                    user.isWorkingTimeDefault = true;

                    user.MorningStartAt = input.MorningHNStartAt;
                    user.MorningEndAt = input.MorningHNEndAt;
                    user.MorningWorking = Double.Parse(input.MorningHNWorking);

                    user.AfternoonStartAt = input.AfternoonHNStartAt;
                    user.AfternoonEndAt = input.AfternoonHNEndAt;
                    user.AfternoonWorking = Double.Parse(input.AfternoonHNWorking);
                    await WorkScope.UpdateAsync(user);
                }
            }

            if (isDNChange)
            {
                var users = await WorkScope.GetAll<User>()
                .Where(s => s.BranchOld == Branch.DaNang)
                .ToListAsync();

                foreach (var user in users)
                {
                    if (user.isWorkingTimeDefault.HasValue && !user.isWorkingTimeDefault.Value)
                    {
                        continue;
                    }
                    user.isWorkingTimeDefault = true;

                    user.MorningStartAt = input.MorningDNStartAt;
                    user.MorningEndAt = input.MorningDNEndAt;
                    user.MorningWorking = Double.Parse(input.MorningDNWorking);

                    user.AfternoonStartAt = input.AfternoonDNStartAt;
                    user.AfternoonEndAt = input.AfternoonDNEndAt;
                    user.AfternoonWorking = Double.Parse(input.AfternoonDNWorking);
                    await WorkScope.UpdateAsync(user);
                }
            }

            if (isHCMChange)
            {
                var users = await WorkScope.GetAll<User>()
                .Where(s => s.BranchOld == Branch.HoChiMinh)
                .ToListAsync();

                foreach (var user in users)
                {
                    if (user.isWorkingTimeDefault.HasValue && !user.isWorkingTimeDefault.Value)
                    {
                        continue;
                    }
                    user.isWorkingTimeDefault = true;

                    user.MorningStartAt = input.MorningHCMStartAt;
                    user.MorningEndAt = input.MorningHCMEndAt;
                    user.MorningWorking = Double.Parse(input.MorningHCMWorking);

                    user.AfternoonStartAt = input.AfternoonHCMStartAt;
                    user.AfternoonEndAt = input.AfternoonHCMEndAt;
                    user.AfternoonWorking = Double.Parse(input.AfternoonHCMWorking);
                    await WorkScope.UpdateAsync(user);
                }
            }
            if (isVinhChange)
            {
                var users = await WorkScope.GetAll<User>()
                .Where(s => s.BranchOld == Branch.Vinh)
                .ToListAsync();

                foreach (var user in users)
                {
                    if (user.isWorkingTimeDefault.HasValue && !user.isWorkingTimeDefault.Value)
                    {
                        continue;
                    }
                    user.isWorkingTimeDefault = true;

                    user.MorningStartAt = input.MorningVinhStartAt;
                    user.MorningEndAt = input.MorningVinhEndAt;
                    user.MorningWorking = Double.Parse(input.MorningVinhWorking);

                    user.AfternoonStartAt = input.AfternoonVinhStartAt;
                    user.AfternoonEndAt = input.AfternoonVinhEndAt;
                    user.AfternoonWorking = Double.Parse(input.AfternoonVinhWorking);
                    await WorkScope.UpdateAsync(user);
                }
            }
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_HRMConfig_View)]
        public async Task<HRMConfigDto> GetHRMConfig()
        {
            //return new HRMConfigDto
            //{
            //    HRMUri = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.HRMUri),
            //    SecretCode = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.HRMSecretCode),
            //};
            return new HRMConfigDto
            {
                HRMUri = _configuration.GetValue<string>("HRMv2Service:BaseAddress"),
                SecretCode = _configuration.GetValue<string>("HRMv2Service:SecurityCode"),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_HRMConfig_Update)]
        public async Task<HRMConfigDto> SetHRMConfig(HRMConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.HRMUri, input.HRMUri);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.HRMSecretCode, input.SecretCode);
            return input;
        }

        [AbpAuthorize(Authorization.PermissionNames.Admin_Configuration_ProjectConfig_View)]
        public async Task<ProjectConfigDto> GetProjectConfig()
        {
            //return new ProjectConfigDto
            //{
            //    ProjectUri = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ProjectUri),
            //    SecretCode = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ProjectSecretCode),
            //};
            return new ProjectConfigDto
            {
                ProjectUri = _configuration.GetValue<string>("ProjectService:BaseAddress"),
                SecretCode = _configuration.GetValue<string>("ProjectService:SecurityCode")
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ProjectConfig_Update)]
        public async Task<ProjectConfigDto> SetProjectConfig(ProjectConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ProjectUri, input.ProjectUri);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ProjectSecretCode, input.SecretCode);
            return input;
        }

        [AbpAuthorize(Authorization.PermissionNames.Admin_Configuration_KomuConfig_View)]
        public async Task<KomuConfigDto> GetKomuConfig()
        {
            return new KomuConfigDto
            {
                KomuUri = _configuration.GetValue<string>("KomuService:BaseAddress"),
                KomuSecretCode = _configuration.GetValue<string>("KomuService:SecurityCode"),
                KomuChannelIdDevMode = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.KomuChannelIdDevMode),
                KomuUserNameDevMode = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.KomuUserNameDevMode),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_KomuConfig_Update)]
        public async Task<KomuConfigDto> SetKomuConfig(KomuConfigDto input)
        {
            //await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.KomuUri, input.KomuUri);
            //await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.KomuSecretCode, input.KomuSecretCode);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.KomuChannelIdDevMode, input.KomuChannelIdDevMode);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.KomuUserNameDevMode, input.KomuUserNameDevMode);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NotificationSetting_View)]
        public async Task<NotificationSettingDto> GetNotificationSetting()
        {
            return new NotificationSettingDto
            {
                SendEmailTimesheet = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailTimesheet),
                SendEmailRequest = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendEmailRequest),
                SendKomuSubmitTimesheet = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuSubmitTimesheet),
                SendKomuRequest = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendKomuRequest),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NotificationSetting_Edit)]
        public async Task<NotificationSettingDto> SetNotificationSetting (NotificationSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendEmailTimesheet, input.SendEmailTimesheet);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendEmailRequest, input.SendEmailRequest);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendKomuSubmitTimesheet, input.SendKomuSubmitTimesheet);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendKomuRequest, input.SendKomuRequest);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NRITConfig_View)]
        public async Task<NRITConfigDto> GetNRITConfig()
        {
            return new NRITConfigDto
            {
                NotifyEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NRITNotifyEnableWorker),
                NotifyAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NRITNotifyAtHour),
                NotifyReviewDeadline = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NRITNotifyReviewDeadline),
                NotifyOnDates = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NRITNotifyOnDates),
                NotifyToChannels = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NRITNotifyToChannels),
                NotifyPenaltyFee = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NRITNotifyPenaltyFee),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NRITConfig_Update)]
        public async Task<NRITConfigDto> SetNRITConfig(NRITConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyEnableWorker, input.NotifyEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyAtHour, input.NotifyAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyReviewDeadline, input.NotifyReviewDeadline);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyOnDates, input.NotifyOnDates);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyToChannels, input.NotifyToChannels);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyPenaltyFee, input.NotifyPenaltyFee);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_UnlockTimesheetSetting_View)]
        public UnlockTimesheetConfigDto GetUnlockTimesheetConfig()
        {
            return new UnlockTimesheetConfigDto
            {
                WeeksCanUnlockBefor = SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.WeeksCanUnlockBefor).Result
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_UnlockTimesheetSetting_Update)]
        public async Task<UnlockTimesheetConfigDto> SetUnlockTimesheetConfig(UnlockTimesheetConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.WeeksCanUnlockBefor, input.WeeksCanUnlockBefor);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_RetroNotifyConfig_View)]
        public async Task<RetroNotifyConfigDto> GetRetroNotifyConfig()
        {
            return new RetroNotifyConfigDto
            {
                RetroNotifyEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.RetroNotifyEnableWorker),
                RetroNotifyAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.RetroNotifyAtHour),
                RetroNotifyDeadline = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.RetroNotifyDeadline),
                RetroNotifyOnDates = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.RetroNotifyOnDates),
                RetroNotifyToChannels = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.RetroNotifyToChannels),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_RetroNotifyConfig_Update)]
        public async Task<RetroNotifyConfigDto> SetRetroNotifyConfig(RetroNotifyConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.RetroNotifyEnableWorker, input.RetroNotifyEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.RetroNotifyAtHour, input.RetroNotifyAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.RetroNotifyDeadline, input.RetroNotifyDeadline);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.RetroNotifyOnDates, input.RetroNotifyOnDates);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.RetroNotifyToChannels, input.RetroNotifyToChannels);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_TeamBuilding_View)]
        public async Task<TeamBuildingConfigDto> GetTeamBuildingConfig()
        {
            return new TeamBuildingConfigDto
            {
                GenerateDataOnDate = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.GenerateDataOnDate),
                TeamBuildingMoney = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.TeamBuildingMoney),
                BillPercentage = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BillPercentage),
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_TeamBuilding_Update)]
        public async Task<TeamBuildingConfigDto> SetTeamBuildingConfig(TeamBuildingConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.GenerateDataOnDate, input.GenerateDataOnDate);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TeamBuildingMoney, input.TeamBuildingMoney);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BillPercentage, input.BillPercentage);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_View)]
        public async Task<ApproveTimesheetNotifyConfigDto> GetApproveTimesheetNotifyConfig()
        {
            return new ApproveTimesheetNotifyConfigDto
            {
                ApproveTimesheetNotifyEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyEnableWorker),
                ApproveTimesheetNotifyAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyAtHour),
                ApproveTimesheetNotifyOnDates = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyOnDates),
                ApproveTimesheetNotifyToChannels = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyToChannels),
                ApproveTimesheetNotifyTimePeriodWithPendingRequest = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyTimePeriodWithPendingRequest),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_Update)]
        public async Task<ApproveTimesheetNotifyConfigDto> SetApproveTimesheetNotifyConfig(ApproveTimesheetNotifyConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyEnableWorker, input.ApproveTimesheetNotifyEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyAtHour, input.ApproveTimesheetNotifyAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyOnDates, input.ApproveTimesheetNotifyOnDates);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyToChannels, input.ApproveTimesheetNotifyToChannels);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveTimesheetNotifyTimePeriodWithPendingRequest, input.ApproveTimesheetNotifyTimePeriodWithPendingRequest);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_View)]
        public async Task<ApproveRequestOffConfigDto> GetApproveRequestOffNotifyConfig()
        {
            return new ApproveRequestOffConfigDto
            {
                ApproveRequestOffNotifyEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyEnableWorker),
                ApproveRequestOffNotifyAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyAtHour),
                ApproveRequestOffNotifyToChannels = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyToChannels),
                ApproveRequestOffNotifyTimePeriodWithPendingRequest = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyTimePeriodWithPendingRequest),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_Update)]
        public async Task<ApproveRequestOffConfigDto> SetApproveRequestOffNotifyConfig(ApproveRequestOffConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyEnableWorker, input.ApproveRequestOffNotifyEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyAtHour, input.ApproveRequestOffNotifyAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyToChannels, input.ApproveRequestOffNotifyToChannels);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyTimePeriodWithPendingRequest, input.ApproveRequestOffNotifyTimePeriodWithPendingRequest);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_View)]
        public async Task<SendMessageRequestPendingTeamBuildingToHRConfigDto> GetSendMessageRequestPendingTeamBuildingToHRConfig()
        {
            return new SendMessageRequestPendingTeamBuildingToHRConfigDto
            {
                SendMessageRequestPendingTeamBuildingToHREnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHREnableWorker),
                SendMessageRequestPendingTeamBuildingToHRAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHRAtHour),
                SendMessageRequestPendingTeamBuildingToHRToChannels = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHRToChannels),
                SendMessageRequestPendingTeamBuildingToHREmail = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHREmail),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_Update)]
        public async Task<SendMessageRequestPendingTeamBuildingToHRConfigDto> SetSendMessageRequestPendingTeamBuildingToHRConfig(SendMessageRequestPendingTeamBuildingToHRConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHREnableWorker, input.SendMessageRequestPendingTeamBuildingToHREnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHRAtHour, input.SendMessageRequestPendingTeamBuildingToHRAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHRToChannels, input.SendMessageRequestPendingTeamBuildingToHRToChannels);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendMessageRequestPendingTeamBuildingToHREmail, input.SendMessageRequestPendingTeamBuildingToHREmail);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_View)]
        public async Task<NotifyHRTheEmployeeMayHaveLeftConfigDto> GetConfigNotifyHRTheEmployeeMayHaveLeft()
        {
            return new NotifyHRTheEmployeeMayHaveLeftConfigDto
            {
                NotifyHRTheEmployeeMayHaveLeftEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftEnableWorker),
                NotifyHRTheEmployeeMayHaveLeftAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftAtHour),
                NotifyHRTheEmployeeMayHaveLeftToChannels = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftToChannels),
                NotifyHRTheEmployeeMayHaveLeftToHREmail = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftToHREmail),
                NotifyHRTheEmployeeMayHaveLeftTimePeriod = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftTimePeriod),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_Update)]
        public async Task<NotifyHRTheEmployeeMayHaveLeftConfigDto> SetConfigNotifyHRTheEmployeeMayHaveLeft(NotifyHRTheEmployeeMayHaveLeftConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftEnableWorker, input.NotifyHRTheEmployeeMayHaveLeftEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftAtHour, input.NotifyHRTheEmployeeMayHaveLeftAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftToChannels, input.NotifyHRTheEmployeeMayHaveLeftToChannels);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftToHREmail, input.NotifyHRTheEmployeeMayHaveLeftToHREmail);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHRTheEmployeeMayHaveLeftTimePeriod, input.NotifyHRTheEmployeeMayHaveLeftTimePeriod);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_View)]
        public async Task<MoneyPMUnlockTimeSheetDto> GetConfigMoneyPMUnlockTimeSheet()
        {
            return new MoneyPMUnlockTimeSheetDto
            {
               MoneyPMUnlockTimeSheet  = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MoneyPMUnlockTimeSheet),            
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_Update)]
        public async Task<MoneyPMUnlockTimeSheetDto> SetConfigMoneyPMUnlockTimeSheet(MoneyPMUnlockTimeSheetDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MoneyPMUnlockTimeSheet, input.MoneyPMUnlockTimeSheet);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_View)]
        public async Task<SendMessageToPunishUserConfigDto> GetConfigSendMessageToPunishUser()
        {
            return new SendMessageToPunishUserConfigDto
            {
                SendMessageToPunishUserEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendMessageToPunishUserEnableWorker),
                SendMessageToPunishUserAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.SendMessageToPunishUserAtHour),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_Update)]
        public async Task<SendMessageToPunishUserConfigDto> SetConfigSendMessageToPunishUser(SendMessageToPunishUserConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendMessageToPunishUserEnableWorker, input.SendMessageToPunishUserEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.SendMessageToPunishUserAtHour, input.SendMessageToPunishUserAtHour);
            return input;
        }

        [HttpGet]
        public async Task<GetResultConnectDto> CheckConnectToProject()
        {
            return await _projectService.CheckConnectToProject();
        }

        [HttpGet]
        public async Task<GetResultConnectDto> CheckConnectToHRM()
        {
            return await _hRMv2Service.CheckConnectToHRM();
        }
    }
}
