using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.Public.Dto
{
    public class WorkingStatusUserDto
    {
        public string EmailAddress { get; set; }
        public DateTime DateAt { get; set; }
        public RequestType RequestType { get; set; }
        public DayType DayType { get; set; }
        public OnDayType? OnDayType { get; set; }
        public double Hour { get; set; }
        public RequestStatus Status { get; set; }
        public string Message
        {
            get
            {
                string statusName = Enum.GetName(typeof(RequestStatus), this.Status);
                string strRequestType = $"[{statusName}] {Enum.GetName(typeof(RequestType), this.RequestType)} ";
                if (this.DayType != DayType.Custom)
                {
                    return strRequestType + Enum.GetName(typeof(DayType), this.DayType);
                }

                if (!OnDayType.HasValue)
                {
                    return strRequestType + this.Hour + "h";
                }

                return strRequestType + Enum.GetName(typeof(OnDayType), this.OnDayType) + ": " + Hour + "h";
                
            }
        }
    }
}
