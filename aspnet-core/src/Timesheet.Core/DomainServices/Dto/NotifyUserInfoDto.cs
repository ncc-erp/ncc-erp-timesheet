using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class NotifyUserInfoDto
    {
        public Branch? Branch { get; set; }
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public ulong? KomuUserId { get; set; }
        public string FullName { get; set; }
        public Usertype? Type { get; set; }
        public UserLevel? Level { get; set; }
        public string BranchDisplayName { get; set; }
        public double MorningWorking { get; set; }
        public double AfternoonWorking { get; set; }

        public string KomuAccountTag()
        {
            return KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"**{FullName}**";
        }

        public string KomuAccountNoTag()
        {
            return $"**{FullName}** [{CommonUtils.BranchName(Branch)} - {CommonUtils.UserTypeName(Type)}]";
        }

        public string KomuAccountInfo
        {
            get
            {
                var user = KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"**{FullName}**";
                return $"{user} [{ BranchDisplayName } - { CommonUtils.UserTypeName(Type)}]";
            }
        }
        public string ToEmailString()
        {
            return $"{FullName} [{CommonUtils.BranchName(Branch)} - {CommonUtils.UserTypeName(Type)}]";
        }
    }
}
