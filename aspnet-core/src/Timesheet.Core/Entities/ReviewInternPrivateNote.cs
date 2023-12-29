using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Entities
{
    public class ReviewInternPrivateNote : FullAuditedEntity<long>
    {
        public long ReviewDetailId { get; set; }

        public long NoteByUserId { get; set; }

        public string PrivateNote { get; set; }
        public ReviewInternNoteType ReviewInternNoteType { get; set; }

        [ForeignKey(nameof(ReviewDetailId))]
        public ReviewDetail Review { get; set; }

        [ForeignKey(nameof(NoteByUserId))]
        public User NoteByUser { get; set; }

    }
}
