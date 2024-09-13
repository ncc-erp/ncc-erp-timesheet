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
        public string UserName => EmailAddress.Split('@')[0];

        public string KomuAccountTag(NotifyChannel notifyChannel)
        {
            switch (notifyChannel)
            {
                case NotifyChannel.KOMU:
                    return KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"{{{UserName}}}";
                case NotifyChannel.Mezon:
                    return $"@{UserName}";
            }
            return default;
        }

        public string KomuAccountNoTag()
        {
            return $"**{FullName}** [{CommonUtils.BranchName(Branch)} - {CommonUtils.UserTypeName(Type)}]";
        }

        public string KomuAccountInfo(NotifyChannel notifyChannel)
        {
            var user=FullName;
            switch (notifyChannel)
            {
                case NotifyChannel.KOMU:
                    user = KomuUserId.HasValue ? $"<@{KomuUserId}>" : $"{{{UserName}}}";
                    break;
                case NotifyChannel.Mezon:
                    user = $"@{UserName}";
                    break;
            }
            return $"{user} [{ BranchDisplayName } - { CommonUtils.UserTypeName(Type)}]";
        }

        public string ToEmailString()
        {
            return $"{FullName} [{CommonUtils.BranchName(Branch)} - {CommonUtils.UserTypeName(Type)}]";
        }
    }
}
