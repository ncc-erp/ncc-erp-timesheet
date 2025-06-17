using Abp.Application.Services.Dto;
using System;

namespace Timesheet.APIs.UserPunishments.Dto
{
    public class UpdateUserPunishmentDto : EntityDto<long>
    {
        public DateTime DateAt { get; set; }
        public long? UserId { get; set; }
        public long PunishmentSystemId { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
        public int TotalMoney { get; set; }
        public string UserNote { get; set; }
        public string NoteReply { get; set; }
    }
}