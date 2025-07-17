using System;
using System.ComponentModel.DataAnnotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.UserPunishments.Dto
{
    public class CreateUserPunishmentDto
    {
        [Required]
        public DateTime DateAt { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public UserPunishmentType Type { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Count must be greater than 0")]
        public int Count { get; set; }

        [MaxLength(500)]
        public string NoteReply { get; set; }
    }
}