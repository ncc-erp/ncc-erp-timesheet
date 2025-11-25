using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.BotReportDaily.Dto;
using Timesheet.APIs.Reports;
using Timesheet.APIs.Reports.Dto;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Services.Mezon;

namespace Timesheet.APIs.BotReportDaily
{
    [AbpAuthorize]
    public class ReportAppService : ApplicationService
    {
        private readonly IWorkScope _workScope;
        private readonly MezonService _mezonService;
        private readonly ISettingManager _settingManager;
        private readonly OfficeWorkingReportAppService _officeWorkingReportAppService;

        public ReportAppService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager, OfficeWorkingReportAppService officeWorkingReportAppService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
            _settingManager = settingManager;
            _officeWorkingReportAppService = officeWorkingReportAppService;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.BranchDirector_Report, Ncc.Authorization.PermissionNames.BranchDirector_OfficeWorkingReport_View)]
        public async Task<List<OfficeWorkingTopLWLMDto>> GetOfficeWorkingTimelogReport(GetOfficeWorkingTimelogReportInputDto input)
        {
            return await _officeWorkingReportAppService.GetOfficeWorkingTimelogReport(input);
        }
    }
}
