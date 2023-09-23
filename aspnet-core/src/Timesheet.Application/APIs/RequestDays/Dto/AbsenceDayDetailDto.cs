using Abp.AutoMapper;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.MyAbsenceDays.Dto
{
    [AutoMapTo(typeof(AbsenceDayDetail))]
    public class AbsenceDayDetailDto : Entity<long>
    {
        public long RequestId { get; set; }
        public DateTime DateAt { get; set; }
        public DayType DateType { get; set; }
        public double Hour { get; set; }
        public OnDayType? AbsenceTime { get; set; }
        public RequestStatus Status { get; set; }

        public string ToKomuString()
        {
            if (this.DateType != DayType.Custom)
            {
                return $"{DateTimeUtils.ToString(DateAt)} [{Enum.GetName(typeof(DayType), DateType)}] - {StatusName()}";
            }

            if (!AbsenceTime.HasValue)
            {
                return $"{DateTimeUtils.ToString(DateAt)} [{this.Hour}h] - {StatusName()}";
            }
            return $"{DateTimeUtils.ToString(DateAt)} [{Enum.GetName(typeof(OnDayType), AbsenceTime)}: {Hour}h] - {StatusName()}";
        }

        public string ToEmailString()
        {
            if (this.DateType != DayType.Custom)
            {
                return $"{DateTimeUtils.ToString(DateAt)} [{Enum.GetName(typeof(DayType), DateType)}] - {StatusName()}";
            }

            if (!AbsenceTime.HasValue)
            {
                return $"{DateTimeUtils.ToString(DateAt)} [{this.Hour}h] - {StatusName()}";
            }
            return $"{DateTimeUtils.ToString(DateAt)} [{Enum.GetName(typeof(OnDayType), AbsenceTime)}: {Hour}h] - {StatusName()}";
        }
        
        public string StatusName()
        {
            return CommonUtils.RequestStatusName(Status);
        }

    }
}

