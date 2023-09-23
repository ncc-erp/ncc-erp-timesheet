using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Timesheets.GetDataFromFaceIDSetting.Dto
{
    public class GetDataFromFaceIDDto
    {
        public string GetDataAt { get; set; }
        public string AccountID { get; set; }
        public string SecretCode { get; set; }
        public string Uri { get; set; }
    }
}
