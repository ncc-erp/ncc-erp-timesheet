using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.HRMv2.Dto
{
    public class InputConfirmPayslipMail
    {
        public string Email { get; set; }
        public long PayslipId { get; set; }
    }
}
