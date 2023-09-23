using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Public.Dto
{
    public class UserInfoToProject
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public long? Branch { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
        public string AvatarPath { get; set; }
        public Usertype? UserType { get; set; }
        public UserLevel? UserLevel { get; set; }
        public string FullAvatarPath => FileUtils.FullFilePath(AvatarPath);
        public string UserTypeName => CommonUtils.UserTypeName(UserType);
    }
}
