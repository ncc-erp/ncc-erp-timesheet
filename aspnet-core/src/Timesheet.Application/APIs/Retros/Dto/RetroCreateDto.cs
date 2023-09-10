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
    [AutoMapTo(typeof(Entities.Retro))]
    public class RetroCreateDto : Entity<long>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public DateTime Deadline { get; set; }
        [Required]
        public RetroStatus Status { get; set; }
    }
}