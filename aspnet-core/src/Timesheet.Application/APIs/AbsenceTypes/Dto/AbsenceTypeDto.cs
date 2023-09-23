using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.DayOffs.Dto
{
    [AutoMapTo(typeof(DayOffType))]
    public class AbsenceTypeDto : Entity<long>
    {
        public OffTypeStatus Status { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
    }
}
