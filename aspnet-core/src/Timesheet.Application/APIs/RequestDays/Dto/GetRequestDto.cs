using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class GetRequestDto : Entity<long>
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Branch { get; set; }
        public Usertype? Type { get; set; } // Staff, Intern, CTV
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        //public UserLevel? Level { get; set; }
        public Sex? Sex { get; set; }

        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public double Hour { get; set; }

        public string DayOffName { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
        public RequestType LeavedayType { get; set; }
        public string BranchDisplayName { get; set; }
        public string BranchColor { get; set; }
        public OnDayType? AbsenceTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string LastModifierUserName { get; set; }
        public List<ProjectInfoDto> ProjectInfos { get; set; }
        public TimesheetStatus TimesheetStatus { get; set; }
    }

    public class ProjectInfoDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public List<PmInfoDto> Pms { get; set; }
    }

    public class PmInfoDto
    {
        public long PmId { get; set; }
        public string PmFullName { get; set; }
        public string PmEmailAddress { get; set; }
    }
}
