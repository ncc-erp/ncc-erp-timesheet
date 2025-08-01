using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.DomainServices.Dto
{
    [AutoMapTo(typeof(ReviewIntern))]
    public class ReviewInternsDto : EntityDto<long>
    {
        public int Month { get; set; }
        public int Year { get; set; }
        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }

    public class LateReviewPunishmentResultDto
    {
        public int TotalPunishedPMs { get; set; }
        public int TotalPunishmentAmount { get; set; }
        public List<PunishedPMDto> PunishedPMs { get; set; }
    }

    public class PunishedPMDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int TotalUnreviewedInterns { get; set; }
        public int PunishmentAmount { get; set; }
    }
}
