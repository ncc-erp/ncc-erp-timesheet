using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.HRMv2.Dto
{
    public class InputComplainPayslipMail: InputConfirmPayslipMail
    {
        public string ComplainNote { get; set; }
    }
}
