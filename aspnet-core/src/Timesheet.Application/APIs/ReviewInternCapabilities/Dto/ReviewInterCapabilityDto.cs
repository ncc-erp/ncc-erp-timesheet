
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;
namespace Timesheet.APIs.ReviewInternCapabilities.Dto
{
    [AutoMapTo(typeof(ReviewInternCapability))]
    public class ReviewInterCapabilityDto : EntityDto<long>
    {
        public long ReviewDetailId { get; set; }
        public string CapabilityName { get; set; }
        public CapabilityType CapabilityType { get; set; }
        public string Note { get; set; }
        public float Point { get; set; }
        public string GuideLine { get; set; }
        public float Confficent { get; set; }
    }
}