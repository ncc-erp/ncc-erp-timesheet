namespace Timesheet.DomainServices.Dto
{
    public class PreviewAndApplyPunishmentPointsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalHashAmount { get; set; }
        public int TotalPunishmentMoney { get; set; }
        public int RemainPoints { get; set; }
        public int EffectivePunishmentAmount { get; set; } 
        public int PunishmentsMarkedAsPaid { get; set; }
        public bool HasSufficientFunds { get; set; }
    }
}
