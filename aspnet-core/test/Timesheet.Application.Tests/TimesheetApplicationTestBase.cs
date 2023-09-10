using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Tests;
using Xunit;

namespace Timesheet.Application.Tests
{
    [Collection("Sequential")]
    public abstract class TimesheetApplicationTestBase : TesBase<TimesheetApplicationTestModule>
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .Build();
    }
}
