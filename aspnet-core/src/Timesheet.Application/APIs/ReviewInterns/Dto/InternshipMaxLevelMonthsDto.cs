using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ReviewInterns.Dto
{
    public class InternshipMaxLevelMonthsDto
    {
        public long internshipId { get; set; }
        public UserLevel? maxLevel { get; set; }
        public long countMonthLevelMax {  get; set; }
    }
}
