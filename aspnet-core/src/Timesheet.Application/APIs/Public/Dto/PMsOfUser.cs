using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Public.Dto
{
    public class PMsOfUser
    {
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public List<UserInfo> PMs { get; set; }
    }
}
