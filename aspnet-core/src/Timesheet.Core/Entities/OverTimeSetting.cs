using Abp.Domain.Entities.Auditing;
using Ncc.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Entities
{
    public class OverTimeSetting : FullAuditedEntity<long>
    {
        [ForeignKey(nameof(ProjectId))]
        public long ProjectId { get; set; }
        public Project Project { get; set; }
        public DateTime DateAt { get; set; }
        public string Note { get; set; }
        public double Coefficient { get; set; }
    }
}
