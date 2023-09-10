using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using Timesheet.Extension;
using static Sieve.Extensions.MethodInfoExtended;
using Timesheet.Uitls;
using Abp.Configuration;
using Ncc.Configuration;

namespace Timesheet.DomainServices
{
    public class SendMessageToUserServices : BaseDomainService, ISendMessageToUserServices, ITransientDependency
    {
        public List<SendMessageToPunishUserDto> GetListUserPunish()
        {
            DateTime DateNow = DateTime.Now;

            List<SendMessageToPunishUserDto> query;
            var predicate = PredicateBuilder.New<Timekeeping>();

            if (DateNow.DayOfWeek >= DayOfWeek.Tuesday && DateNow.DayOfWeek <= DayOfWeek.Friday)
            {
                predicate.And(s => s.DateAt.Date == DateNow.AddDays(-1).Date);
            }
            else if(DateNow.DayOfWeek == DayOfWeek.Monday)
            {
                predicate.And(s => s.DateAt.Date == DateNow.AddDays(-3).Date);
            }

            query = WorkScope.GetAll<Timekeeping>()
                .Where(s => s.MoneyPunish > 0)
                .Where(s => s.UserNote == null)
                .Where(s => s.NoteReply == null ? true : s.NoteReply.ToLower() == "stoped working" ? false : true)
                .Where(predicate)
                .Select(s => new SendMessageToPunishUserDto
                {
                    UserName = s.User.UserName,
                    DateAt = s.DateAt.Date,
                    MoneyPunish = s.MoneyPunish,
                    CheckIn = s.CheckIn,
                    CheckOut = s.CheckOut,
                    IsPunishedCheckIn = s.IsPunishedCheckIn,
                    IsPunishedCheckOut = s.IsPunishedCheckOut,
                    RegisterCheckIn = s.RegisterCheckIn,
                    RegisterCheckOut = s.RegisterCheckOut,
                    ResultCheckIn = CommonUtils.SubtractHHmm(s.CheckIn, s.RegisterCheckIn),
                    ResultCheckOut = CommonUtils.SubtractHHmm(s.RegisterCheckOut, s.CheckOut),
                    NotePunish = GetNotePunish(s.IsPunishedCheckIn, s.IsPunishedCheckOut, CommonUtils.SubtractHHmm(s.CheckIn, s.RegisterCheckIn), CommonUtils.SubtractHHmm(s.RegisterCheckOut, s.CheckOut))
                }).ToList();

            return query;
        }

        private string GetNotePunish(bool isPunishedCheckIn, bool isPunishedCheckOut, double? resultCheckIn, double? resultCheckOut)
        {
            var LimitedMinute = double.Parse(SettingManager.GetSettingValue(AppSettingNames.LimitedMinutes));

            if ((isPunishedCheckIn && !isPunishedCheckOut) && (resultCheckIn > LimitedMinute))
                return "Being Late For Work";
            else if (isPunishedCheckIn && !isPunishedCheckOut && (resultCheckIn <= LimitedMinute))
                return "Not CheckIn";
            else if (!isPunishedCheckIn && isPunishedCheckOut && (resultCheckOut == 0))
            {
                return "Not CheckOut";
            }
            else if (isPunishedCheckIn && isPunishedCheckOut)
            {
                if(resultCheckIn > LimitedMinute && resultCheckOut == 0)
                {
                    return "Being Late For Work And Not CheckOut";
                }
                return "Not CheckIn And Not CheckOut";
            }
            return null;
        }
    }
}
