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
    [AutoMapTo(typeof(Timekeeping))]
    public class TimekeepingDto: EntityDto<long>
    {
        public bool IsPunishedCheckIn { get; set; }       

        public string NoteReply { get; set; }
        public int MoneyPunish { get; set; }
        public CheckInCheckOutPunishmentType StatusPunish { get; set; }
    }
}
