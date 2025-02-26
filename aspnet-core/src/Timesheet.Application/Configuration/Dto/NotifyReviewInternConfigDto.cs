using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class NotifyReviewInternViaMezonAndEmailConfigDto
    {
        public string NotifyReviewInternEnableWorker { get; set; }
        public string NotifyReviewInternIntervalMinutes { get; set; }
        public string NotifyReviewInternAtHour { get; set; }
        public string NotifyHeadPMReviewInternOnDate { get; set; }
        public string NotifyPresidentReviewInternOnDate { get; set; }
        public string NotifyHeadPmMail { get; set; }
        public string NotifyPresidentEmail { get; set; }
        public string NotifyHrEmail { get; set; }
        public string UpdateTimeCronjobAtHour { get; set; }
        public string UpdateTimeCronjobOnDate { get; set; }
        public string NotifyPmReviewInternOnDates { get; set; }
    }
}
