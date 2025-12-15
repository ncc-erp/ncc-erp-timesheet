using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.ReviewInterns.Dto
{
    public class ReviewDataDto
    {
        public ReviewIntern ReviewIntern { get; set; }
        public List<ReviewDetail> ReviewDetails { get; set; }
        public List<User> Reviewers { get; set; }
        public List<User> Internships { get; set; }
        public int MonthReviewIntern { get; set; }
        public int YearReviewIntern { get; set; }
    }
}
