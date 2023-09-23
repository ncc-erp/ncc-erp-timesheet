using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Public.Dto
{
    public class InputGetTimesheetChartOfUserGroupDto
    {
        public string ProjectCode { get; set; }
        public List<string> Emails { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
