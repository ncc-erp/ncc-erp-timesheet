using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace Timesheet.Entities
{
    public class ReviewInternCapability : FullAuditedEntity<long>
    {
        public long ReviewDetailId { get; set; }
        [ForeignKey(nameof(ReviewDetailId))]
        public ReviewDetail ReviewDetail { get; set; }
        public long CapabilityId { get; set; }
        [ForeignKey(nameof(CapabilityId))]
        public Capability Capability { get; set; }
        public float Point { get; set; }
        public float Coefficient { get; set; }
        public string Note { get; set; }
    }
}