using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class DayOffType : FullAuditedEntity<long>
    {
        public OffTypeStatus Status { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }

    }
}
