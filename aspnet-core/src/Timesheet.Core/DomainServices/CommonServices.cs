using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;

namespace Timesheet.DomainServices
{
    public class CommonServices : ITransientDependency, ICommonServices
    {
        public DateTime getlockDatePM()
        {
            byte[] arrDayOfWeek = { 7, 8, 2, 3, 4, 5, 6 };
            var now = DateTimeUtils.GetNow();
            var dayOfWeek = (int)Enum.Parse(typeof(System.DayOfWeek), now.DayOfWeek.ToString());
            var lockMonth = now.AddDays(-(now.Day));
            var lockWeek = now.AddDays(-arrDayOfWeek[dayOfWeek]);
            var lockDate = now.Day == 1 ? lockWeek : lockWeek > lockMonth ? lockWeek : lockMonth;
            return lockDate;
        }

        public DateTime getlockDateUser()
        {
            byte[] arrDayOfWeek = { 7, 1, 2, 3, 4, 5, 6 };
            var now = DateTimeUtils.GetNow();
            var dayOfWeek = (int)Enum.Parse(typeof(System.DayOfWeek), now.DayOfWeek.ToString());
            var lockMonth = now.AddDays(-(now.Day));
            var lockWeek = now.AddDays(-arrDayOfWeek[dayOfWeek]);
            var lockDate = lockWeek > lockMonth ? lockWeek : lockMonth;
            return lockDate;
        }
    }
}
