using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Sieve.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Entities
{
    public class Customer : FullAuditedEntity<long>
    {        
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
    }
}
