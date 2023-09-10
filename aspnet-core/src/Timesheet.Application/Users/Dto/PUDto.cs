using Ncc.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Users.Dto
{
    public class PUDto
    {
        public long ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public ProjectUserType ProjectUserType { get; set; }
        public List<string> Pms { get; set; }
    }
}
