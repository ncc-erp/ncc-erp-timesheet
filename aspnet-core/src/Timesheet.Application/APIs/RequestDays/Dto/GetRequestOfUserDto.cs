using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.RequestDays.Dto
{
    public class GetRequestOfUserDto : Entity<long>
    {
        public long UserId { get; set; }

        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public double Hour { get; set; }

        public string DayOffName { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
        public RequestType LeavedayType { get; set; }

        public OnDayType? AbsenceTime { get; set; }
        public bool IsFuture {
            get
            {
                if (DateAt.Date > DateTimeUtils.GetNow().Date)
                {
                    return true;
                }
                return false;
            }
        }   


    }
}
