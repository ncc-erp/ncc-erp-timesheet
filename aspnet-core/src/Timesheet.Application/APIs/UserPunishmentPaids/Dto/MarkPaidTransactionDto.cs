using System.ComponentModel.DataAnnotations;

namespace Timesheet.APIs.UserPunishmentPaids.Dto
{
    public class MarkPaidTransactionDto
    {
        [Required]
        [MaxLength(255)]
        public string TransactionHash { get; set; }
        
        [Required]
        [Range(2000, 5500)]
        public int Year { get; set; }
        
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }
    }

    public class MarkPaidTransactionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
