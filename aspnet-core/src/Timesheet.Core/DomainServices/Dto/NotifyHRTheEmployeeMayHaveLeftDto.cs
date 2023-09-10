using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class NotifyHRTheEmployeeMayHaveLeftDto
    {
        public List<HRInfoTheEmployeeMayHaveLeftDto> HRInfos { get; set; }
        public List<ProjectInfoNotifyHRTheEmployeeMayHaveLeftDto> ProjectInfos { get; set; }
    }

    public class ProjectInfoNotifyHRTheEmployeeMayHaveLeftDto
    {
        public List<PMInfoNotifyHRTheEmployeeMayHaveLeftDto> PMs { get; set; }
        public List<EmployeeInfoNotifyHRTheEmployeeMayHaveLeftDto> Employees { get; set; }
        
    }

    public class HRInfoTheEmployeeMayHaveLeftDto
    {
        public ulong? HRKomuUserId { get; set; }
        public string HREmailAddress { get; set; }
        public string HRKomuAccountTag()
        {
            return HRKomuUserId.HasValue ? $"<@{HRKomuUserId}>" : $"**{HREmailAddress}**";
        }
    }

    public class PMInfoNotifyHRTheEmployeeMayHaveLeftDto
    {
        public ulong? PMKomuUserId { get; set; }
        public string PMEmailAddress { get; set; }
        public string PMKomuAccountTag()
        {
            return PMKomuUserId.HasValue ? $"<@{PMKomuUserId}>" : $"**{PMEmailAddress}**";
        }
    }

    public class EmployeeInfoNotifyHRTheEmployeeMayHaveLeftDto 
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
    }
}
