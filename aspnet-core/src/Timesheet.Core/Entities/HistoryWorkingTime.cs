using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class HistoryWorkingTime : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public DateTime ApplyDate { get; set; }
        [MaxLength(5)]
        public string MorningStartTime { get; set; }
        [MaxLength(5)]
        public string MorningEndTime { get; set; }
        [MaxLength(5)]
        public double MorningWorkingTime { get; set; }
        [MaxLength(5)]
        public string AfternoonStartTime { get; set; }
        [MaxLength(5)]
        public string AfternoonEndTime { get; set; }
        [MaxLength(5)]
        public double AfternoonWorkingTime { get; set; }
        public RequestStatus Status { get; set; }
    }
}
