using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.OverTimeHours.Dto
{
    public class ProjectOTSettingDto
    {
        public long ProjectId { get; set; }
        public DateTime DateAt { get; set; }
        public double Coefficient { get; set; }
    }
}
