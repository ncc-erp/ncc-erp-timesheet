using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class ProjectUserInfoDto
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public ulong? KomuUserId { get; set; }
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
    }
}
