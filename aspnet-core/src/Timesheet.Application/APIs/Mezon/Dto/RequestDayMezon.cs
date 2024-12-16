using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Mezon.Dto
{
    public class RequestDayMezon
    {
        public long[] RequestIds { get; set; }
        public string Email { get; set; }
    }
}
