using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Timesheet.Entities
{
    public class Branch : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        [MaxLength(5)]
        public double MorningWorking { get; set; }
        [MaxLength(5)]
        public string MorningStartAt { get; set; }
        [MaxLength(5)]
        public string MorningEndAt { get; set; }
        [MaxLength(5)]
        public double AfternoonWorking { get; set; }
        [MaxLength(5)]
        public string AfternoonStartAt { get; set; }
        [MaxLength(5)]
        public string AfternoonEndAt { get; set; }
        public string Color { get; set; }
        public string Code { get; set; }
    }
}
