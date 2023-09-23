using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class ReviewDetail : FullAuditedEntity<long>
    {
        
        public long ReviewId { get; set; }

        [ForeignKey(nameof(ReviewId))]
        public ReviewIntern Review { get; set; }
      
        public long InternshipId { get; set; }

        [ForeignKey(nameof(InternshipId))]
        public User InterShip { get; set; }
        public long? ReviewerId { get; set; }

        [ForeignKey(nameof(ReviewerId))]        
        public User Reviewer { get; set; }
        public UserLevel? CurrentLevel { get; set; }
        public UserLevel? NewLevel { get; set; }
        public ReviewInternStatus Status { get; set; }
        public string Note { get; set; }
        public Usertype Type { get; set; }
        public bool? IsFullSalary { get; set; }
        public byte? SubLevel { get; set; }
        public int? Salary { get; set; }
        public float? RateStar { get; set; }
    }
}
