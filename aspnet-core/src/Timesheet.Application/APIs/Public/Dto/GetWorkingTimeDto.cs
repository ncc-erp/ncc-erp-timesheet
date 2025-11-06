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
    }
}
