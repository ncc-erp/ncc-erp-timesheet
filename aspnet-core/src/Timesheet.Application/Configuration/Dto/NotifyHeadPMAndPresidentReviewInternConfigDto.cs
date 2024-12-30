using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class NotifyHeadPMAndPresidentReviewInternConfigDto
    {
        public string NotifyHeadPMAndPresidentReviewInternEnableWorker { get; set; }
        public string NotifyHeadPMAndPresidentReviewInternAtHour { get; set; }
        public string NotifyHeadPMAndPresidentReviewInternOnDate { get; set; }
        public string NotifyHeadPmMail { get; set; }
        public string NotifyPresidentEmail { get; set; }
    }
}
