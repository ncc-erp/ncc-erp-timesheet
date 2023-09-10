using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;


namespace Timesheet.APIs.ReviewDetails.Dto
{
    public class ReviewDetailDto : EntityDto<long>
    {
        public long ReviewId { get; set; }
        [ApplySearchAttribute]
        public string InternName { get; set; }
        public long InternshipId { get; set; }
        public string ReviewerName { get; set; }
        public long? ReviewerId { get; set; }
        public UserLevel? CurrentLevel { get; set; }
        public bool IsUpOfficial { get; set; }
        public UserLevel? NewLevel { get; set; }
        public ReviewInternStatus Status { get; set; }
        public Branch? Branch { get; set; }
        public string Note { get; set; }
        public string InternAvatar { get; set; }
        public string InternFullPath => FileUtils.FullFilePath(InternAvatar);
        [ApplySearchAttribute]
        public string InternEmail { get; set; }
        public string ReviewerAvatar { get; set; }
        public string ReviewerAvatarFullPath => FileUtils.FullFilePath(ReviewerAvatar);
        public string ReviewerEmail { get; set; }
        public Usertype? Type { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long? UpdatedId { get; set; }
        public string UpdatedName { get; set; }
        public bool? IsFullSalary { get; set; }
        public byte? SubLevel { get; set; }
        public int? Salary { get; set; }
        public UserLevel? UserLevel { get; set; }
        public float? RateStar { get; set; }
        public float? PreviousRateStar { get; set; }
        [DefaultValue(false)]
        public bool IsFirst4M { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
        public long? BranchId { get; set; }
        public long? PositionId { get; set; }
        public string PositionShortName { get; set; }
        public string PositionColor { get; set; }
        public float? Average { get; set; }
        public float? PreviousAverage { get; set; }
    }
}
