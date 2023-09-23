using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Timesheets.Timesheets.Dto
{
    public class MyTimeSheetDto : EntityDto<long>
    {
        public string EmailAddress { get; set; }
        public TimesheetStatus Status { get; set; }
        public int WorkingTime { get; set; }//min     
        public DateTime DateAt { get; set; }
        public long ProjectId { get; set; }
        public string User { get; set; }
        public long UserId { get; set; }
        public long TaskId { get; set; }
        public string TaskName { get; set; }
        public string MytimesheetNote { get; set; }
        public string CustomerName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public TypeOfWork TypeOfWork { get; set; }
        public bool IsCharged { get; set; }
        public bool IsUserInProject { get; set; } //true - timesheet cua user thuoc project co PM la Abp.Session.UserId
        public string BranchName { get; set; } //HN, ĐN
        public Branch? Branch { get; set; }
        public Usertype? Type { get; set; } // Staff, Intern, CTV
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
        public UserLevel? Level { get; set; }
        public List<string> ListPM { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string BranchColor { get; set; }
        public string BranchDisplayName { get; set; }
        public string LastModifierUser { get; set; }
        public bool IsTemp { get; set; }
        public bool? IsUnlockedByEmployee { get; set; }

        public string WorkType
        {
            get
            {
                return CommonUtils.ProjectUserWorkType(IsTemp);
            }
        }
        public double? OffHour { get; set; }
        public bool IsOffDay;
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }
    public class RequestDetail
    {
        public DateTime DateAt { get; set; }
        public double Hour { get; set; }
        public long CreatorUserId { get; set; }
    }


    public class TimeSheetWarningDto : EntityDto<long>
    {
        public long UserId { get; set; }
        public string EmailAddress { get; set; }
        public string TaskName { get; set; }
        public string MytimesheetNote { get; set; }
        public string ProjectName { get; set; }

        public int WorkingTime { get; set; }//min     
        public double WorkingTimeHour
        {
            get
            {
                return CommonUtils.ConvertMinuteToHour(WorkingTime);
            }
        }
        public double HourOff { get; set; }//min     
        public DateTime DateAt { get; set; }
        public int TotalWorkingTimeDateAt { get; set; }//min  
        public double TotalWorkingTimeHourDateAt
        {
            get
            {
                return CommonUtils.ConvertMinuteToHour(TotalWorkingTimeDateAt);
            }
        }

        public TimesheetStatus Status { get; set; }
        public bool IsThanDefaultWorkingHourPerDay
        {
            get
            {
                if (CommonUtils.ConvertMinuteToHour(WorkingTime) > 8 - HourOff)
                {
                    return true;
                }
                return false;
            }
        }

    }

    public class TotalWorkingTimeUserAtDate
    {
        public long UserId { get; set; }
        public int TotalWorkingTime { get; set; }//min     
        public DateTime DateAt { get; set; }
        public double HourOff { get; set; }

        public int TotalWorkingTimeDateAt { get; set; }//min    
        public bool IsThanDefaultWorkingHourPerDay 
        {
            get
            {
                if (CommonUtils.ConvertMinuteToHour(TotalWorkingTime) > 8 - (HourOff + CommonUtils.ConvertMinuteToHour(TotalWorkingTimeDateAt) ) )
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsThanDefaultWorkingHourPerSaturday
        {
            get
            {
                if (DateAt.DayOfWeek == DayOfWeek.Saturday && 
                    CommonUtils.ConvertMinuteToHour(TotalWorkingTime) > 4 - (HourOff + CommonUtils.ConvertMinuteToHour(TotalWorkingTimeDateAt)))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsOffDay;
    }

    public class QuantiyTimesheetStatusDto
    {
        public long UserId { get; set; }
        public DateTime DateAt { get; set; }
        public TimesheetStatus Status { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }

}