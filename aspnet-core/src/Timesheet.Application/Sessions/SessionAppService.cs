using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Auditing;
using Ncc.Configuration;
using Ncc.IoC;
using Ncc.Sessions.Dto;

namespace Ncc.Sessions
{
    public class SessionAppService : AppServiceBase, ISessionAppService
    {
        public SessionAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [DisableAuditing]
        public async Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations()
        {
            var output = new GetCurrentLoginInformationsOutput
            {
                Application = new ApplicationInfoDto
                {
                    Version = AppVersionHelper.Version,
                    ReleaseDate = AppVersionHelper.ReleaseDate,
                    Features = new Dictionary<string, bool>()
                }
            };

            if (AbpSession.TenantId.HasValue)
            {
                output.Tenant = ObjectMapper.Map<TenantLoginInfoDto>(await GetCurrentTenantAsync());
            }

            if (AbpSession.UserId.HasValue)
            {
                output.User = ObjectMapper.Map<UserLoginInfoDto>(await GetCurrentUserAsync());
                output.User.AvatarPath = !string.IsNullOrEmpty(output.User.AvatarPath) ?  output.User.AvatarPath : "";

                //if (output.User.isWorkingTimeDefault == true || output.User.isWorkingTimeDefault == null)
                //{
                //    if (output.User.Branch == Entities.Enum.StatusEnum.Branch.DaNang)
                //    {
                //        output.User.MorningWorking = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningDNWorking);
                //        output.User.MorningStartAt = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningDNStartAt);
                //        output.User.MorningEndAt = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningDNEndAt);
                //        output.User.AfternoonWorking = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonDNWorking);
                //        output.User.AfternoonStartAt = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonDNStartAt);
                //        output.User.AfternoonEndAt = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonDNEndAt);
                //    }
                //    else if (output.User.Branch == Entities.Enum.StatusEnum.Branch.HaNoi)
                //    {
                //        output.User.MorningWorking = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningHNWorking);
                //        output.User.MorningStartAt = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningHNStartAt);
                //        output.User.MorningEndAt = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningHNEndAt);
                //        output.User.AfternoonWorking = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonHNWorking);
                //        output.User.AfternoonStartAt = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonHNStartAt);
                //        output.User.AfternoonEndAt = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonHNEndAt);
                //    }
                //    else
                //    {
                //        output.User.MorningWorking = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningHCMWorking);
                //        output.User.MorningStartAt = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningHCMStartAt);
                //        output.User.MorningEndAt = await SettingManager.GetSettingValueAsync(AppSettingNames.MorningHCMEndAt);
                //        output.User.AfternoonWorking = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonHCMWorking);
                //        output.User.AfternoonStartAt = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonHCMStartAt);
                //        output.User.AfternoonEndAt = await SettingManager.GetSettingValueAsync(AppSettingNames.AfternoonHCMEndAt);
                //    }
                //}
            }

            return output;
        }
    }
}
