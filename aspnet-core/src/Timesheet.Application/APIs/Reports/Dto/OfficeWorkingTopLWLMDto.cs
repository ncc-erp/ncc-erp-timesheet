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
        public int TotalAllLW { get; set; }
        public int OfficeLW { get; set; }
        public int WfhLW { get; set; }
        public int TotalAllLM { get; set; }
        public int OfficeLM { get; set; }
        public int WfhLM { get; set; }
        public double TotalAllLWHours => Math.Round(TotalAllLW / 60.0, 2);
        public double OfficeLWHours => Math.Round(OfficeLW / 60.0, 2);
        public double WfhLWHours => Math.Round(WfhLW / 60.0, 2);
        public double TotalAllLMHours => Math.Round(TotalAllLM / 60.0, 2);
        public double OfficeLMHours => Math.Round(OfficeLM / 60.0, 2);
        public double WfhLMHours => Math.Round(WfhLM / 60.0, 2);
    }
}
