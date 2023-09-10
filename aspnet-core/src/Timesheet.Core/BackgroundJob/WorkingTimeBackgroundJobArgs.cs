using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Entities;

namespace Timesheet.BackgroundJob
{
    public class WorkingTimeBackgroundJobArgs
    {
        public HistoryWorkingTime Target { get; set; }
    }
}
