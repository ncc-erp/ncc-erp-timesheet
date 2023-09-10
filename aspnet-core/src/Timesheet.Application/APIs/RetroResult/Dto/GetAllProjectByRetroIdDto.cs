using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.RetroDetails.Dto
{
    public class GetAllProjectByRetroIdDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
