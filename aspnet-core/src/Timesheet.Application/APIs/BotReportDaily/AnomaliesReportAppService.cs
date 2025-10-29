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
    public class AnomaliesReportAppService : ApplicationService
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private readonly ISettingManager _settingManager;
        private readonly IAbsenceDayService _absenceDayService;

        public AnomaliesReportAppService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager, DomainServices.AbsenceDayService absenceDayService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
            _settingManager = settingManager;
            _absenceDayService = absenceDayService;
        }

        [HttpGet]
        public async Task<AnomaliesTimelogReportDto> GetAnomaliesTimelogReport(GetAnomaliesTimelogReportInput input)
        {
            return await _absenceDayService.GetAnomaliesTimelogReport(input);
        }
    }
}
