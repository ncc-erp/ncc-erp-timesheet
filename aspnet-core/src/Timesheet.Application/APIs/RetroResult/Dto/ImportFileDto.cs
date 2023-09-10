using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.RetroDetails.Dto
{
    public class ImportFileDto
    {
        public IFormFile File { get; set; }
        public long retroId { get; set; }
        public long projectId { get; set; }
        public long pmId { get; set; }
    }
}
