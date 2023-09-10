using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class CheckInOutTimeDto
    {
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string Note { get; set; }
        public DayType AbsenceDayType { get; set; }
    }
}
