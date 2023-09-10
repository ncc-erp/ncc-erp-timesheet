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
    public class BranchDto : Entity<long>
    {
        [ApplySearchAttribute]
        [Required]
        public string Name { get; set; }
        [Required]
        public string DisplayName { get; set; }
    }
}
