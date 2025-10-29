using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices
{
    public interface IAbsenceDayService : IDomainService
    {
        Task<List<YesterdayAnomalyDTO>> GetYesterdayAnomalies(string branchName, DateTime date);
        Task<List<LastWeekAnomalyDTO>> GetLastWeekAnomalies(string branchName, DateTime startDate, DateTime endDate);
        Task<bool> SendDailyAnomaliesToMezon(AnomaliesReportSettingDto input, bool isWeekly);
        Task<AnomaliesTimelogReportDto> GetAnomaliesTimelogReport(AnomaliesTimelogReportInputDto input);
    }
}