using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class AbsenceReportDto
    {
        public long userId { get; set; }
        public string userFirstname { get; set; }
        public string userSurname { get; set; }
        public string userEmail { get; set; }

        public UserLevel? userLevel { get; set; }
        public Usertype? userType { get; set; }

        public string userLevelName {get; set;}
        public string userTypeName { get; set; }
        public string Position { get; set; }
        public string Branch { get; set; }
        public string BranchDisplayName { get; set; }
        public string BranchColor { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public long totalTime { get; set; }
    }
}
