using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.MyWorkingTimes.Dto
{
    public class ChangeWorkingTimeDto : GetMyWorkingTimeDto
    {
        public long Id { get;set; }
        public DateTime ApplyDate { get; set; }
    }
}
