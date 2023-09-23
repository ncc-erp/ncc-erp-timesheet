using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class GetSecurityCodeDto
    {
        public string SecretCode { get; set; }
        public string SecurityCodeHeader { get; set; }
    }
}
