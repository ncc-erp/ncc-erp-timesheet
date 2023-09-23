using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Anotations;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.NormalWorkingHours.Dto
{
    public class GetNormalWorkingHourDto
    {
        public long UserId { get; set; }
        [ApplySearchAttribute]
        public string Name { get; set; }
        [ApplySearchAttribute]
        public string Surname { get; set; }
        [ApplySearchAttribute]
        public string UserName { get; set; }
        [ApplySearchAttribute]
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public double TotalWorkingHour { get; set; }
        public double TotalWorkingday { get; set; }
        public double TotalWorkingHourOfMonth { get; set; }
        public double TotalOpenTalk { get; set; }
        public Usertype? Type { get; set; }
        public string BranchName { get; set; }
        public Branch? Branch { get; set; }
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public UserLevel? Level { get; set; }
        public Sex? Sex { get; set; }
        public bool IsPM { get; set; }
        public bool IsUnlockPM { get; set; }
        public bool IsUnlock { get; set; }
        public string BranchDisplayName { get; set; }
        public string BranchColor { get; set; }
        public IEnumerable<WorkingHourDto> ListWorkingHour { get; set; }
    }
    public class WorkingHourDto
    {
        public int Date { get; set; }
        public string DayName { get; set; }
        public double? WorkingHour { get; set; }
        public bool IsOpenTalk { get; set; }
        public DayType? AbsenceType { get; set; }
        public double? OffHour { get; set; }
        public bool IsLock { get; set; }
        public bool IsOnsite { get; set; }
        public bool IsOffDaySetting;
        public int TotalOpenTalkPerDay { get; set; }
        public bool IsThanDefaultWorkingHourPerDay
        {
            get
            {
                double offHour = OffHour.HasValue ? OffHour.Value : 0;
                double hour = DayName == DayOfWeek.Saturday.ToString() ? 4 : 8;
                if (WorkingHour > hour - offHour)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsOffDay { get; set; }
        public List<AbsenceDetaiInDay> ListAbsenceDetaiInDay { get; set; }

        public bool IsNoOffAndNoCheckIn { get; set; }
        public bool IsNoCheckIn { get; set; }

        public bool IsHalfWidth
        {
            get
            {
                if (IsOffDay || IsOffDaySetting || DayName == DayOfWeek.Sunday.ToString())
                {
                    return true;
                }
                return false;
            }
        }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }

        public string CheckInShow
        {
            get
            {
                return string.IsNullOrEmpty(CheckIn) ? "--:--" : CheckIn;
            }
        }
        public string CheckOutShow
        {
            get
            {
                return string.IsNullOrEmpty(CheckOut) ? "--:--" : CheckOut;
            }
        }

        public string InfoTooltip
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(WorkingHour.HasValue ? "Timesheet normal " + WorkingHour.Value : "Timesheet normal 0");
                if (IsThanDefaultWorkingHourPerDay)
                {
                    sb.AppendLine("TS log > working time");
                }
                if (IsLock && !IsOffDaySetting)
                {
                    sb.AppendLine("Day locked");
                }
                if (IsOffDaySetting)
                {
                    sb.AppendLine("Day off");
                }
                ListAbsenceDetaiInDay.ForEach(absenceDetaiInDay =>
                {
                    sb.AppendLine(GetInfoAbsence(absenceDetaiInDay));
                });
                sb.AppendLine(string.IsNullOrEmpty(CheckIn) ? "No Check In" : "Check In: " + CheckIn);
                sb.AppendLine(string.IsNullOrEmpty(CheckOut) ? "No Check Out" : "Check Out: " + CheckOut);
                return sb.ToString();
            }
        }

        private string GetInfoAbsence(AbsenceDetaiInDay absenceDetaiInDay)
        {
            if (absenceDetaiInDay == null) return null;
            var result = "";
            switch (absenceDetaiInDay.Type)
            {
                case RequestType.Onsite:
                    result = "Onsite ";
                    break;
                case RequestType.Remote:
                    result = "Remote ";
                    break;
                default:
                    result = "Off ";
                    break;
            }

            if (absenceDetaiInDay.AbsenceType == DayType.Fullday)
            {
                return result += "Full day";
            }
            if (absenceDetaiInDay.AbsenceType == DayType.Morning)
            {
                return result += "Morning";
            }
            if (absenceDetaiInDay.AbsenceType == DayType.Afternoon)
            {
                return result += "Afternoon";
            }
            if (absenceDetaiInDay.AbsenceType == DayType.Custom &&
              absenceDetaiInDay.AbsenceTime == OnDayType.DiMuon)
            {
                return "Đi muộn: " + absenceDetaiInDay.Hour;
            }
            return "Về sớm: " + absenceDetaiInDay.Hour;
        }
        public string ShortDayName { 
            get {
                return DayName.Substring(0, 3);
            } 
        }
    }

    public class AbsenceDetaiInDay
    {
        public DayType? AbsenceType { get; set; }
        public double Hour { get; set; }
        public OnDayType? AbsenceTime { get; set; }
        public RequestType Type { get; set; }
    }

    public class AbsenceDayDetailNormal
    {
        public long UserId { get; set; }
        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public double Hour { get; set; }
        public long RequestId { get; set; }
        public RequestType Type { get; set; }
        public OnDayType? AbsenceTime { get; set; }
    }

    public class NoOffNoCheckInDto
    {
        public long UserId { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public RequestType? RequestType { get; set; }
        public DateTime DateAt { get; set; }
    }

    public class CheckInDto
    {
        public long? UserId { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public DateTime DateAt { get; set; }
    }

    public class GetNormalWorkingHourByUserLoginDto
    {
        public double TotalWorkingHour { get; set; }
        public double TotalOpenTalk { get; set; }
        public IEnumerable<WorkingHourDto> ListWorkingHour { get; set; }
    }

}
