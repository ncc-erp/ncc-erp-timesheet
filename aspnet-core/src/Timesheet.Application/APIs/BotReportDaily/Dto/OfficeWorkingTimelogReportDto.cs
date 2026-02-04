using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.Reports.Dto;

namespace Timesheet.APIs.BotReportDaily.Dto
{
    public class GetOfficeWorkingTimelogReportInputDto
    {
        public List<long> BranchId { get; set; } = new List<long>();
        public int Limit { get; set; } = int.MaxValue;
    }
}