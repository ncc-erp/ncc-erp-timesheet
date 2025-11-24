using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.APIs.Reports;
using Timesheet.APIs.BotReportDaily.Dto;
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
        private readonly IAbsenceDayServices _absenceDayService;
        private readonly OfficeWorkingReportAppService _officeWorkingReportAppService;
        private readonly IBotReportDailyService _botReportDailyService;

        public ReportAppService(IWorkScope workScope, MezonService mezonService, ISettingManager settingManager, DomainServices.AbsenceDayServices absenceDayService, OfficeWorkingReportAppService officeWorkingReportAppService, DomainServices.BotReportDailyService botReportDailyService)
        {
            _workScope = workScope;
            _mezonService = mezonService;
            _settingManager = settingManager;
            _absenceDayService = absenceDayService;
            _officeWorkingReportAppService = officeWorkingReportAppService;
            _botReportDailyService = botReportDailyService;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.BranchDirector_Report, Ncc.Authorization.PermissionNames.BranchDirector_AnomaliesReport_View)]
        public async Task<AnomaliesTimelogReportDto> GetAnomaliesTimelogReport(GetAnomaliesTimelogReportInput input)
        {
            return await _absenceDayService.GetAnomaliesTimelogReport(input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.BranchDirector_Report, Ncc.Authorization.PermissionNames.BranchDirector_OfficeWorkingReport_View)]
        public async Task<OfficeWorkingTimelogReportDto> GetOfficeWorkingTimelogReport(GetOfficeWorkingTimelogReportInput input)
        {
            return await _officeWorkingReportAppService.GetOfficeWorkingTimelogReport(input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.BranchDirector_Report, Ncc.Authorization.PermissionNames.BranchDirector_ProjectWorkingReport_View)]
        public async Task<DailyProjectTimelogReportDto> GetDailyProjectTimelogReport(GetDailyProjectTimelogReportInput input)
        {
            return await _botReportDailyService.GetDailyProjectTimelogReport(input);
        }
    }
}