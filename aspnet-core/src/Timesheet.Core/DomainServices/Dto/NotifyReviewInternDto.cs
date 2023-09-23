using System;
using System.Collections.Generic;
using System.Linq;
using Timesheet.DomainServices.Dto;
using Microsoft.EntityFrameworkCore.Internal;

namespace Timesheet.DomainServices.Dto
{
    public class NotifyReviewInternDto
    {
        public ulong? KomuUserId { get; set; }
        public string EmailAddress { get; set; }

        public List<NotifyUserInfoDto> InterShips { get; set; }

        public string KomuAccountTag()
        {
            return KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"**{EmailAddress}**";
        }
    }
}
