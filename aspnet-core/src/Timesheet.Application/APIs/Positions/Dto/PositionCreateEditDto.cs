using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Entities;

namespace Timesheet.APIs.Positions.Dto
{
    [AutoMapTo(typeof(Position))]
    public class PositionCreateEditDto : Entity<long>
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        public string Color { get; set; }

        public string Code { get; set; }
    }
}