using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices;
using Timesheet.DomainServices.Dto;
using Timesheet.Paging;
using Timesheet.Services.Mezon;

namespace Timesheet.APIs.Reports
{
    [AbpAuthorize]
    public class ReportAppService : ApplicationService
    {
        private readonly IAbsenceDayServices _absenceDayService;
        private readonly IOfficeWorkingReportServices _officeWorkingReportService;
        private readonly IBotReportDailyService _botReportDailyService;

        public ReportAppService(AbsenceDayServices absenceDayService, OfficeWorkingReportServices officeWorkingReportService, BotReportDailyService botReportDailyService)
        {
            _absenceDayService = absenceDayService;
            _officeWorkingReportService = officeWorkingReportService;
            _botReportDailyService = botReportDailyService;
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.BranchDirector_OfficeWorkingReport_View)]
        public async Task<PagedResultDto<OfficeWorkingTopLWLMDto>> GetOfficeWorkingTimelogReport(GridParam param, GetOfficeWorkingTimelogReportInputDto input)
        {
            return await _officeWorkingReportService.GetOfficeWorkingTimelogReport(param, input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.BranchDirector_ProjectWorkingReport_View)]
        public async Task<PagedResultDto<TotalTimelogProjectDto>> GetDailyProjectTimelogReport(GridParam param, GetDailyProjectTimelogReportInput input)
        {
            return await _botReportDailyService.GetPagedDailyProjectTimelogReport(param, input);
        }

        [HttpGet]
        [AbpAuthorize(Ncc.Authorization.PermissionNames.BranchDirector_AnomaliesReport_View)]
        public async Task<AnomaliesTimelogReportDto> GetAnomaliesTimelogReport(GetAnomaliesTimelogReportInput input)
        {
            return await _absenceDayService.GetAnomaliesTimelogReport(input);
        }
    }
}