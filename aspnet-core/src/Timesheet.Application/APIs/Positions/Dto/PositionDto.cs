using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;

namespace Timesheet.APIs.Positions.Dto
{
    public class PositionDto : Entity<long>
    {
        [ApplySearchAttribute]
        public string Name { get; set; }
        [ApplySearchAttribute]
        public string ShortName { get; set; }
        public string Color { get; set; }
        [ApplySearchAttribute]
        public string Code { get; set; }
    }
}