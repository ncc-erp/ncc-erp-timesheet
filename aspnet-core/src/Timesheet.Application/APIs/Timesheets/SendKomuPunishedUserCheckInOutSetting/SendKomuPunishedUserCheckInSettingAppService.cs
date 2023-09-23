using Abp.Authorization;
using Ncc;
using Ncc.Configuration;
using Ncc.Configuration.Dto;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.SendKomuPunishedUserCheckInOutSetting.Dto;

namespace Timesheet.APIs.Timesheets.SendKomuPunishedUserCheckInOutSetting
{
    public class SendKomuPunishedUserCheckInSettingAppService : AppServiceBase
    {
        public SendKomuPunishedUserCheckInSettingAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_View)]
        public async Task<KomuSendNotifyPunishedUser> GetPunishedCheckInConfig()
        {
            return new KomuSendNotifyPunishedUser
            {
                TimeSendPunishUser = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NofityKomuCheckInOutPunishmentAtHour),
                ChannelNotifyPunishUser = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.NofityKomuCheckInOutPunishmentToChannelId),
                PercentOfTrackerOnWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PercentOfTrackerOnWorking),

            };
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_Update)]
        public async Task<KomuSendNotifyPunishedUser> ChangePunishedCheckInConfig(KomuSendNotifyPunishedUser input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NofityKomuCheckInOutPunishmentAtHour, input.TimeSendPunishUser);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.NofityKomuCheckInOutPunishmentToChannelId, input.ChannelNotifyPunishUser);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PercentOfTrackerOnWorking, input.PercentOfTrackerOnWorking);
            return input;
        }
    }
}
