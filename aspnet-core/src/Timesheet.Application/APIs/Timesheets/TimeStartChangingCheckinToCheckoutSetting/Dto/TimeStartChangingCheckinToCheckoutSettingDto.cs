using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.TimeStartChangingCheckinToCheckoutSetting.Dto
{
    public class TimeStartChangingCheckinToCheckoutSettingDto
    {
        public string  TimeStartCheckOut { get; set; }
        public string TimeStartCheckOutCaseOffAfternoon { get; set; }
        public string EnableTimeStartChangingToCheckout { get; set; }
    }
}
