namespace Timesheet.APIs.TeamBuildingDetailsPM.Dto
{
    public class GetAllUserRequestMoneyDto
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public long? BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchColor { get; set; }
    }
}
