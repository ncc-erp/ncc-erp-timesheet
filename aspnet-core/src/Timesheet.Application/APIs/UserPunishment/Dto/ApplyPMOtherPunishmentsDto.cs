using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Timesheet.APIs.UserPunishments.Dto
{
    public class ApplyPMOtherPunishmentsDto
    {
        [Required]
        [JsonProperty("getPMOtherPunishmentAtMonth")]
        public int Month { get; set; }

        [Required]
        [JsonProperty("getPMOtherPunishmentAtYear")]
        public int Year { get; set; }
    }
}
