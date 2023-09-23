using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class MapAbsenceUserDto
    {
        public long UserId { get; set; }
        public DayType DateType { get; set; }//morning, afternoon, fullday, custom
        public OnDayType? AbsenceTime { get; set; }//dau. giua, cuoi
        public double Hour { get; set; }
    }
}
