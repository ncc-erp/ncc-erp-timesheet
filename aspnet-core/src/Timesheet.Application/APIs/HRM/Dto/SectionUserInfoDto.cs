using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.OverTimeHours.Dto
{
    public class SectionUserInfoDto
    {
        public double WorkingHour { get; set; }
        public int OpenTalk { get; set; }
        public int LeaveTime { get; set; }
        public int LeaveTimeNoneCharge { get; set; }
        public int RemainingTime { get; set; }
    }
}
