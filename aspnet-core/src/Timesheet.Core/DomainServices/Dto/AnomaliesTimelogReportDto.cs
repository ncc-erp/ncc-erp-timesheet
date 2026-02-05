using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.DomainServices.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class AnomaliesTimelogReportDto
    {
        public PagedResultDto<YesterdayAnomalyDTO> YesterdayAnomalies { get; set; }
        public PagedResultDto<LastWeekAnomalyDTO> LastWeekAnomalies { get; set; }

        public AnomaliesTimelogReportDto()
        {
            YesterdayAnomalies = new PagedResultDto<YesterdayAnomalyDTO>();
            LastWeekAnomalies = new PagedResultDto<LastWeekAnomalyDTO>();
        }
    }

    public class GetAnomaliesTimelogReportInput
    {
        public List<long> BranchIds { get; set; } = new List<long>();
        public EYesterdayAnomaliesSortColumn? YesterdayAnomaliesSortColumn { get; set; }
        public ELastWeekAnomaliesSortColumn? LastWeekAnomaliesSortColumn { get; set; }
        public ESortDirection SortDirection { get; set; }
    }
}