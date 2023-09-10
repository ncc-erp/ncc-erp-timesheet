using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Entities;
using System;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ProjectManagement.Dto
{
    [AutoMapTo(typeof(Project))]
    public class UserJoinProjectDto : EntityDto<long>
    {
        public string ProjectCode { get; set; }
        public string EmailAddress { get; set; }
        public bool IsPool { get; set; }
        public int Role { get; set; }
        public DateTime StartDate { get; set; }
        public string PMEmail { get; set; }

    }
}
