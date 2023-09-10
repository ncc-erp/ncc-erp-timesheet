using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Public.Dto
{
    public class UserInfo
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string BranchName { get; set; }
        public string AvatarPath { get; set; }
        public Usertype? UserType { get; set; }
        public string FullAvatarPath => FileUtils.FullFilePath(AvatarPath);
        public string UserTypeName => CommonUtils.UserTypeName(UserType);
    }
}
