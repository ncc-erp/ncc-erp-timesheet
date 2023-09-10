using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class AbsenceDayDetail : FullAuditedEntity<long>
    {
        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public double Hour { get; set; }
        public long RequestId { get; set; }
        public OnDayType? AbsenceTime { get; set; }

        [ForeignKey(nameof(RequestId))]
        public AbsenceDayRequest Request { get; set; }
    }
}
