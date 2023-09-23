using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class SendMessageRequestPendingTeamBuildingToHRDto
    {
        public ulong? KomuUserId { get; set; }
        public string EmailAddress { get; set; }
        public List<RequestTeamBuildingDto> Requests { get; set; }
        public string KomuAccountTag()
        {
            return KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"**{EmailAddress}**";
        }
    }

    public class RequestTeamBuildingDto
    {
        public string Title { get; set; }
        public string RequesterEmail { get; set; }
        public string Note { get; set; }
    }
}
