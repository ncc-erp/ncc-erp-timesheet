using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.DomainServices.Dto;

namespace Timesheet.DomainServices.Dto
{
    public class AnomaliesTimelogReportDto
    {
        public string Yesterday { get; set; }
        public string LastWeekStart { get; set; }
        public string LastWeekEnd { get; set; }
        public List<YesterdayAnomalyDTO> YesterdayAnomalies { get; set; }
        public List<LastWeekAnomalyDTO> LastWeekAnomalies { get; set; }

        public AnomaliesTimelogReportDto()
        {
            YesterdayAnomalies = new List<YesterdayAnomalyDTO>();
            LastWeekAnomalies = new List<LastWeekAnomalyDTO>();
        }
    }

    public class GetAnomaliesTimelogReportInput
    {
        public List<long> BranchIds { get; set; } = new List<long>();
    }
}
