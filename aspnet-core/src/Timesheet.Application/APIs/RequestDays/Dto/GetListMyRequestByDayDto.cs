using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.MyAbsenceDays.Dto;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class GetListMyRequestByDayDto
    {
        public DateTime DateAt { get; set; }
        public List<GetMyRequestDto> Requests { get; set; }
    }
}
