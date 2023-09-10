using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;


namespace Timesheet.APIs.ReviewDetails.Dto
{
    [AutoMapTo(typeof(ReviewDetail))]
    public class ReviewDetailInputDto : EntityDto<long>
    {
        public long ReviewId { get; set; }
        public long InternshipId { get; set; }
        
        public UserLevel? CurrentLevel { get; set; }
        public UserLevel? NewLevel { get; set; }
        public string Note { get; set; }
        public ReviewInternStatus Status { get; set; }
        public long? ReviewerId { get; set; }
        public long? PositionId { get; set; }
        public Usertype Type { get; set; } = Usertype.Internship;
    }
}
