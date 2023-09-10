using System;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Anotations;
using Timesheet.Uitls;

namespace Timesheet.APIs.KomuTrackers.Dto
{
    public class GetKomuTrackerUserDto
    {
        public DateTime DateAt { get; set; }
        public float WorkingMinute { get; set; }
        public string ComputerName { get; set; }
        public string FullName { get; set; }
        public Usertype? UserType { get; set; }
        [ApplySearchAttribute]
        public string EmailAddress { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public Branch? Branch { get; set; }
        public string WorkingHour => CommonUtils.ConvertHourToHHmm((int)this.WorkingMinute);
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
    }
}
