using Abp.AutoMapper;
using Abp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.Entities;
using Timesheet.Uitls;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.MyAbsenceDays.Dto
{
    [AutoMapTo(typeof(AbsenceDayRequest))]
    public class MyRequestDto 
    {
        public long DayOffTypeId { get; set; }
        public RequestStatus Status { get; set; }
        public string Reason { get; set; }
        public RequestType Type { get; set; }
        public List<AbsenceDayDetailDto> Absences { get; set; }

        public bool IsValidDiMuonVeSom()
        {
            return (((this.Type == RequestType.Off && !this.Absences.Any(s => s.AbsenceTime == OnDayType.MiddleOfDay || s.Hour <= 0))
                        || this.Type != RequestType.Off )
                    || !this.Absences.Any(s => s.DateType == DayType.Custom));
        }

        public string GetInValidDiMuonVeSom()
        {
            var dates = this.Absences.Where(s => (s.DateType == DayType.Custom && (s.AbsenceTime == OnDayType.MiddleOfDay || s.Hour <= 0) && this.Type == RequestType.Off)
                                              || (this.Type != RequestType.Off && s.DateType == DayType.Custom))
                                     .Select(s => s.DateAt.Date)
                                     .ToArray();

            return string.Join(", ", dates);
        }

        public string ToKomuStringRequestDates()
        {
            var sb = new StringBuilder();
            foreach (var item in Absences)
            {
                sb.AppendLine(item.ToKomuString());
            }
            return sb.ToString();
        }

        public string ToEmailString()
        {
            var sb = new StringBuilder();
            foreach (var item in Absences)
            {
                sb.AppendLine(item.ToEmailString());
            }
            return sb.ToString();
        }

        public string ListDay()
        {
            return Absences.Select(s => DateTimeUtils.ToString(s.DateAt)).ToList().Join(", ");
        }

        public string GetRequestName(string offTypeName)
        {
            if(this.Type == RequestType.Off && this.Absences.Any(s => s.DateType == DayType.Custom))
            {
                return "Đi muộn/Về sớm";
            }

            return CommonUtils.RequestTypeToString(this.Type, offTypeName);
        }
    }
}
