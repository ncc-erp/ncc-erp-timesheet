using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timesheet.Controllers.Dto
{
    public class TokenDto
    {
        public string googleToken { get; set; }
        public string secretCode { get; set; }
    }

    public class OAuth2TokenDto
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
