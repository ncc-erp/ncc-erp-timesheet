using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class ProjectHistoryDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public ProjectUserType ProjectUserType { get; set; }
        public List<ProjectIntervalDto> Intervals { get; set; }
        public List<MonthlyEffortDto> MonthlyEfforts { get; set; }
        public ProjectStatus Status { get; set; }
    }

    public class ProjectIntervalDto
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class MonthlyEffortDto
    {
        public string MonthYear { get; set; }
        public double Effort { get; set; }
    }
}
