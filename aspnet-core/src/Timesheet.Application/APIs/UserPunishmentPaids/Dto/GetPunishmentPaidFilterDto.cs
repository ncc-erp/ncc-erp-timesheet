using System;
using System.ComponentModel.DataAnnotations;

namespace Timesheet.APIs.UserPunishmentPaids.Dto
{
    public class GetPunishmentPaidFilterDto
    {
        [Required]
        public int Year { get; set; }
        
        [Required]
        public int Month { get; set; }

        public DateTime GetStartDate()
        {
            return new DateTime(Year, Month, 1);
        }

        public DateTime GetEndDate()
        {
            return GetStartDate().AddMonths(1);
        }
    }
}
