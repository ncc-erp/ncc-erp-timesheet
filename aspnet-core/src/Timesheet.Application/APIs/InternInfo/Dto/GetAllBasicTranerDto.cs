using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.InternInfo.Dto
{
    public class GetAllBasicTranerDto
    {
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public long Id { get; set; }
    }
}
