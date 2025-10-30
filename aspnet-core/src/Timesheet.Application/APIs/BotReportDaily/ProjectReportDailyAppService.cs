using Abp.Application.Services;
using Abp.Application.Services.Dto;
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
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.BotReportDaily
{
    [AbpAuthorize]
    public class ProjectReportAppService : ApplicationService
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private readonly ISettingManager _settingManager;
        private readonly IBotReportDailyService _botReportDailyService;

        public ProjectReportAppService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager, DomainServices.BotReportDailyService botReportDailyService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
            _settingManager = settingManager;
            _botReportDailyService = botReportDailyService;
        }

        [HttpPost]
        public async Task<PagedProjectTimelogDto> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportRequestDto request)
        {
            return await _botReportDailyService.GetDailyProjectTimelogReport(request);
        }
    }
}
