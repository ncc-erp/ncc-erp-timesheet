using System;
using Ncc.Entities.Enum;
using Timesheet.Constants;

namespace Timesheet.Helper
{
    public class TimeHelper {
        public static (int startHour, int endHour, bool isFullday) GetTimeRangeNotifyPmReviewIntern (string notifyAtHourType)
        {
            if (Enum.TryParse(notifyAtHourType, out StatusEnum.NotifyAtHourType type))
            {
                return type == StatusEnum.NotifyAtHourType.Fullday 
                    ? (CommonConstant.NotifyReviewIntern_Fullday_StartHour, CommonConstant.NotifyReviewIntern_Fullday_EndHour, true) 
                    : (-1, -1, false);
            }
            return (-1, -1, false);
        }
    }
}
