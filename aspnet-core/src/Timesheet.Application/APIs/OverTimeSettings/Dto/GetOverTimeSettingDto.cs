using System;
using Timesheet.Anotations;
using Abp.Domain.Entities;

namespace Timesheet.APIs.OverTimeSettings.Dto
{
    public class GetOverTimeSettingDto : Entity<long>
    {
        [ApplySearchAttribute]
        public string ProjectName { get; set; }
        public DateTime DateAt { get; set; }
        public string Note { get; set; }
        public double Coefficient { get; set; }
        public long ProjectId { get; set; }
    }
}
