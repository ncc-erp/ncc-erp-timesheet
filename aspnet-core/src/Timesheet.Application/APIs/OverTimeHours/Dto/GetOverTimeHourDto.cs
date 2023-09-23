using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timesheet.Anotations;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.OverTimeHours.Dto
{
    public class GetOverTimeHourDto
    {
        public long UserId { get; set; }
        public string NormalizedEmailAddress { get; set; }
        [ApplySearchAttribute]
        public string EmailAddress { get; set; }
        [ApplySearchAttribute]
        public string FullName { get; set; }
        public string BranchCode { get; set; }
        public string Branch { get; set; }
        public long BranchId { get; set; }
        public List<OverTimeHourDto> ListOverTimeHour { get; set; }

        public class OverTimeHourDto
        {
            public DateTime DateAt { get; set; }

            public double WorkingMinute { get; set; }
            public long ProjectId { get; set; }
            public string ProjectName { get; set; }

            public bool IsCharged { get; set; }

            public double? ProjectCoefficient { get; set; }
            public double? DayOffCoefficient { get; set; }


            public double Coefficient { 
                get
                {
                    if (!IsCharged)
                    {
                        return 1;
                    }

                    if (ProjectCoefficient >= 1)
                    {
                        return ProjectCoefficient.Value;
                    }
                    if (DayOffCoefficient >= 1)
                    {
                        return DayOffCoefficient.Value;
                    }

                    if ( DateAt.DayOfWeek == DayOfWeek.Saturday || DateAt.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return 2;
                    }
                    return 1;
                }
            }

            public string Date
            {
                get
                {
                    return this.DateAt.ToString("yyyy-MM-dd");
                }
            }

            public int Day
            {
                get
                {
                    return DateAt.Day;
                }

            }
            public string DayName
            {
                get
                {
                    return this.DateAt.DayOfWeek.ToString();
                }
            }

            public double WorkingHour
            {
                get
                {
                    return (double) WorkingMinute / 60;
                }
            }

            public double OTHour
            {
                get
                {
                    return WorkingHour * Coefficient;
                }
            }

        }
    }
}
