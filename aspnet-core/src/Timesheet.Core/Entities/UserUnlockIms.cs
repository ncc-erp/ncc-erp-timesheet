using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class UserUnlockIms : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        //public int Month { get; set; }
        //public int Year { get; set; }
        public LockUnlockTimesheetType Type { get; set; }
        public int Times { get; set; }
        public double Amount { get; set; }
        public bool IsPayment { get; set; }
    }
}
