using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ReviewDetails.Dto
{
    public class DetailHistoriesDto
    {
        public int HistoryMonth { get; set; }
        public int HistoryYear { get; set; }
        public UserLevel? FromLevel { get; set; }
        public UserLevel? ToLevel { get; set; }
        public long? ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public long InternshipId { get; set; }
        public float? RateStar { get; set; }
        public float? Average { get; set; }
    }
}
