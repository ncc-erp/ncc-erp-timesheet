using System;
using System.ComponentModel.DataAnnotations;

namespace Timesheet.APIs.UserPunishments.Dto
{
    public class CreateUserPunishmentDto
    {
        [Required]
        public DateTime DateAt { get; set; }

        [Required]
        public long? UserId { get; set; }

        [Required]
        public long PunishmentSystemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Type { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Count must be greater than 0")]
        public int Count { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "TotalMoney must be greater than or equal to 0")]
        public int TotalMoney { get; set; }

        [MaxLength(500)]
        public string UserNote { get; set; }

        [MaxLength(500)]
        public string NoteReply { get; set; }
    }
}