using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.MyAbsenceDays.Dto
{
    public class GetMyRequestDto : Entity<long>
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string DayOffName { get; set; }
        public RequestStatus Status { get; set; }       // trang thai pending, aprove, reject
        public string Reason { get; set; }
        public RequestType Type { get; set; }  // kieu nghi la leave, onsite, remote
        public RequestDetailDto Detail { get; set; }
    }

    public class RequestDetailDto : Entity<long>
    {
        public string DateAt { get; set; }
        public DayType DateType { get; set; }            // full sang chieu custom
        public double Hour { get; set; }
        public OnDayType? AbsenceTime { get; set; }   // dau, giua, cuoi h
    }

}
