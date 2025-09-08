using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Reports.Dto
{
    public class OfficeWorkingTopDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string OfficeName { get; set; }
        public string OfficeCode { get; set; }
        public int TotalMinutesLW { get; set; }
        public double TotalHoursLW => Math.Round(TotalMinutesLW / 60.0, 2);
    }
}
