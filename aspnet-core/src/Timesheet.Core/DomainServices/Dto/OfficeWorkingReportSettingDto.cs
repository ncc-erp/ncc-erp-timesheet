using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class OfficeWorkingReportSettingDto
    {
        public bool enable { get; set; }
        public bool everyday { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }
        public string officeIds { get; set; }
        public int limit { get; set; }
        public string mezonUrl { get; set; }
    }
}