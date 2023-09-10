using Abp.Domain.Entities.Auditing;
using Ncc.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Entities
{
    public class KomuTracker : FullAuditedEntity<long>
    {
        public DateTime DateAt { get; set; }
        public string EmailAddress { get; set; }
        public string ComputerName { get; set; }
        public float WorkingMinute { get; set; }
    }
}
