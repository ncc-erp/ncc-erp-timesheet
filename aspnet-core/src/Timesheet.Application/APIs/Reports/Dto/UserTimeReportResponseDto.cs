using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Reports.Dto
{
    public class UserTimeReportResponseDto
    {
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public List<UserTimeReportByLWAndLMDto> Users { get; set; }
    }
}
