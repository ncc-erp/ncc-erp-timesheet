using Abp.Application.Services.Dto;
using System;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timekeepings.Dto
{
    public class UserComplaintDto : EntityDto<long>
    {
        public string UserNote { get; set; }
        public UserPunishmentType PunishmentType { get; set; }
        public DateTime Date { get; set; }
    }

    public class SubmitUserComplaintDto
    {
        public long UserPunishmentId { get; set; }
        public string UserNote { get; set; }
    }

    public class UserComplaintResultDto
    {
        public long UserPunishmentId { get; set; }
        public long? TimekeepingId { get; set; }
        public string UserNote { get; set; }
        public UserPunishmentType PunishmentType { get; set; }
        public string PunishmentTypeName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}