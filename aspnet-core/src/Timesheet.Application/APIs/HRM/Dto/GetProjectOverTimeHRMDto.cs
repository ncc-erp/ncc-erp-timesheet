using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRM.Dto
{
    public class GetProjectOverTimeHRMDto
    {
        public long UserId { get; set; }
        public IEnumerable<ProjectOverTimeDetailDto> ProjectOverTime { get; set; }
    }
    public class ProjectOverTimeDetailDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double TotalWorkingHour { get; set; }
    }
}
