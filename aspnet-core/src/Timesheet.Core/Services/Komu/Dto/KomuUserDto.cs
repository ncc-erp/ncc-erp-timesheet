using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Services.Komu.Dto
{
    public class KomuUserDto
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("userid")]
        public ulong? KomuUserId { get; set; }
    }
}
