using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using Timesheet.Entities;
using Ncc.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.APIs.OverTimeSettings.Dto
{
    [AutoMap(typeof(OverTimeSetting))]
    public class OverTimeSettingDto : Entity<long>
    {
        [ForeignKey(nameof(ProjectId))]
        public long ProjectId { get; set; }
        public Project Project { get; set; }
        [Required]
        public DateTime DateAt { get; set; }
        public string Note { get; set; }
        [Required]
        public double Coefficient { get; set; }
    }
}
