using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Entities
{
    public class DayOffSetting : FullAuditedEntity<long>
    {
        public DateTime DayOff { get; set; }

        public string Name { get; set; }
        public double Coefficient { get; set; }
        public Ncc.Entities.Enum.StatusEnum.Branch? Branch { get; set; }
 
    }
}
