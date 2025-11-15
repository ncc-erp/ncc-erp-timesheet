using System.ComponentModel.DataAnnotations;

namespace Timesheet.APIs.UserPunishmentPaids.Dto
{
    public class PreviewAndApplyPunishmentPointsDto
    {
        [Required]
        [Range(2000, 5500)]
        public int Year { get; set; }
        
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }
    }

    public class PreviewAndApplyPunishmentPointsResultDto
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
