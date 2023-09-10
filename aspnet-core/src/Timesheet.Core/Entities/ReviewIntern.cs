using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Timesheet.Entities
{
    public class ReviewIntern : FullAuditedEntity<long>
    {
        public int Month { get; set; }
        public int Year { get; set; }
        //[DefaultValue("true")]
        public bool IsActive { get; set; }
    }
}
