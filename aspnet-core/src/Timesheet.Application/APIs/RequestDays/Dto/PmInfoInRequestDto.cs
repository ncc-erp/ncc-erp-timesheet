using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class PmInfoInRequestDto
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public ulong? KomuUserId { get; set; }
    }
}
