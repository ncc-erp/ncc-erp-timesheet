using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;

namespace Timesheet.Services.Komu.Dto
{
    public class GetDailyReportDto
    {
        public List<DailyDto> daily { get; set; }
        public List<MentionDto> mention { get; set; }
        public List<WFHDto> wfh { get; set; }
        public List<TrackerDto> tracker { get; set; }
    }

    public class DailyDto
    {
        public string email { get; set; }
        public int count { get; set; }
    }
    public class MentionDto
    {
        public string name { get; set; }
        public int count { get; set; }
    }
    public class WFHDto
    {
        public string userid { get; set; }
        public int total { get; set; }
        public string name { get; set; }
    }
    public class TrackerDto
    {
        public string email { get; set; }
        public string spent_time { get; set; }
        public float SpentMinute => DateTimeUtils.ConvertHHmmssToMinutes(spent_time);
    }
}
