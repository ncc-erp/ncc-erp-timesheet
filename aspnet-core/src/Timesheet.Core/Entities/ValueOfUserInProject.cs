using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Timesheet.Entities
{
    public class ValueOfUserInProject : FullAuditedEntity<long>
    {
        public long UserId { get; set; }
        public long ProjectId { get; set; }
        public ValueOfUserType Type { get; set; }
        public float ShadowPercentage { get; set; }

        #region Foreign Keys

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }

        #endregion
    }

    public enum ValueOfUserType
    {
        Member = 0,
        Expose = 1,
        Shadow = 2,
    }
}
