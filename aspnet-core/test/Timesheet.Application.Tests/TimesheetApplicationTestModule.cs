using Abp.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Core.Tests;

namespace Timesheet.Application.Tests
{
    [DependsOn(
    typeof(TimesheetCoreTestModule)
    )]
    public class TimesheetApplicationTestModule : AbpModule
    {
    }
}
