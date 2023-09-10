using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;
using System.Linq;
using Ncc.IoC;

namespace Timesheet.Timesheets.Projects.Dto
{
    [AutoMapTo(typeof(Project))]
    public class ProjectDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public ProjectStatus Status { get; set; }       
        public DateTime TimeStart { get; set; }
        public DateTime? TimeEnd { get; set; }
        public string Note { get; set; }
        public ProjectType ProjectType { get; set; }
        public long CustomerId { get; set; }
        public List<ProjectTaskDto> Tasks { get; set; }
        public List<ProjectUsersDto> Users { get; set; }
        public List<ProjectTargetUserDto> ProjectTargetUsers { get; set; }
        public string KomuChannelId { get; set; }
        public bool IsNotifyToKomu { get; set; }
        public bool IsNoticeKMSubmitTS { get; set; } = false;
        public bool IsNoticeKMRequestOffDate { get; set; } = false;
        public bool IsNoticeKMApproveRequestOffDate { get; set; } = false;
        public bool IsNoticeKMRequestChangeWorkingTime { get; set; } = false;
        public bool IsNoticeKMApproveChangeWorkingTime { get; set; } = false;
        public bool isAllUserBelongTo { get; set; }
    }
}
