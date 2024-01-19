using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ReviewDetails.Dto
{
    [AutoMapTo(typeof(ReviewInternPrivateNote))]
    public class ReviewInternPrivateNoteDto : EntityDto<long>
    {

        public long ReviewDetailId { get; set; }

        public long NoteByUserId { get; set; }
        public string NoteByUserName { get; set; }

        public string PrivateNote { get; set; }

        public DateTime Created { get; set; }
        public ReviewInternNoteType ReviewInternNoteType { get; set; }

    }

    public class CreatePrivateNoteDto : EntityDto<long>
    {
        public long ReviewDetailId { get; set; }

        public ReviewInternStatus Status { get; set; }

        public string PrivateNote { get; set; }
    }

    public class HeadPmVerifyDto : EntityDto<long>
    {
        public long ReviewDetailId { get; set; }

        public ReviewInternStatus Status { get; set; }

    }
}
