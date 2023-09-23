using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class Fund : FullAuditedEntity<long>
    {
        public double Amount { get; set; }
        public FundStatus Status { get; set; }
    }
}
