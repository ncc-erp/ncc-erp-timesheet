using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using Abp.Configuration;
using System.Threading.Tasks;
using Abp.UI;
using System.Linq;
using Ncc.Configuration;

namespace Timesheet.DomainServices
{
    public class CommonServices : ITransientDependency, ICommonServices
    {
        private readonly ISettingManager _settingManager;

        public CommonServices(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

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

        public async Task checkIsMonthLocked(IEnumerable<DateTime> dates)
        {
            bool isLockTimesheet = await _settingManager.GetSettingValueAsync<bool>(AppSettingNames.LockTimesheet);
            if (isLockTimesheet)
            {
                DateTime firstDateOfCurrentMonth = DateTimeUtils.FirstDayOfMonth(DateTimeUtils.GetNow());

                if (dates.Any(date => date.Date < firstDateOfCurrentMonth))
                {
                    throw new UserFriendlyException("Timesheet of previous month is locked!");
                }
            }
        }
    }
}
