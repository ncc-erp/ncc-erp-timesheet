using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.ReviewInternCapabilities.Dto;
namespace Timesheet.APIs.ReviewDetails.Dto
{
    public class NewPMReviewDetailDto : PMReviewDetailDto
    {
        public List<ReviewInterCapabilityDto> reviewInternCapabilities { get; set; }
    }
}