using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ncc.Authorization.Users;
using Ncc.Configuration.Dto;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
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
                NotifyAtHourType = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NRITNotifyAtHourType),
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
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyAtHourType, input.NotifyAtHourType);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyReviewDeadline, input.NotifyReviewDeadline);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyOnDates, input.NotifyOnDates);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyToChannels, input.NotifyToChannels);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NRITNotifyPenaltyFee, input.NotifyPenaltyFee);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NRITVMAEConfig_View)]
        public async Task<NotifyReviewInternViaMezonAndEmailConfigDto> GetNRITVMAEConfig()
        {
            return new NotifyReviewInternViaMezonAndEmailConfigDto
            {
                NotifyReviewInternEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyReviewInternEnableWorker),
                NotifyReviewInternIntervalMinutes = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyReviewInternIntervalMinutes),
                NotifyReviewInternAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyReviewInternAtHour),
                NotifyHeadPMReviewInternOnDate = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHeadPMReviewInternOnDate),
                NotifyPresidentReviewInternOnDate = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyPresidentReviewInternOnDate),
                NotifyHeadPmMail = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHeadPmMail),
                NotifyPresidentEmail = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyPresidentEmail),
                NotifyHrEmail = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyHrEmail),
                UpdateTimeCronjobAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.UpdateTimeCronjobAtHour),
                UpdateTimeCronjobOnDate = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.UpdateTimeCronjobOnDate),
                NotifyPmReviewInternOnDates = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NotifyPmReviewInternOnDates),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_NRITVMAEConfig_Update)]
        public async Task<NotifyReviewInternViaMezonAndEmailConfigDto> SetNRITVMAEConfig(NotifyReviewInternViaMezonAndEmailConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyReviewInternEnableWorker, input.NotifyReviewInternEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyReviewInternIntervalMinutes, input.NotifyReviewInternIntervalMinutes);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyReviewInternAtHour, input.NotifyReviewInternAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHeadPMReviewInternOnDate, input.NotifyHeadPMReviewInternOnDate);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyPresidentReviewInternOnDate, input.NotifyPresidentReviewInternOnDate);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHeadPmMail, input.NotifyHeadPmMail);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyPresidentEmail, input.NotifyPresidentEmail);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyHrEmail, input.NotifyHrEmail);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.UpdateTimeCronjobAtHour, input.UpdateTimeCronjobAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.UpdateTimeCronjobOnDate, input.UpdateTimeCronjobOnDate);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NotifyPmReviewInternOnDates, input.NotifyPmReviewInternOnDates);
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
                VAT = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.VAT),
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_TeamBuilding_Update)]
        public async Task<TeamBuildingConfigDto> SetTeamBuildingConfig(TeamBuildingConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.GenerateDataOnDate, input.GenerateDataOnDate);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.TeamBuildingMoney, input.TeamBuildingMoney);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.VAT, input.VAT);
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
                ApproveRequestOffSendUserAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveRequestOffSendUserAtHour),
                ApproveRequestOffNotifyToChannels = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyToChannels),
                ApproveRequestOffNotifyTimePeriodWithPendingRequest = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyTimePeriodWithPendingRequest),
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_Update)]
        public async Task<ApproveRequestOffConfigDto> SetApproveRequestOffNotifyConfig(ApproveRequestOffConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyEnableWorker, input.ApproveRequestOffNotifyEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveRequestOffNotifyAtHour, input.ApproveRequestOffNotifyAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ApproveRequestOffSendUserAtHour, input.ApproveRequestOffSendUserAtHour);
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

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CreateNewRetroConfig_View)]
        public async Task<CreateNewRetroConfigDto> GetConfigCreateNewRetro()
        {
            return new CreateNewRetroConfigDto
            {
                CreateNewRetroEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.CreateNewRetroEnableWorker),
                CreateNewRetroAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.CreateNewRetroAtHour),
                CreateNewRetroOnDate = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.CreateNewRetroOnDate)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CreateNewRetroConfig_Update)]
        public async Task<CreateNewRetroConfigDto> SetConfigCreateNewRetro(CreateNewRetroConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CreateNewRetroEnableWorker, input.CreateNewRetroEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CreateNewRetroAtHour, input.CreateNewRetroAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CreateNewRetroOnDate, input.CreateNewRetroOnDate);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_GenerateRetroResultConfig_View)]
        public async Task<GenerateRetroResultConfigDto> GetConfigGenerateRetroResult()
        {
            return new GenerateRetroResultConfigDto
            {
                GenerateRetroResultEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.GenerateRetroResultEnableWorker),
                GenerateRetroResultAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.GenerateRetroResultAtHour),
                GenerateRetroResultOnDate = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.GenerateRetroResultOnDate)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_GenerateRetroResultConfig_Update)]
        public async Task<GenerateRetroResultConfigDto> SetConfigGenerateRetroResult(GenerateRetroResultConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.GenerateRetroResultEnableWorker, input.GenerateRetroResultEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.GenerateRetroResultAtHour, input.GenerateRetroResultAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.GenerateRetroResultOnDate, input.GenerateRetroResultOnDate);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ResetDataTeamBuildingConfig_View)]
        public async Task<ResetDataTeamBuildingConfigDto> GetResetDataTeamBuildingConfig()
        {
            return new ResetDataTeamBuildingConfigDto
            {
                ResetDataTeamBuildingEnableWorker = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ResetDataTeamBuildingEnableWorker),
                ResetDataTeamBuildingAtHour = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ResetDataTeamBuildingAtHour),
                ResetDataTeamBuildingOnDateAndMonth = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ResetDataTeamBuildingOnDateAndMonth)
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_ResetDataTeamBuildingConfig_Update)]

        public async Task<ResetDataTeamBuildingConfigDto> SetResetDataTeamBuildingConfig(ResetDataTeamBuildingConfigDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ResetDataTeamBuildingEnableWorker, input.ResetDataTeamBuildingEnableWorker);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ResetDataTeamBuildingAtHour, input.ResetDataTeamBuildingAtHour);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ResetDataTeamBuildingOnDateAndMonth, input.ResetDataTeamBuildingOnDateAndMonth);
            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_LateInternReviewConfig_View)]
        public async Task<LateInternReviewSettingDto> GetLateInternReviewSetting()
        {
            return new LateInternReviewSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ReviewEnableWorker)),
                deadlineDay = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ReviewDeadlineDay)),
                startDayOfMonth = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ReviewStartDayOfMonth)),
                nextRunDate = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.ReviewNextRunDate))
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_LateInternReviewConfig_Update)]
        public async Task<LateInternReviewSettingDto> SetLateInternReviewSetting(LateInternReviewSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ReviewEnableWorker, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ReviewDeadlineDay, input.deadlineDay.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ReviewStartDayOfMonth, input.startDayOfMonth.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.ReviewNextRunDate, input.nextRunDate.ToString());

            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_PMReportConfig_View)]
        public async Task<PMReportPunishSettingDto> GetPMReportPunishSetting()
        {
            return new PMReportPunishSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PMReportPunishEnable)),
                hour = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PMReportPunishAtHour)),
                dayofweek = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PMReportPunishAtDayOfWeek)
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_PMReportConfig_Update)]
        public async Task<PMReportPunishSettingDto> SetPMReportPunishSetting(PMReportPunishSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PMReportPunishEnable, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PMReportPunishAtHour, input.hour.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PMReportPunishAtDayOfWeek, input.dayofweek);

            return input;
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_BotReportConfig_View)]
        public async Task<Timesheet.Configuration.Dto.BotReportSettingDto> GetBotReportSetting()
        {
            var result = new Timesheet.Configuration.Dto.BotReportSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportEnable)),
                everyday = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportEveryday)),
                hour = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportAtHour)),
                dayofweek = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportAtDayOfWeek),
                botUri = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportWebhookUrl),
                minHours = double.TryParse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportMinHours), out var minH) ? (double?)minH : null,
                topN = int.TryParse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportTopN), out var tN) ? (int?)tN : null,
                projectIds = JsonConvert.DeserializeObject<List<long>>(
                    await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportProjectIds)
                    ?? "[]"),
                branchCodes = JsonConvert.DeserializeObject<List<string>>(
                    await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.BotReportBranchCodes)
                    ?? "[]")
            };

            return result;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_BotReportConfig_Update)]
        public async Task<Timesheet.Configuration.Dto.BotReportSettingDto> SetBotReportSetting(Timesheet.Configuration.Dto.BotReportSettingDto input)
        {
            var projectIdsJson = JsonConvert.SerializeObject(input.projectIds ?? new List<long>());
            var branchCodesJson = JsonConvert.SerializeObject(input.branchCodes ?? new List<string>());          
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportEnable, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportEveryday, input.everyday.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportAtHour, input.hour.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportAtDayOfWeek, input.dayofweek);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportWebhookUrl, input.botUri);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportBranchCodes, branchCodesJson); 
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportMinHours, input.minHours?.ToString() ?? string.Empty);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportTopN, input.topN?.ToString() ?? string.Empty);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.BotReportProjectIds, projectIdsJson);

            return input;
        }
         [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_BotReportConfig_View)]
        public async Task<OfficeWorkingReportSettingDto> GetOfficeWorkingReportSetting()
        {
            return new OfficeWorkingReportSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OfficeWorkingReportEnable)),
                everyday = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OfficeWorkingEveryday)),
                hour = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OfficeWorkingReportAtHour)),
                officeIds = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OfficeWorkingReportOfficeIds),
                limit = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OfficeWorkingReportLimit)),
                mezonUrl = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.OfficeWorkingReportMezonUrl)
            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_BotReportConfig_Update)]
        public async Task<OfficeWorkingReportSettingDto> SetOfficeWorkingReportSetting(OfficeWorkingReportSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.OfficeWorkingReportEnable, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.OfficeWorkingEveryday, input.everyday.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.OfficeWorkingReportAtHour, input.hour.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.OfficeWorkingReportOfficeIds, input.officeIds);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.OfficeWorkingReportLimit, input.limit.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.OfficeWorkingReportMezonUrl, input.mezonUrl);

            return input;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_AnomaliesReportConfig_View)]
        public async Task<Timesheet.Configuration.Dto.AnomaliesReportSettingDto> GetAnomaliesReportSetting()
        {
            var result = new Timesheet.Configuration.Dto.AnomaliesReportSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AnomaliesReportEnable)),
                hour = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AnomaliesReportAtHour)),
                dayofweek = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AnomaliesReportAtDayOfWeek),
                botUri = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AnomaliesReportWebhookUrl),
                branchCodes = JsonConvert.DeserializeObject<List<string>>(
                    await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AnomaliesReportBranchCodes)
                    ?? "[]")
            };

            return result;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_AnomaliesReportConfig_Update)]
        public async Task<Timesheet.Configuration.Dto.AnomaliesReportSettingDto> SetAnomaliesReportSetting(Timesheet.Configuration.Dto.AnomaliesReportSettingDto input)
        {
            var branchCodesJson = JsonConvert.SerializeObject(input.branchCodes ?? new List<string>());

            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AnomaliesReportEnable, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AnomaliesReportAtHour, input.hour.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AnomaliesReportAtDayOfWeek, input.dayofweek);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AnomaliesReportWebhookUrl, input.botUri);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AnomaliesReportBranchCodes, branchCodesJson);

            return input;
        }
    }
}