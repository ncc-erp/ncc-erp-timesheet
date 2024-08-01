using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.External.Dto
{
    public class ListUserDto
    {
        public string FullName { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }
}
