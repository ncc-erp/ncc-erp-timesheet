using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class ProjectByCurrentUserDto
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
}
