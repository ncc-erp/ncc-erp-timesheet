using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ReviewInterns.Dto
{
    public class ReportInternDto
    {
        public int Month { get; set; }
        public List<ReportDetailDto> ReportDetails { get; set; }
    }
    public class ReportDetailDto
    {
        public long InternshipId { get; set; }
        public string InternName { get; set; }
        public long? ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public UserLevel? NewLevel { get; set; }
        public int Month { get; set; }
        public ReviewInternStatus ReviewInternStatus { get; set; }
        public int IsWarning { get; set; } = 0;
    }
    public class ReportInternForMonth
    {
        public string InternName { get; set; }
        public string EmailAddress { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public Branch? Branch { get; set; }
        public UserLevel? Level { get; set; }
        public Usertype? Type { get; set; }
        public List<ReviewResult> ReviewDetailForMonths { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
    }
    public class ReviewResult
    {
        public UserLevel? NewLevel { get; set; }
        public UserLevel? CurrentLevel { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string ReviewerName { get; set; }
        public InternWarningType WarningType { get; set; }
        public int IndexColumnExcel { get; set; }
        public bool HasReview { get; set; }
    }

    public class DraftReviewDetail
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public long InternshipId { get; set; }
        public UserLevel? NewLevel { get; set; }
    }



    public class ReportInternOutput
    {
        public List<ReportInternForMonth> listInternLevel { get; set; }
        public List<string> listMonth { get; set; }
    }

    public class MonthOfYear
    {
        public string year { get; set; }
        public string listMonth { get; set; }
    }

    public enum InternWarningType
    {
        Normal,
        Orange,
        Red,
        Staff
    }
}
