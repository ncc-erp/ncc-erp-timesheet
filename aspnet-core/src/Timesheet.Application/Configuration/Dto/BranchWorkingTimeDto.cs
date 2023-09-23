using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class BranchWorkingTimeDto : Entity<long>
    {
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
        public string MorningWorking { get; set; }
        public string AfternoonWorking { get; set; }
    }
}
