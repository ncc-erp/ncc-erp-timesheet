using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class RetroResult : FullAuditedEntity<long>
    {
        [ForeignKey(nameof(RetroId))]
        public virtual Retro Retro { get; set; }

        public long RetroId { get; set; }

        public float Point { get; set; }

        public string Note { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public long UserId { get; set; }

        public Usertype UserType { get; set; }
        public UserLevel UserLevel { get; set; }

        [ForeignKey(nameof(PositionId))]
        public virtual Position Position { get; set; }

        public long PositionId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; }

        public long ProjectId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public virtual Branch Branch { get; set; }

        public long? BranchId { get; set; }
        [ForeignKey(nameof(PmId))]
        public virtual User Pm { get; set; }

        public long? PmId { get; set; }
    }
}