namespace Timesheet.Configuration.Dto
{
    public class LateInternReviewSettingDto
    {
        public bool enable { get; set; }
        public int deadlineDay { get; set; }
        public int daysToAdd { get; set; }
        public int startDayOfMonth { get; set; }
        public int nextRunDate { get; set; }
    }
}
