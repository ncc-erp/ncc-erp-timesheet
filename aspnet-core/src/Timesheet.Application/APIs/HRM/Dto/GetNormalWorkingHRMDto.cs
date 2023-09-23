using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.NormalWorkingHours.Dto
{
    public class GetNormalWorkingHRMDto
    {
        public long UserId { get; set; }
        public string NormalizedEmailAddress { get; set; }    
        public IEnumerable<UserNormalWorkingDto> ListWorkingHour { get; set; }

        public double TotalWorkingHour => TotalWorkingHourOfMonth + TotalOpenTalk * 4;
        public double TotalWorkingHourOfMonth => ListWorkingHour == null ? 0 : ListWorkingHour.Where(s => !s.IsOpenTalk).Sum(s => s.WorkingHour);
        public double TotalOpenTalk => ListWorkingHour == null ? 0 : ListWorkingHour.Where(s => s.IsOpenTalk).Take(2).Count();
    }
    public class UserNormalWorkingDto
    {
        public int Day { get; set; }
        public string DayName { get; set; }
        public double WorkingHour { get; set; }
        public bool IsOpenTalk { get; set; }
    }
}
