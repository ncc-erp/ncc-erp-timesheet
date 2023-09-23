using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ncc.Entities
{
    public class ProjectTargetUser : FullAuditedEntity<long>
    {
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public long UserId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }
        public long ProjectId { get; set; }

        public string RoleName { get; set; }
    }
}
