using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ReviewInterns.Dto
{
    public class ReviewDataDto
    {
        public int MonthReviewIntern { get; set; }
        public int YearReviewIntern { get; set; }
        public List<ReviewerAssignmentDto> Reviewers { get; set; }
    }

    public class ReviewerAssignmentDto
    {
        public long ReviewerId { get; set; }
        public string ReviewerUserName { get; set; }
        public string ReviewerFullName { get; set; }
        public string ReviewerEmail { get; set; }
        public List<InternInfoForReviewDto> Interns { get; set; }
    }

    public class InternInfoForReviewDto
    {
        public long InternId { get; set; }
        public string InternUserName { get; set; }
        public string InternFullName { get; set; }
        public UserLevel? InternCurrentLevel { get; set; }
        public UserLevel? InternNewLevel { get; set; }
        public ReviewInternStatus ReviewStatus { get; set; }
    }

    public class ReviewDetailForHRDto
    {
        public string ReviewerFullName { get; set; }
        public string ReviewerUserName { get; set; }
        public string InternFullName { get; set; }
        public string InternUserName { get; set; }
        public UserLevel? InternCurrentLevel { get; set; }
        public UserLevel? InternNewLevel { get; set; }
        public ReviewInternStatus ReviewStatus { get; set; }
    }
}
