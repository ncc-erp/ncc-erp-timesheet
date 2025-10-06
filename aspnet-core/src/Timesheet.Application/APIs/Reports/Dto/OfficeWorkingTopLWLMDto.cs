using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Reports.Dto
{
    public class OfficeWorkingTopLWLMDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string OfficeName { get; set; }
        public string OfficeCode { get; set; }
        public double TotalAllLW { get; set; } 
        public double OfficeLW { get; set; } 
        public double WfhLW { get; set; }
        public double TotalAllLM { get; set; } 
        public double OfficeLM { get; set; } 
        public double WfhLM { get; set; } 
        public double TotalAllLWHours => TotalAllLW;
        public double OfficeLWHours => OfficeLW;
        public double WfhLWHours => WfhLW;
        public double TotalAllLMHours => TotalAllLM;
        public double OfficeLMHours => OfficeLM;
        public double WfhLMHours => WfhLM;
    }
}