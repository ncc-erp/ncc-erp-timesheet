using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.FilesService.Dto
{
    internal class InputFileDto
    {
        public IFormFile File { get; set; }
    }
}
