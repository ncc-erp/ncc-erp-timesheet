using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Timesheets.EmailSettings.Dto
{
    public class MailSettingDto
    {
        public string FromDisplayName { get; set; }
        public string UserName { get; set; }
        public string Port { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public string EnableSsl  { get; set; }
        public string DefaultFromAddress { get; set; }
        public string UseDefaultCredentials { get; set; }
    }
}
