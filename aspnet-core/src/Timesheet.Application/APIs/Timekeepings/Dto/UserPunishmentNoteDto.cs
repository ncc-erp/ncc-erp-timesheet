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
    public class UserPunishmentNoteDto: EntityDto<long>
    {
        [Required]
        public UserPunishmentType Type { get; set; }

        [Required]
        public DateTime PunishmentDate { get; set; }

        [StringLength(1000)]
        public string UserNote { get; set; }
    }
}
