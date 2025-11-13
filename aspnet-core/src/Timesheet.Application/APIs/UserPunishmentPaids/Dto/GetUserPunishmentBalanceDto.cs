namespace Timesheet.APIs.UserPunishmentPaids.Dto
{
    public class GetUserPunishmentBalanceDto
    {
        public int TotalPunishmentMoney { get; set; }
        public int RemainPoints { get; set; }
        public int EffectiveAmount { get; set; } 
        public bool HasBalance { get; set; } 
    }
}
