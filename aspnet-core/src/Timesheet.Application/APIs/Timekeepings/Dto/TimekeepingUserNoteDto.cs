using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timekeepings.Dto
{
    [AutoMapTo(typeof(Timekeeping))]
    public class TimekeepingUserNoteDto: EntityDto<long>
    {
        public string UserNote { get; set; }
        public UserPunishmentType PunishmentType { get; set; }
    }
}
