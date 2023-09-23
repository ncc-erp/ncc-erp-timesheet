using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Anotations;

namespace Timesheet.APIs.RetroDetails.Dto
{
    public class GetAllPmInRetroResultDto
    {
        public long PmId { get; set; }
        public string PmFullName { get; set; }
        public string PmEmailAddress { get; set; }
        public bool IsDefault { get; set; }
    }
}
