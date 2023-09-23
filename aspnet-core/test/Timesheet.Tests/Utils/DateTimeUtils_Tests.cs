using Ncc.Tests;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using Xunit;

namespace Timesheet.Tests.Utils
{
    public class DateTimeUtils_Tests : TimesheetTestBase
    {
        [Fact]
        public void GetListSaturdayDate_Test1()
        {
            int year = 2023;
            int month = 7;

            var output = DateTimeUtils.GetListSaturdayDate(year, month);

            output.Count.ShouldBe(5);
            Equals(output[0].ToString("yyyy-MM-dd"), "2023-07-01");
            Equals(output[1].ToString("yyyy-MM-dd"), "2023-07-08");
            Equals(output[2].ToString("yyyy-MM-dd"), "2023-07-15");
            Equals(output[3].ToString("yyyy-MM-dd"), "2023-07-22");
            Equals(output[4].ToString("yyyy-MM-dd"), "2023-07-29");
        }

        [Fact]
        public void GetListSaturdayDate_Test2()
        {
            int year = 2023;
            int month = 2;

            var output = DateTimeUtils.GetListSaturdayDate(year, month);

            output.Count.ShouldBe(4);
            Equals(output[0].ToString("yyyy-MM-dd"), "2023-02-04");
            Equals(output[1].ToString("yyyy-MM-dd"), "2023-02-11");
            Equals(output[2].ToString("yyyy-MM-dd"), "2023-02-18");
            Equals(output[3].ToString("yyyy-MM-dd"), "2023-02-25");
        }

        [Fact]
        public void GetListSaturdayDate_Test3()
        {
            int year = 2020;
            int month = 2;

            var output = DateTimeUtils.GetListSaturdayDate(year, month);

            output.Count.ShouldBe(5);
            Equals(output[0].ToString("yyyy-MM-dd"), "2020-02-01");
            Equals(output[1].ToString("yyyy-MM-dd"), "2020-02-08");
            Equals(output[2].ToString("yyyy-MM-dd"), "2020-02-15");
            Equals(output[3].ToString("yyyy-MM-dd"), "2020-02-22");
            Equals(output[4].ToString("yyyy-MM-dd"), "2020-02-29");
        }
    }
}
