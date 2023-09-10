using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.MyWorkingTimes.Dto
{
    public class HistoryWorkingTimeDto : ChangeWorkingTimeDto
    {
        public DateTime ReqestTime { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string LastModifierUser { get; set; }
    }
}
