using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.InternInfo.Dto
{
    public class ReviewDetailFilterDto
    {
        public UserLevel? NextLevel { get; set; }
        public long UserId { get; set; }
        public ReviewInternStatus Status { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public float? RateStar { get; set; }
        public string ReviewerName { get; set; }
        public DateTime GetDateAt => new DateTime(Year, Month, 1);
        public string Note { get; set; }
    }
}
