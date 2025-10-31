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
using Timesheet.APIs.BotReportDaily.Dto;
using Timesheet.APIs.Reports;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Services.Mezon;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.BotReportDaily
{
    [AbpAuthorize]
    public class OfficeReportAppService : ApplicationService
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private readonly ISettingManager _settingManager;
        private readonly OfficeWorkingReportAppService _officeWorkingReportAppService;

        public OfficeReportAppService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager, OfficeWorkingReportAppService officeWorkingReportAppService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
            _settingManager = settingManager;
            _officeWorkingReportAppService = officeWorkingReportAppService;
        }

        [HttpGet]
        public async Task<OfficeWorkingTimelogReportDto> GetOfficeWorkingTimelogReport(GetOfficeWorkingTimelogReportInput input)
        {
            return await _officeWorkingReportAppService.GetOfficeWorkingTimelogReport(input);
        }
    }
}