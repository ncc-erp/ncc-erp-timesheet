using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Entities;
using System;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagement.Dto
{
    [AutoMapTo(typeof(Project))]
    public class SpecialProjectDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime? TimeEnd { get; set; }
        public string CustomerCode { get; set; }
        public int ProjectType { get; set; }
        public string EmailPM { get; set; }

        public int Status { get; set; }
    }
}
