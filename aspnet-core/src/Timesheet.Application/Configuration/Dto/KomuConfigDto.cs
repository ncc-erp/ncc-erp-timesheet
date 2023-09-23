using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class KomuConfigDto
    {
        public string KomuUri { get; set; }
        public string KomuSecretCode { get; set; }
        public string KomuChannelIdDevMode { get; set; }
        public string KomuUserNameDevMode { get; set; }
    }
}
