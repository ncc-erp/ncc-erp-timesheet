using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.APIs.MyWorkingTimes.Dto;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ManageWorkingTimes.Dto
{
    public class UserHistoryWorkingTime : HistoryWorkingTimeDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);

        public string BranchDisplayName { get; set; }
        public Usertype? Type { get; set; }
        public string UserName { get; set; }
        public string BranchColor { get; set; }
    }
}
