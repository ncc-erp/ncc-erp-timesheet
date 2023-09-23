using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Ncc.Authorization.Users;
using System;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Ncc.Sessions.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserLoginInfoDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public Double? AllowedLeaveDay { get; set; }
        public Usertype? Type { get; set; }
        public UserLevel? Level { get; set; }
        public Sex? Sex { get; set; }
        public Branch? Branch { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public string MorningWorking { get; set; }
        public string MorningStartAt { get; set; }
        public string MorningEndAt { get; set; }
        public string AfternoonWorking { get; set; }
        public string AfternoonStartAt { get; set; }
        public string AfternoonEndAt { get; set; }
        public bool? isWorkingTimeDefault { get; set; }
        public long? BranchId { get; set; }
    }
}
