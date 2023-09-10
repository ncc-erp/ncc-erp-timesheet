using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.HRMv2.Dto
{
    public class GetEmployeeRequestDayDto
    {
        public string NormalizedEmailAddress { get; set; }
        public List<OffDateDto> OffDates { get; set; }
                
        /// <summary>
        /// list date empoloyee chi lam viec o nha -> ko duoc huong che do remote (ho tro tien xe oto, ho tro nha xa, ..)
        /// </summary>
        public HashSet<DateTime> WorkAtHomeOnlyDates { get; set; }

        /// <summary>
        /// danh sach cac ngay nghi co phep thang truoc
        /// </summary>
        public List<OffDateDto> OffDateLastMonth { get; set; }

    }
    public class OffDateDto
    {
        public DateTime DateAt { get; set; }
        public double DayValue { get; set; }
        public long DayOffTypeId { get; set; }
        public int LeaveDay { get; set; }
    }

}
