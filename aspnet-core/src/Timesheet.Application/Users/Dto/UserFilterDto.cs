using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Users.Dto
{
    public class UserFilterDto
    {
        public long Id { get; set; }
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
    }
}
