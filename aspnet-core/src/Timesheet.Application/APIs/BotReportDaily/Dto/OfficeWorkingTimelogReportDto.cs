using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.Reports.Dto;
using Timesheet.Paging;

namespace Timesheet.APIs.BotReportDaily.Dto
{
    public class OfficeWorkingTimelogReportDto
    {
        public string LastWeekStart { get; set; }
        public string LastWeekEnd { get; set; }
        public string LastMonth { get; set; }
        public List<OfficeWorkingTopLWLMDto> OfficeWorkingData { get; set; }

        public OfficeWorkingTimelogReportDto()
        {
            OfficeWorkingData = new List<OfficeWorkingTopLWLMDto>();
        }
    }
    public class GetOfficeWorkingTimelogReportInput
    {
        public List<long> BranchId { get; set; } = new List<long>();
        public int Limit { get; set; } = int.MaxValue;
    }
    public class GetOfficeWorkingTimelogReportRequestDto
    {
        public GridParam Param { get; set; } = new GridParam();
        public GetOfficeWorkingTimelogReportInput Input { get; set; } = new GetOfficeWorkingTimelogReportInput();
    }
    public class PagedOfficeWorkingTopLWLMDto : PagedResultDto<OfficeWorkingTopLWLMDto>
    {
        public string LastWeekStart { get; set; }
        public string LastWeekEnd { get; set; }
        public string LastMonth { get; set; }
    }
}
