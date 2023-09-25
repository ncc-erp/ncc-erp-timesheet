using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class ReviewInternComment : FullAuditedEntity<long>
    {
        public long ReviewDetailId { get; set; }

        [ForeignKey(nameof(ReviewDetailId))]
        public ReviewDetail Review { get; set; }
      
        public long? CommentUserId { get; set; }

        [ForeignKey(nameof(CommentUserId))]        
        public User CommentUser { get; set; }

        public string PrivateNote { get; set; }

    }
}
