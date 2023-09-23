using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ncc.Entities
{
    public class ProjectUser: FullAuditedEntity<long>
    {
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public long UserId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }
        public long ProjectId { get; set; }
                
        public ProjectUserType Type { get; set; }
        public bool IsTemp { get; set; }

    }

    public enum ProjectUserType
    {
        Member = 0,
        PM = 1,
        Shadow = 2,
        DeActive = 3//users do project but not report for customer( example: mr.Binh do UCG but not report for UCG)
    }
}
