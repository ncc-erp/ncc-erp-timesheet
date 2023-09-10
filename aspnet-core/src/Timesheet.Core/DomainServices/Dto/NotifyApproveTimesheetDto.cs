using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class UserInfoApproveTimesheetDto
    {
        public string FullName { get; set; }
        public DateTime DateAt { get; set; }
    }

    public class NotifyApproveTimesheetDto
    {
        public ulong? KomuUserId { get; set; }
        public string EmailAddress { get; set; }

        public List<UserInfoApproveTimesheetDto> Users { get; set; }
        public int CountUsers => Users.Count;

        public string KomuAccountTag()
        {
            return KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"**{EmailAddress}**";
        }
    }
}
