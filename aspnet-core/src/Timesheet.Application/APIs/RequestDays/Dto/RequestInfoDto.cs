using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class RequestInfoDto
    {
        public DateTime Date { get; set; } 
        public RequestType Type { get; set; } 
        public OnDayType? AbsenceTime { get; set; }
        public DayType DateType { get; set; } 
        public double Hour { get; set; } 
        public long RequestId { get; set; } 
        public RequestStatus Status { get; set; } 
        public long Id { get; set; } 
    }
}
