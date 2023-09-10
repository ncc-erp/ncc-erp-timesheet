using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.APIs.ReviewInterns.Dto
{
    [AutoMapTo(typeof(ReviewIntern))]
    public class ReviewInternDto : EntityDto<long>
    {
        public int Month { get; set; }
        public int Year { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}
