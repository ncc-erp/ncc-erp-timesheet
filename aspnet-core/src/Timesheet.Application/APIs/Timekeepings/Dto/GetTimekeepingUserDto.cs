using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timekeepings.Dto
{
    public class GetTimekeepingUserDto
    {
        public long TimekeepingId { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public Usertype? UserType { get; set; }
        public string UserEmail { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public Branch? Branch { get; set; }
        public DateTime Date { get; set; }
        public string RegistrationTimeStart { get; set; }
        public string RegistrationTimeEnd { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public double? ResultCheckIn { get; set; }
        public double? ResultCheckOut { get; set; }
        public long? EditByUserId { get; set; }
        public string EditByUserName { get; set; }
        public PunishmentStatus Status { get; set; }
        public string UserNote { get; set; }
        public string NoteReply { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
        public long? BranchId { get; set; }
        public CheckInCheckOutPunishmentType StatusPunish { get; set; }
        public int MoneyPunish { get; set; }
        public string TrackerTime { get; set; }
        public string StrTimekeepingId => TimekeepingId.ToString();
    }
}
