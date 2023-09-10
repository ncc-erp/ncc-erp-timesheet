using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ReviewDetails.Dto
{
    [AutoMapTo(typeof(ReviewDetail))]
    public class PMReviewDetailDto : EntityDto<long>
    {
        public UserLevel? NewLevel { get; set; }
        public string Note { get; set; }
        public Usertype Type { get; set; }
        public bool? IsFullSalary { get; set; }
        public byte? SubLevel { get; set; }
        public int? RateStar { get; set; }
        public int? Salary { get; set; }

    }
}
