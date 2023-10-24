using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class InputGenerateDataRetroResultDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public long RetroId { get; set; }
    }
}
