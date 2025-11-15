namespace Timesheet.DomainServices.Dto
{
    public class UserPunishmentSummaryDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public GetUserPunishmentBalanceDto UserBalance { get; set; }
        public int TotalRemainPointsUsedInMonth { get; set; }
        public int TotalPaidPunishmentInMonth { get; set; }
    }

    public class GetUserPunishmentBalanceDto
    {
        public int TotalPunishmentMoney { get; set; }
        public int RemainPoints { get; set; }
    }
}
