using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.External.Dto
{
    public class ListUserDto
    {
        public string fullName { get; set; }
        public string email { get; set; }
        public string google_id { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }
}
