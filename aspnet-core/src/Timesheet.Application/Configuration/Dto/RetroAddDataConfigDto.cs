using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Configuration.Dto
{
    public class CreateNewRetroConfigDto
    {
        public string CreateNewRetroEnableWorker { get; set; }
        public string CreateNewRetroAtHour { get; set; }
        public string CreateNewRetroOnDate { get; set; }

    }
    public class GenerateRetroResultConfigDto
    {
        public string GenerateRetroResultEnableWorker { get; set; }
        public string GenerateRetroResultAtHour { get; set; }
        public string GenerateRetroResultOnDate { get; set; }

    }
}
