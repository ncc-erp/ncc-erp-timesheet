using System;
using System.Collections.Generic;
using System.Text;
using Ncc.Entities.Enum;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.APIs.Public.Dto
{
    public class GetWorkingTimeDto
    {
        public string UserEmail { get; set; }
        public string MorningStartTime { get; set; }
        public string MorningEndTime { get; set; }
        public string AfternoonStartTime { get; set; }
        public string AfternoonEndTime { get; set; }
        public DateTime DateAt { get; set; }
        public double Hour { get; set; }
        public RequestStatus Status { get; set; }
        public long RequestId { get; set; }
        public DayType DayType { get; set; }
        public OnDayType? AbsenceTime { get; set; }
    }
}
