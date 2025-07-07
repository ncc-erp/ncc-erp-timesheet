using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timekeepings.Dto
{
    [AutoMapTo(typeof(UserPunishment))]
    public class RespondToComplaintDto
    {
        public long UserpunishmentId { get; set; }
        public string NoteReply { get; set; }
        public UserPunishmentType StatusPunish { get; set; }
        public int? ChangeCount { get; set; } 
    }
    public class RespondToComplaintResultDto
    {
        public bool Success { get; set; }
        public long PunishmentId { get; set; }
        public string PunishmentType { get; set; }
        public int PunishmentMoney { get; set; }
        public int? RemainingCount { get; set; } 
    }
}
