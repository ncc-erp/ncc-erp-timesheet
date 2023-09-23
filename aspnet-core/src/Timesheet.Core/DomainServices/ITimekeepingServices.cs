using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;

namespace Timesheet.DomainServices
{
    public interface ITimekeepingServices : IDomainService
    {
        Task<List<Timekeeping>> AddTimekeepingByDay(DateTime selectedDate);
        CheckInOutTimeDto CaculateCheckInOutTime(Dictionary<long, MapAbsenceUserDto> mapAbsenceUsers, TimesheetUserDto user);
        void CheckIsPunished(Timekeeping timekeeping, int LimitMinute);
        void CheckIsPunished(Timekeeping timekeeping);
        Task<object> NoticePunishUserCheckInOut(DateTime now);
        Task CheckIsPunishedByRule(Timekeeping timekeeping, int limitedMinute, float trackerTime);
    }
}
