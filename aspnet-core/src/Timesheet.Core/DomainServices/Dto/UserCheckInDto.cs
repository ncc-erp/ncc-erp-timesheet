using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.DomainServices.Dto
{
    public class UserCheckInDto
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("dateTime")]
        public DateTime DateTime { get; set; }
        [JsonProperty("verifyStartTimeStr")]
        public string VerifyStartTimeStr { get; set; }
        [JsonProperty("verifyEndTimeStr")]
        public string VerifyEndTimeStr { get; set; }
    }
}
