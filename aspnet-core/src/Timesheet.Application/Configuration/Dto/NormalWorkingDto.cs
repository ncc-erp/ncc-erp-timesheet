using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class NormalWorkingDto
    {
        public string MorningHNStartAt { get; set; }
        public string MorningHNEndAt { get; set; }
        public string AfternoonHNStartAt { get; set; }
        public string AfternoonHNEndAt { get; set; }
        public string MorningDNStartAt { get; set; }
        public string MorningDNEndAt { get; set; }
        public string AfternoonDNStartAt { get; set; }
        public string AfternoonDNEndAt { get; set; }
        public string MorningHCMStartAt { get; set; }
        public string MorningHCMEndAt { get; set; }
        public string AfternoonHCMStartAt { get; set; }
        public string AfternoonHCMEndAt { get; set; }
        public string MorningVinhStartAt { get; set; }
        public string MorningVinhEndAt { get; set; }
        public string AfternoonVinhStartAt { get; set; }
        public string AfternoonVinhEndAt { get; set; }

        public string MorningHNWorking { get; set; }
        public string MorningDNWorking { get; set; }
        public string MorningHCMWorking { get; set; }
        public string MorningVinhWorking { get; set; }
        public string AfternoonHNWorking { get; set; }
        public string AfternoonDNWorking { get; set; }
        public string AfternoonHCMWorking { get; set; }
        public string AfternoonVinhWorking { get; set; }
        public string EmailHR { get; set; }
        public string EmailHRDN { get; set; }
        public string EmailHRHCM { get; set; }
        public string EmailHRVinh { get; set; }

    }
}
