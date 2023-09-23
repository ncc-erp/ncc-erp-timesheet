using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class AbsenceDayRequest : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public long DayOffTypeId { get; set; }
        [ForeignKey(nameof(DayOffTypeId))]
        public DayOffType DayOffType { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
        public RequestType Type { get; set; }
    }
}
