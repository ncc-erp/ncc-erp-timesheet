using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class NotificationSettingDto
    {
        public string SendEmailTimesheet { get; set; }
        public string SendEmailRequest { get; set; }

        public string SendKomuSubmitTimesheet { get; set; }
        public string SendKomuRequest { get; set; }
    }
}
