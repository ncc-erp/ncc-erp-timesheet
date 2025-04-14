using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Paging;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class InputAbsenceReportDto: GridParam
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string email { get; set; }
        public long? positionId { get; set; }
        public long? branchId { get; set; }
        public RequestType? requestType { get; set; }
    }
}
