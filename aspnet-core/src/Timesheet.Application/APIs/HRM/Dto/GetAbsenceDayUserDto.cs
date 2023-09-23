using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class GetAbsenceDayUserDto
    {
        public long UserId { get; set; }
        public string NormalizedEmailAddress { get; set; }
        public List<AbsenceDayUserDetailDto> absenceDayDetails { get; set; }
    }
    public class AbsenceDayUserDetailDto :Entity<long>
    {
        public DateTime DateAt { get; set; }
        //public double Hour { get; set; }
        public OffTypeStatus Status { get; set; }
        public DayType DayType { get; set; }
        public string DateTypeName => Enum.GetName(typeof(DayType), DayType);
        public double Hour => DayType == DayType.Fullday ? 8 : 4;
    }

}
