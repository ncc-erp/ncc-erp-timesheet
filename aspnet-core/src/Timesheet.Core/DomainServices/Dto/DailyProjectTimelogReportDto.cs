using System;
using System.Collections.Generic;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class TotalTimelogProjectDto
    {
        public string Name { get; set; }
        public List<string> Members { get; set; }
        public double TotalTimelogLW { get; set; }
        public double TotalTimelogLM { get; set; }
        public TotalTimelogProjectDto()
        {
            Members = new List<string>();
        }
    }

    public class GetDailyProjectTimelogReportInput
    {
        public List<long> BranchIds { get; set; } = new List<long>();
        public double? MinHours { get; set; } = 0;
        public int? Limit { get; set; }
        public List<long> ProjectIds { get; set; } = new List<long>();
        public bool IsAllBranch { get; set; }
    }

    public class GetDailyProjectTimelogReportByBranchCodesInput
    {
        public List<string> BranchCodes { get; set; } = new List<string>();
        public double? MinHours { get; set; } = 0;
        public int? TopN { get; set; }
        public List<long> ProjectIds { get; set; } = new List<long>();
    }
}
