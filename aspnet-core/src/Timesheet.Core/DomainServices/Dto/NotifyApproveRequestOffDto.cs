using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class UserInfoApproveRequestOffDto
    {
        public string FullName { get; set; }
        public List<RequestOffDto> RequestOffDtos { get; set; }
    }
    public class RequestOffDto
    {
        public string UserName { get; set; }
        public DateTime DateAt { get; set; }
        public RequestType RequestType { get; set; }
        public DayType DayType { get; set; }
    }
    public class NotifyApproveRequestOffDto
    {
        public ulong? KomuUserId { get; set; }
        public string EmailAddress { get; set; }

        public List<UserInfoApproveRequestOffDto> Users { get; set; }
        public int CountUsers => Users.Count;

        public string KomuAccountTag()
        {
            return KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"**{EmailAddress}**";
        }
    }

    public class PmInfoRequestOffDto
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public ulong? KomuUserId { get; set; }
    }

    public class RequestAddDto
    {
        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public string FullName { get; set; }
        public RequestType RequestType { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
    }

    public class SendMessageToUserRequestOffDto
    {
        public string UserName { get; set; }
        public List<RequestOffDto> RequestOffDtos { get; set; }
    }
}
