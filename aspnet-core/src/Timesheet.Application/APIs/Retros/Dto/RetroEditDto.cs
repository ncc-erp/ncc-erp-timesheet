using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Retros.Dto
{
    [AutoMapTo(typeof(Entities.Retro))]
    public class RetroEditDto : Entity<long>
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Deadline { get; set; }
    }
}
