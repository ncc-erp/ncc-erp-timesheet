using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ncc.Authorization.Users;
using Ncc.Configuration;
using Ncc.Entities;
using Ncc.Entities.Enum;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.DomainServices;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.BotReportDaily
{
    [AbpAuthorize]
    public class BotReportDailyAppService : ApplicationService
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private readonly ISettingManager _settingManager;
        private readonly IBotReportDailyService _botReportDailyService;

        public BotReportDailyAppService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager, DomainServices.BotReportDailyService botReportDailyService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
            _settingManager = settingManager;
            _botReportDailyService = botReportDailyService;
        }

        [HttpGet]
        public async Task<bool> SendDailyProjectTimelogToMezon(GetDailyProjectTimelogReportInput input)
        {
            return await _botReportDailyService.SendDailyProjectTimelogToMezon(input);
        }
    }
}
