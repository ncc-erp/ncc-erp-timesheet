using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.InternInfo.Dto;
using Timesheet.Paging;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class AbsenceResultDto
    {
        public GridResult<AbsenceReportDto> ListAbsenceRequests { get; set; }
    }
}
