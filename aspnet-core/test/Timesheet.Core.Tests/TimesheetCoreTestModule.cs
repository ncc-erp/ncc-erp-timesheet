using Abp.Modules;
using Ncc.Tests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Core.Tests
{
    [DependsOn(
    typeof(TimesheetTestModule)
    )]
    public class TimesheetCoreTestModule : AbpModule
    {
    }
}
