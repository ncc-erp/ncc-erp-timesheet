using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc;
using Ncc.Authorization;
using Ncc.IoC;
using System;
using System.Threading.Tasks;
using Timesheet.Core;
using Timesheet.DomainServices.Dto;
using Timesheet.Services.Mezon;

namespace Timesheet.Application
{
    [AbpAuthorize(PermissionNames.Timekeeping_ViewAnomaliesList)]
    public class BotReportDailyAppService : ApplicationService
    {
        private readonly IAbsenceDayServices _absenceDayServices;
        private readonly IWorkScope _workScope;
        private readonly ISettingManager _settingManager;
        private readonly MezonService _mezonService;

        public BotReportDailyAppService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager, IAbsenceDayServices absenceDayService)
        {
            _absenceDayServices = absenceDayService;
            _workScope = workScope;
            _settingManager = settingManager;
            _mezonService = mezonService;
        }

        [HttpGet]
        public async Task<bool> GetAnomalies([FromQuery] BotReportSettingDto input, [FromQuery] bool isWeekly)
        {
            return await _absenceDayServices.SendDailyAnomaliesToMezon(input, isWeekly);
        }
    }
}