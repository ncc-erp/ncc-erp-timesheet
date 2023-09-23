using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.ReviewInternCapabilities.Dto;
using Timesheet.Entities;
namespace Timesheet.APIs.ReviewDetails.Dto
{
    public class NewReviewDetailDto : ReviewDetailDto
    {
        public float? Average { get; set; }
        public List<ReviewInterCapabilityDto> ReviewInternCapabilities { get; set; }
        public bool IsUpOfficial { get; set; }
    }
}