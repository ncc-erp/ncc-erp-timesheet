using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.OverTimeHours.Dto
{
    public class DayOffSettingDto
    {
        public Branch? Branch { get; set; }
        public DateTime DateAt { get; set; }
        public double Coefficient { get; set; }
    }
}
