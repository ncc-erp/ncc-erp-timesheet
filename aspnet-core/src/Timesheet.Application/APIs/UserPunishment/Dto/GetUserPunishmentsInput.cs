using Abp.Application.Services.Dto;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.UserPunishments.Dto
{
    public class GetUserPunishmentsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; } // Tìm kiếm theo Type, UserNote, NoteReply
        public long? UserId { get; set; }
        public long? PunishmentSystemId { get; set; }
        public UserPunishmentType Type { get; set; }
        public bool? IsActive { get; set; } // Dựa trên IsDeleted = false
    }
}