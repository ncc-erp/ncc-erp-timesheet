using Ncc.Tests;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;
using Xunit;

namespace Timesheet.Tests.Utils
{
    public class CommonUtils_Tests : TimesheetTestBase
    {
        [Fact]
        public void SeparateMessage_Test()
        {

            string[] inputMessage = {"a234", "b23456","c234567","d2345689"};
            int maxLength = 12;
            // Act
            var output = CommonUtils.SeparateMessage(inputMessage, maxLength, "\n");

            output.Count.ShouldBe(3);
            output[0].ShouldBe("a234\nb23456\n");
            output[1].ShouldBe("c234567\n");
            output[2].ShouldBe("d2345689\n");
        }

        [Fact]
        public void SeparateMessage2_Test()
        {

            string inputMessage = "a234 b23456 c234567 d2345689";
            int maxLength = 12;
            // Act
            var output = CommonUtils.SeparateMessage(inputMessage, maxLength, " ");

            output.Count.ShouldBe(3);
            output[0].ShouldBe("a234 b23456 ");
            output[1].ShouldBe("c234567 ");
            output[2].ShouldBe("d2345689 ");
        }
    }
}
