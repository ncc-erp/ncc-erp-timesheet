using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Timesheet.Services.Mezon.Dto
{
    public class UserInfoResponse
    {
        [JsonProperty("aud")]
        public List<string> Audience { get; set; }

        [JsonProperty("auth_time")]
        public long AuthTime { get; set; }

        [JsonProperty("iat")]
        public long IssuedAt { get; set; }

        [JsonProperty("iss")]
        public string Issuer { get; set; }

        [JsonProperty("rat")]
        public long RequestedAt { get; set; }

        [JsonProperty("sub")]
        public string Subject { get; set; }

        [JsonProperty("user_id")]
        public string MezonUserId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }

        public DateTime AuthTimeUtc => DateTimeOffset.FromUnixTimeSeconds(AuthTime).UtcDateTime;
        public DateTime IssuedAtUtc => DateTimeOffset.FromUnixTimeSeconds(IssuedAt).UtcDateTime;
        public DateTime RequestedAtUtc => DateTimeOffset.FromUnixTimeSeconds(RequestedAt).UtcDateTime;
    }
}
