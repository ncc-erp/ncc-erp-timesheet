using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.Komu.Dto
{
    public class GetDailyReportDto
    {
        public List<DailyDto> daily { get; set; }
        public List<DailyDto> mention { get; set; }
    }

    public class DailyDto
    {
        public string email { get; set; }
        public int count { get; set; }
    }
}
