using Abp.Timing;
using Amazon.S3.Model.Internal.MarshallTransformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.Services.Project.Dto;

namespace Timesheet.Uitls
{
    public class DateTimeUtils
    {
        // All now function use Clock.Provider.Now
        public static DateTime GetNow()
        {
            return Clock.Provider.Now;
        }
        public static DateTime FirstDayOfWeek(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(-6).Date : date.AddDays(1 - (int)date.DayOfWeek).Date;
        }
        public static DateTime FirstDayOfYear(DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }
        public static DateTime LastDayOfYear(DateTime date)
        {
            return new DateTime(date.Year, 12, 31);
        }

        public static DateTime FirstDayOfMonth(DateTime date)
        {
            return date.AddDays(1 - date.Day).Date;
        }

        public static DateTime LastDayOfMonth(DateTime date)
        {
            return FirstDayOfMonth(date).AddMonths(1).AddDays(-1);
        }

        public static DateTime LastDayOfWeek(DateTime date)
        {
            return FirstDayOfWeek(date).AddDays(6);
        }

        public static DateTime FirstDayOfCurrentyWeek()
        {
            return FirstDayOfWeek(GetNow());
        }

        public static DateTime LastDayOfCurrentWeek()
        {
            return LastDayOfWeek(GetNow());
        }
        public static DateTime FirstDayOfCurrentyYear()
        {
            return FirstDayOfYear(GetNow());
        }
        public static DateTime LastDayOfCurrentyYear()
        {
            return LastDayOfYear(GetNow());
        }

        public static List<DateTime> GetListWeek(DateTime startDate, DateTime endDate)
        {
            var date = startDate.Date;
            var result = new List<DateTime>();
            while (date <= endDate)
            {
                var firstDayOfWeek = FirstDayOfWeek(date);
                if (!result.Contains(firstDayOfWeek))
                {
                    result.Add(firstDayOfWeek);
                }
                date = date.AddDays(7);
            }
            return result;
        }

        public static List<DateTime> GetListMonth(DateTime startDate, DateTime endDate)
        {
            var date = startDate.Date;
            var result = new List<DateTime>();
            while (date <= endDate)
            {
                var firstDayOfMonth = FirstDayOfMonth(date);
                if (!result.Contains(firstDayOfMonth))
                {
                    result.Add(firstDayOfMonth);
                }
                date = date.AddMonths(1);
            }
            return result;
        }

        public static string GetWeekName(DateTime firstDayOfWeek)
        {
            var lastDayOfWeek = LastDayOfWeek(firstDayOfWeek);
            if (firstDayOfWeek.Month == lastDayOfWeek.Month)
            {
                return firstDayOfWeek.Day.ToString() + "-" + lastDayOfWeek.Day + "/" + firstDayOfWeek.Month;
            }
            else
            {
                return firstDayOfWeek.Day.ToString() + "/" + firstDayOfWeek.Month + "-" + lastDayOfWeek.Day + "/" + lastDayOfWeek.Month;
            }

        }

        public static string GetMonthName(DateTime date)
        {
            return date.ToString("MM-yyyy");

        }

        public static string ToString(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy");
        }

        public static List<DateTime> GetListWorkingDate(List<DateTime> offDays, int year, int month)
        {
            var date = new DateTime(year, month, 1);
            var listWorkingDate = new List<DateTime>();
            while (date.Month == month)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday
                    && date.DayOfWeek != DayOfWeek.Sunday
                    && !offDays.Contains(date.Date))
                {
                    listWorkingDate.Add(date.Date);
                }
                date = date.AddDays(1);
            }

            return listWorkingDate;
        }
        public static long NowToMilliseconds()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static string NowToYYYYMMddHHmmss()
        {
            return GetNow().ToString("yyyyMMddHHmmss");
        }

        public static DateTime GetStartDateForAutoSubmitTS(DateTime now)
        {
            var startDate = now.AddDays(-1);
            while (startDate.DayOfWeek != DayOfWeek.Monday && startDate.Day != 1)
            {
                startDate = startDate.AddDays(-1);
            }
            return startDate;
        }

        public static DateTime GetEndDateForAutoSubmitTS(DateTime startDate)
        {
            var endDate = startDate.AddDays(1);
            while (endDate.DayOfWeek != DayOfWeek.Sunday)
            {
                endDate = endDate.AddDays(1);
            }
            return endDate;
        }

        public static List<StartEndDate> GetStartEndDates(List<TimeJoinOut> listTimeJoinOut)
        {
            if (listTimeJoinOut == null || listTimeJoinOut.Count() == 0)
            {
                return null;
            }

            var results = new List<StartEndDate>();
            var sortedList = listTimeJoinOut.OrderBy(s => s.DateAt).ToArray();
            int length = sortedList.Length;
            if (length == 1 && sortedList[0].IsJoin)
            {
                results.Add(new StartEndDate { StartDate = sortedList[0].DateAt.Date, EndDate = GetNow().Date });
                return results;
            }

            if (length == 1 && !sortedList[0].IsJoin)
            {
                return results;
            }

            for (int i = 0; i < length - 1; i++)
            {
                var current = sortedList[i];
                var next = sortedList[i + 1];
                if (current.IsJoin && !next.IsJoin)
                {
                    results.Add(new StartEndDate
                    {
                        StartDate = current.DateAt.Date,
                        EndDate = next.DateAt.Date
                    });
                    continue;
                }

                if (next.IsJoin && i == length - 2)
                {
                    results.Add(new StartEndDate
                    {
                        StartDate = next.DateAt.Date,
                        EndDate = GetNow().Date
                    });
                }

            }

            return results;

        }
        public static bool IsOffDay(List<DateTime> dayOffSettings, DateTime dateAt)
        {
            if (dayOffSettings.Contains(dateAt.Date) || dateAt.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }

            return false;
        }

        public static DayOfWeek[] ListDayOfWeek()
        {
            DayOfWeek[] DayOfWeekName = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

            return DayOfWeekName;
        }

        public static bool IsTheSameWeek(DateTime date1, DateTime date2)
        {
            return FirstDayOfWeek(date1) == FirstDayOfWeek(date2);
        }
        public static bool IsTheSameMonth(DateTime date1, DateTime date2)
        {
            return FirstDayOfMonth(date1) == FirstDayOfMonth(date2);
        }

        public static bool DateBetween(DateTime input, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return true;
            }
            return (input.Date >= startDate.Value.Date && input.Date <= endDate.Value.Date);
        }
        public static bool DateBetweenMonth(DateTime input, DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return true;
            }
            return (input.Date >= startDate.Value.Date && input.Date <= endDate.Value.Date)
                    || IsTheSameMonth(input, startDate.Value)
                    || IsTheSameMonth(input, endDate.Value);
        }

        public static DateTime Max(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }
        public static DateTime Min(DateTime date1, DateTime date2)
        {
            return date1 < date2 ? date1 : date2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">HH:mm:ss ex: 10:48:25.829000</param>
        /// <returns>minutes, return -1 if exception</returns>
        public static int ConvertHHmmssToMinutes(string input)
        {
            try
            {
                var time = DateTime.Parse(input);

                if (time.TimeOfDay.Hours == 0 && time.TimeOfDay.Minutes == 0)
                {
                    return 0;
                }
                return time.TimeOfDay.Hours * 60 + (int)time.TimeOfDay.Minutes + 1;
            }
            catch
            {
                return -1;
            }
        }

        public static int ConvertHourToMinutes(double input)
        {
            return (int)input * 60;
        }


        public static double ConvertMinuteToHour(double minute)
        {
            return Math.Round(minute / 60, 2);
        }


        public static List<DateTime> GetListSaturdayDate(int year, int month)
        {
            var date = new DateTime(year, month, 1);
            var resultDates = new List<DateTime>();
            while (date.Month == month)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    resultDates.Add(date.Date);
                }
                date = date.AddDays(1);
            }

            return resultDates;
        }
    }


}
