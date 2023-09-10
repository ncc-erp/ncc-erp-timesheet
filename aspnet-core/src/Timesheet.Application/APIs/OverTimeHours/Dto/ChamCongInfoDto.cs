using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.OverTimeHours.Dto
{
    public class ChamCongInfoDto
    {
        public string NormalizeEmailAddress { get; set; }
        public List<DateTime> OpenTalkDates { get; set; }
        public List<DateTime> NormalWorkingDates { get; set; }
        [JsonIgnore]
        public DateTime? StartWorkingDate { get; set; }
        [JsonIgnore]
        public DateTime? StopWorkingDate { get; set; }
    }
}
