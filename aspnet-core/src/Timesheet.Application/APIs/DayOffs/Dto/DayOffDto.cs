using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.DayOffs.Dto
{
    [AutoMap(typeof(DayOffSetting))]
    public class DayOffDto : Entity<long>
    {
        [Required]
        public DateTime DayOff { get; set; }
        [ApplySearchAttribute]
        public string Name { get; set; }
        public double Coefficient { get; set; }
        public Ncc.Entities.Enum.StatusEnum.Branch? Branch { get; set; }

    }
}
