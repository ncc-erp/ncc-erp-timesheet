using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Entities
{
    public class Position : FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Code { get; set; }
        public string Color { get; set; }
    }
}