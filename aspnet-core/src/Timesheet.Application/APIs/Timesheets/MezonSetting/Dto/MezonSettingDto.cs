using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.MezonSetting.Dto
{
    public class MezonSettingDto
    {
        public bool enable {  get; set; }
        public int hour { get; set; }
        public string dayofweek { get; set; }
        public string secretCode { get; set; }
        public string uri { get; set; }

    }
}
