using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.HRMv2.Dto
{
    public class InputCollectDataForPayslipDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<string> UpperEmails { get; set; }
    }
}
