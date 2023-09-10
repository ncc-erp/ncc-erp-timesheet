namespace Timesheet.APIs.TeamBuildingDetails.Dto
{
    public class GetAllRequesterEmailAddressInTeamBuildingDetailDto
    {
        public long RequesterId { get; set; }
        public string RequesterEmailAddress { get; set; }
    }

    public class GetAllProjectInTeamBuildingDetailDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}
