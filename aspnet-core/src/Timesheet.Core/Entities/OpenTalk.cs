using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Timesheet.Uitls;
namespace Timesheet.Entities
{
    public class OpenTalk : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public int totalTime {  get; set; }
    }
}
