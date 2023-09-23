using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.Timekeepings.Dto
{
    [AutoMapTo(typeof(Timekeeping))]
    public class TimekeepingUserNoteDto: EntityDto<long>
    {
        public string UserNote { get; set; }
    }
}
