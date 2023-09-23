using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using Timesheet.Entities;
using Ncc.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using Timesheet.Anotations;

namespace Timesheet.APIs.Branchs.Dto
{
    [AutoMap(typeof(Branch))]
    public class BranchCreateEditDto : Entity<long>
    {
        [Required]
        [ApplySearchAttribute]
        public string Name { get; set; }
        [Required]
        [ApplySearchAttribute]
        public string DisplayName { get; set; }
        [Required]
        public double MorningWorking { get; set; }
        [Required]
        public string MorningStartAt { get; set; }
        [Required]
        public string MorningEndAt { get; set; }
        [Required]
        public double AfternoonWorking { get; set; }
        [Required]
        public string AfternoonStartAt { get; set; }
        [Required]
        public string AfternoonEndAt { get; set; }
        public string Color { get; set; }
        [ApplySearchAttribute]
        public string Code { get; set; }
    }
}
