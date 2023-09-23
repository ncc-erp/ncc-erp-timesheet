using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class NotifyRetroDto
    {
        public ulong? KomuUserId { get; set; }
        public string EmailAddress { get; set; }
        public List<NotifyProjectRetroInfoDto> Projects { get; set; }
        public string KomuAccountTag()
        {
            return KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"**{EmailAddress}**";
        }
    }
}
