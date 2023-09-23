using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;

namespace Timesheet.Services.Tracker.Dto
{
    public class GetSUerAndActiveTimeTrackerDto
    {
        public string email { get; set; }
        public string active_time { get; set; }
        public float ActiveMinute => DateTimeUtils.ConvertHHmmssToMinutes(active_time);
    }
}
