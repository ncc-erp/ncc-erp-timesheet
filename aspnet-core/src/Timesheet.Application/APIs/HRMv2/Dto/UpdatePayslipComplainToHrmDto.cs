using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRMv2.Dto
{
    public class UpdatePayslipComplainToHrmDto
    {
        public string ComplainNote { get; set; }
        public long PayslipId { get; set; }
    }
}
