using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.ProjectManagement.Dto
{
    public class InputRetroReviewInternHistoriesDto
    {
        public List<string> Emails { get; set; }
        public int MaxCountHistory { get; set; } = 12;
    }
}
