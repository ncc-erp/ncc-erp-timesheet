using System;
using Ncc.Entities.Enum;

namespace Timesheet.Helper
{
    public class TimeHelper {
        public static (int startHour, int endHour, bool isFullday) GetTimeRangeNotifyPmReviewIntern (string notifyAtHourType)
        {
            if (Enum.TryParse(notifyAtHourType, out StatusEnum.NotifyAtHourType type))
            {
                return type == StatusEnum.NotifyAtHourType.Fullday ? (10, 24, true) : (-1, -1, false);
            }
            return (-1, -1, false);
        }
    }
}
