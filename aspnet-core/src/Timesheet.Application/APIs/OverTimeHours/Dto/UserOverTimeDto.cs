using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.NormalWorkingHours.Dto
{
    public class UserOverTimeDto

    {
        public string Date { get; set; }
        public string DayName { get; set; }
        public double WorkingHour { get; set; }
        public double Coefficient { get; set; }
        public double ProjectCoefficient { get; set; }

        public UserOverTimeDto(string date, string dayName, double workingHour, Ncc.Entities.Enum.StatusEnum.Branch? branch, DayOffSetting setting, OverTimeSetting overTimeSetting)
        {
            Date = date;
            DayName = dayName;
            WorkingHour = workingHour;
            Coefficient = overTimeSetting != default ? overTimeSetting.Coefficient : (branch == Ncc.Entities.Enum.StatusEnum.Branch.DaNang && setting != null) ? setting.Coefficient :
                          (branch == Ncc.Entities.Enum.StatusEnum.Branch.HaNoi && setting != null) ? setting.Coefficient :
                          (branch == Ncc.Entities.Enum.StatusEnum.Branch.HoChiMinh && setting != null) ? setting.Coefficient :
                          (branch == Ncc.Entities.Enum.StatusEnum.Branch.Vinh && setting != null) ? setting.Coefficient :
                          (dayName == DayOfWeek.Saturday.ToString() && setting == null) ? 2 : 1;
        }
    }
}
