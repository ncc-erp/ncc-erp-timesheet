using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Users.Dto
{
    public class UserManagerDto
    {
        public long ManagerId { get; set; }
        public string AvatarPath { get; set; }
        public string ManagerName { get; set; }
    }
}
