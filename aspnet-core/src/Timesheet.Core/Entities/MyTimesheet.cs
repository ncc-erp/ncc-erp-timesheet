using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using Ncc.Entities;
using Sieve.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class MyTimesheet: FullAuditedEntity<long>
    {
           
        public long ProjectTaskId { get; set; }

        [ForeignKey(nameof(ProjectTaskId))]
        public ProjectTask ProjectTask { get; set; }

        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public long? ProjectTargetUserId { get; set; }
        [ForeignKey(nameof(ProjectTargetUserId))]
        public ProjectTargetUser ProjectTargetUser { get; set; }

        public TypeOfWork TypeOfWork { get; set; }
        public bool IsCharged { get; set; }
        public string Note { get; set; }        

        public TimesheetStatus Status { get; set; }

        public DateTime DateAt { get; set; }
        public int WorkingTime { get; set; }//min     
        public int TargetUserWorkingTime { get; set; }
        public bool? IsUnlockedByEmployee { get; set; }
        public bool IsTemp { get; set; }
    }

}
