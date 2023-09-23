using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Timesheets.MyTimesheets.Dto
{
    public class WarningMyTimesheetDto
    {
        public long UserId { get; set; }
        public DateTime DateAt { get; set; }
        public double? HourOff { get; set; }
        public int WorkingTime { get; set; }//min     
        public int WorkingTimeLogged { get; set; }//min     
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
        public bool IsWarning
        {
            get
            {
                if (IsOffDay || string.IsNullOrEmpty(CheckOut) || string.IsNullOrEmpty(CheckIn) ||
                    MinuteActive - ((HourOff + HourDiMuon + HourVeSom) * 60 + WorkingTimeLogged) < WorkingTime)
                {
                    return true;
                }
                return false;
            }
        }
        public double? HourDiMuon { get; set; }
        public double? HourVeSom { get; set; }
        public bool IsOffHalfDay { get; set; }
        public bool IsOffDay { get; set; }
        public double? MinuteActive
        {
            get
            {
                var minuteCheckIn = CommonUtils.SubtractHHmm(CheckOut, CheckIn);
                if (IsOffHalfDay)
                {
                    return minuteCheckIn;
                }
                return minuteCheckIn - 60;
            }
        }
    }
}
