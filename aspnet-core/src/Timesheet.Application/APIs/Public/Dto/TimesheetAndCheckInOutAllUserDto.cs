using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Public.Dto
{
    public class TimesheetAndCheckInOutAllUserDto
    {
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public Usertype? UserType { get; set; }
        public string Branch { get; set; }
        public List<TimesheetAndCheckInOutDto> ListDate { get; set; }
    }

    public class TimesheetAndCheckInOutDto
    {
        public DateTime DateAt { get; set; }
        public int TimeSheetMinute { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public double CheckOutInMinute
        {
            get
            {
                var checkOutInMinute = CommonUtils.SubtractHHmm(CheckOut, CheckIn);
                if (CommonUtils.SubtractHHmm("12:00", CheckIn) > 0 && CommonUtils.SubtractHHmm(CheckOut, "13:00") > 0)
                {
                    checkOutInMinute -= 60;
                }
                return checkOutInMinute;
            }
        }
    }
}
