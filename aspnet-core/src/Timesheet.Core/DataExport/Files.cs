using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DataExport
{
    public class FileBase64Dto
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Base64 { get; set; }
    }
}