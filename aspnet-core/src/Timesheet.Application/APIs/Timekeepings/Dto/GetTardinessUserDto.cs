using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timekeepings.Dto
{
    public class GetTardinessUserDto
    {
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public Usertype? UserType { get; set; }
        public string UserEmail { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public Branch? Branch { get; set; }
        public int NumberOfTardies { get; set; }
        public int NumberOfLeaveEarly { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
    }
}
