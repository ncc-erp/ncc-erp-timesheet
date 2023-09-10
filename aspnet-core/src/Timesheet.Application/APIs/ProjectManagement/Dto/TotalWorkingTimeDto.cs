using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.ProjectManagement.Dto
{
    public class TotalWorkingTimeDto
    {
        public int NormalWorkingMinute { get; set; }
        public int OTMinute { get; set; }
        public int OTNoChargeMinute { get; set; }
        public string ProjectCode { get; set; }
        public long ProjectId { get; set; }
        public double NormalWorkingTimeAll { get; set; }
        public double NormalWorkingTimeStandard { get; set; }

        public float NormalWorkingTime => (float)NormalWorkingMinute / 60;
        public float OverTime => (float)OTMinute / 60;
        public float OverTimeNoCharge => (float)OTNoChargeMinute / 60;
    }
}
