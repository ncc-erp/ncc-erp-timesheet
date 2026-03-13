using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class BranchToDisplayDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Color { get; set; }
    }
    public class OfficeWorkingTopLWLMDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public double TotalAllLW { get; set; }
        public double OfficeLW { get; set; }
        public double WfhLW { get; set; }
        public double TotalAllLM { get; set; }
        public double OfficeLM { get; set; }
        public double WfhLM { get; set; }
        public BranchToDisplayDto Branch { get; set; }

    }
    public class UserLiteDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string MorningEndAt { get; set; }
        public string AfternoonStartAt { get; set; }
        public BranchToDisplayDto Branch { get; set; }
    }
    public class SendTopOfficeWorkingTimeNotificationDto
    {
        public long? OfficeId { get; set; }
        public int Limit { get; set; }
        public string MezonUrl { get; set; }
    }
    public class GetOfficeWorkingTimelogReportInputDto
    {
        public List<long> BranchIds { get; set; } = new List<long>();
        public int Limit { get; set; } = int.MaxValue;
    }
    public class ComputeWorkingHoursInputDto
    {
        public TimeSpan? CheckInTs { get; set; }
        public TimeSpan? CheckOutTs { get; set; }
        public TimeSpan? MorningEndAtTs { get; set; }
        public TimeSpan? AfternoonStartAtTs { get; set; }
        public double TrackerHours { get; set; }
        public bool IsRemoteRequest { get; set; }
        public DayType? RemoteDayType { get; set; }
    }
}
