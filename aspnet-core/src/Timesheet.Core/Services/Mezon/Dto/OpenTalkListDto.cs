using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.Mezon.Dto
{
    public class OpenTalkListDto
    {
        public string fullName { get; set; }
        public string googleId { get; set; }
        public DateTime date { get; set; }
        public int totalTime { get; set; }
        public string mezonId { get; set; }
    }

    public class OpenTalkListNewDto
    {
        public string user_id { get; set; }
        public string email { get; set; }
        public int totalTime { get; set; }
        public DateTime date { get; set; }
    }
}
