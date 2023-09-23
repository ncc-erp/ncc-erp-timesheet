using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class ConfigCheckinDto
    {
        public string CheckInInternalUrl { get; set; }
        public string CheckInInternalAccount { get; set; }
        public string CheckInInternalXSecretKey { get; set; }
    }
}
