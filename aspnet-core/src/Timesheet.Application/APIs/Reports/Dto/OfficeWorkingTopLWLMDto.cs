using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.APIs.Reports.Dto
{
    public class OfficeWorkingTopLWLMDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchColor { get; set; }
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
    public class UserLiteDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchColor { get; set; }
    }
    public struct RemoteFlags
    {
        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public RemoteFlags(bool morning, bool afternoon)
        {
            Morning = morning;
            Afternoon = afternoon;
        }
    }
    public class TopOfficeWorkingTimeDto
    {
        public long OfficeId { get; set; }
        public int Limit { get; set; }
        public DateTime? ReportDate { get; set; }
        public long? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class SendTopOfficeWorkingTimeNotificationDto
    {
        public long? OfficeId { get; set; }
        public int Limit { get; set; }
        public DateTime? ReportDate { get; set; }
        public string MezonUrl { get; set; }
        public long? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ShowAll { get; set; }
    }
    public class UserMapDto
    {
        public string Name { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchColor { get; set; }
        public int Minutes { get; set; }
    }
}