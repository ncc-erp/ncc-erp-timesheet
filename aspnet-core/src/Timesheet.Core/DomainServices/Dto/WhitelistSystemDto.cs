using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class GetWhitelistSystemDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public WhitelistType Type { get; set; }
        public string TypeName { get; set; }
        public bool IsActive { get; set; }
    }

    public class AddWhitelistTypeDto
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
        [Required]
        [MaxLength(256)]
        [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Code must be uppercase letters, numbers, and underscores only.")]
        public string Code { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [Required]
        public WhitelistType Type { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWhitelistTypeDto : AddWhitelistTypeDto
    {
        [Range(1, long.MaxValue, ErrorMessage = "Id is required")]
        public long Id { get; set; }
    }

    public class WhitelistTypeDto
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }
}
