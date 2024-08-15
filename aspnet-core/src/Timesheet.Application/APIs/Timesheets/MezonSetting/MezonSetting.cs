using Abp.Application.Services;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.IoC;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.APIs.Timesheets.MezonSetting.Dto;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using Timesheet.Services.Mezon.Dto;

namespace Timesheet.APIs.Timesheets.MezonSetting
{
    [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration)]
    public class MezonSetting : ApplicationService
    {
        protected IWorkScope WorkScope { get; set; }
        private readonly MezonService _mezonService;
        public MezonSetting(IWorkScope workScope, MezonService mezonService)
        {
            WorkScope = workScope;
            _mezonService = mezonService;
        }

        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MezonSetting_View)]
        public async Task<MezonSettingDto> Get()
        {
            return new MezonSettingDto
            {
                enable = bool.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AddDataToOpenTalkEnable)),
                hour = int.Parse(await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtHour)),
                dayofweek = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtDayOfWeek),
                secretCode = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MezonSecurityCode),
                uri = await SettingManager.GetSettingValueForApplicationAsync(AppSettingNames.MezonBaseAddress)
            };
        }
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MezonSetting_Edit)]
        public async Task<MezonSettingDto> Change(MezonSettingDto input)
        {
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AddDataToOpenTalkEnable, input.enable.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtHour, input.hour.ToString());
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.AddDataToOpenTalkAtDayOfWeek, input.dayofweek);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MezonSecurityCode, input.secretCode);
            await SettingManager.ChangeSettingForApplicationAsync(AppSettingNames.MezonBaseAddress, input.uri);
            return input;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.Admin_Configuration_MezonSetting_Edit)]
        public async System.Threading.Tasks.Task createOpentalkLog(DateTime? date)
        {
            OpenTalkListDto[] userList = _mezonService.GetOpenTalkLog(date);
            var userDict = userList.ToDictionary(s => s.googleId, s => s);
            var OpentalkList = await WorkScope.GetAll<User>().Where(s => s.GoogleId != null && userDict.ContainsKey(s.GoogleId))
                                      .Select(s => new OpenTalk
                                      {
                                          UserId = s.Id,
                                          DateAt = userDict[s.GoogleId].date,
                                          totalTime = userDict[s.GoogleId].totalTime
                                      }).ToListAsync();
            foreach (var opentalk in OpentalkList)
            {
                var dbRequest = await WorkScope.GetAll<OpenTalk>().Where(s => s.UserId == opentalk.UserId)
                                                                  .Where(s => s.DateAt.Date == opentalk.DateAt.Date)
                                                                  .FirstOrDefaultAsync();
                if (dbRequest == null)
                {
                    await WorkScope.InsertAsync(opentalk);
                }
                else
                {
                    dbRequest.totalTime = opentalk.totalTime;
                    await WorkScope.UpdateAsync(dbRequest);
                }
            }
        }
    }
}
