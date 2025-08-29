using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Reports.Dto
{
    public class UserTimeReportByLWAndLMDto
    {
        public string UserName { get; set; }
        public string BranchName { get; set; }
        public double TotalTimeLW { get; set; }            
        public double TotalTimeLM { get; set; }            
        public double TotalWorkingTimeLW { get; set; }      
        public double TotalWorkingTimeLM { get; set; }   
    }
}

