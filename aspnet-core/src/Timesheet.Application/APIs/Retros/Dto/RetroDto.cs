using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Anotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Retros.Dto
{
    public class RetroDto : Entity<long>
    {
        [ApplySearch]
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Deadline { get; set; }
        public RetroStatus Status { get; set; }
    }
}