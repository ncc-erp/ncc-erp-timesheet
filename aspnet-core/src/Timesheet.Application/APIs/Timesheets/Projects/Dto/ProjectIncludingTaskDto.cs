using Abp.Application.Services.Dto;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.Projects.Dto
{
    public class ProjectIncludingTaskDto : EntityDto<long>
    {
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public string ProjectCode { get; set; }
        public ProjectUserType ProjectUserType { get; set; }
        public List<string> ListPM { get; set; }
        public List<PTaskDto> Tasks { get; set; }
        public List<PTargetUserDto> TargetUsers { get; set; }
    }
    public class PTaskDto
    {
        public long ProjectTaskId { get; set; }
        public string TaskName { get; set; }
        public bool Billable { get; set; }
        public bool IsDefault { get; set; }
    }

    public class PTargetUserDto
    {
        public long ProjectTargetUserId { get; set; }
        public string UserName { get; set; }
    }
}
