using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Sieve.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Entities
{
    public class Task: FullAuditedEntity<long>
    {
        public string Name { get; set; }
        public TaskType Type { get; set; }
    }
}
