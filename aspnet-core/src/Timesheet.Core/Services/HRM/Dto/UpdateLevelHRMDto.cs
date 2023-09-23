using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.Services.HRM.Dto
{
    public class UpdateLevelHRMDto
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }
        [JsonProperty("newLevel")]
        public UserLevel? NewLevel { get; set; }
        [JsonProperty("type")]
        public Usertype? Type { get; set; }
        [JsonProperty("isFullSalary")]
        public bool? IsFullSalary { get; set; }
        [JsonProperty("subLevel")]
        public byte? SubLevel { get; set; }
        [JsonProperty("salary")]
        public int? Salary { get; set; }
        public string RequestName { get; set; }
        public long UpdateToHRMByUserId { get; set; }
        public DateTime ExcutedDate { get; set; }
        public string NormalizedEmailAddress { get; set; }

    }
}
