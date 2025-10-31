using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
 
        public class DailyProjectTimelogReportDto
        {
            public string ReportDate { get; set; }
            public string LastWeekStart { get; set; }
            public string LastWeekEnd { get; set; }
            public string LastMonth { get; set; }
            public List<ProjectTimelogDto> Projects { get; set; }

            public DailyProjectTimelogReportDto()
            {
                Projects = new List<ProjectTimelogDto>();
            }
        }

        public class ProjectTimelogDto
        {
            public string Name { get; set; }
            public List<string> Members { get; set; }
            public double TotalTimelogLW { get; set; }
            public double TotalTimelogLM { get; set; } 

            public ProjectTimelogDto()
            {
                Members = new List<string>();
            }
        }

        public class GetDailyProjectTimelogReportInput
        {
            public List<long> BranchId { get; set; } = new List<long>();
            public double? MinHours { get; set; } = 0;
            public int? TopN { get; set; }
            public List<long> ProjectIds { get; set; } = new List<long>();
        }

        public class GetDailyProjectTimelogReportByBranchCodesInput
        {
            public List<string> BranchCodes { get; set; } = new List<string>();
            public double? MinHours { get; set; } = 0;
            public int? TopN { get; set; }
            public List<long> ProjectIds { get; set; } = new List<long>();
        }
}
