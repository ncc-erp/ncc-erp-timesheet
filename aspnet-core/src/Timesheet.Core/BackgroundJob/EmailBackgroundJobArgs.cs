using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Timesheet.BackgroundJob
{
    public class EmailBackgroundJobArgs
    {
        public List<string> TargetEmails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
