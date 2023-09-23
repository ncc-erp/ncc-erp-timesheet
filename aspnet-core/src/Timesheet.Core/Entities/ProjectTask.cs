using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ncc.Entities
{
    public class ProjectTask : FullAuditedEntity<long>
    {
        [ForeignKey(nameof(TaskId))]
        public Task Task { get; set; }
        public long TaskId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }
        public long ProjectId { get; set; }
        
        public bool Billable { get; set; }
    }
}
