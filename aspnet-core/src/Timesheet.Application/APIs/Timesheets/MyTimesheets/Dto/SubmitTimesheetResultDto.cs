namespace Timesheet.APIs.Timesheets.MyTimesheets.Dto
{
    public class SubmitTimesheetResultDto
    {
        public bool Success { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
