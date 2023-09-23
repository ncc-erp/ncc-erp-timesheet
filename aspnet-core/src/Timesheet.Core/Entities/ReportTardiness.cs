using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Timesheet.Entities
{
    public class ReportTardiness : FullAuditedEntity<long>
    {
        [MaxLength(256)]
        public string UserName { get; set; }
        [MaxLength(256)]
        public string UserEmail { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int NumberOfTardies { get; set; }
        public int NumberOfLeaveEarly { get; set; }
    }
}
