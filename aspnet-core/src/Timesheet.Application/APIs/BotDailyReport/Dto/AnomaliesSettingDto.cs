using System;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.BotDailyReport.Dto
{
    public class GetAnomalyInputDto
    {
        public long BranchId { get; set; }
        public TimeRangeOption TimeRange { get; set; }
    }

    public class AnomaliesSettingDto
    {
        public bool? YesterdayEnable { get; set; }
        public int? YesterdayAtHour { get; set; }
        public bool? LastWeekEnable { get; set; }
        public int? LastWeekAtHour { get; set; }
        public string LastWeekAtDayOfWeek { get; set; }
        public string WebhookUrl { get; set; }
        public string BranchName { get; set; }
    }
}