using Abp.Authorization;
using Abp.Configuration;
using Ncc.Configuration;
using Ncc.IoC;
using Ncc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.Dto.LevelSettings;
using Timesheet.APIs.Timekeepings.Dto;
using Timesheet.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Timesheet.DomainServices.Dto;
using Abp.UI;

namespace Timesheet.APIs.Timekeepings
{
    public class CheckInCheckOutPunishmentSettingAppService : AppServiceBase
    {
        public CheckInCheckOutPunishmentSettingAppService(IWorkScope workScope) : base(workScope)
        {

        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_View)]
        public async Task<CheckInCheckOutPunishmentSettingAndPercentTrackerConfigDto> GetCheckInCheckOutPunishmentSetting()
        {
            var checkInCheckOutPunishmentSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInCheckOutPunishmentSetting);
            var rs = JsonConvert.DeserializeObject<List<CheckInCheckOutPunishmentSettingDto>>(checkInCheckOutPunishmentSetting);
            return new CheckInCheckOutPunishmentSettingAndPercentTrackerConfigDto
            {
                PercentOfTrackerOnWorking = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.PercentOfTrackerOnWorking),
                CheckInCheckOutPunishmentSetting = rs
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_Edit)]
        public async Task<InputToUpdateSettingDto> SetCheckInCheckOutPunishmentSetting(InputToUpdateSettingDto input)
        {
            var checkInCheckOutPunishmentSetting = await SettingManager.GetSettingValueAsync(AppSettingNames.CheckInCheckOutPunishmentSetting);
            var rs = JsonConvert.DeserializeObject<List<CheckInCheckOutPunishmentSettingDto>>(checkInCheckOutPunishmentSetting);
            var currentPunish = rs.Where(x => x.Id == input.Id).FirstOrDefault();
            if (currentPunish == default)
            {
                throw new UserFriendlyException($"Không tìm thấy loại phạt có Id = {input.Id}");    
            }
            currentPunish.Money = input.Money;

            string json = JsonConvert.SerializeObject(rs);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.CheckInCheckOutPunishmentSetting, json);
            return input;
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_Edit)]
        public async Task<string> SetPercentOfTrackerOnWorkingSetting(string input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.PercentOfTrackerOnWorking, input);
            return input;
        }
    }
}
