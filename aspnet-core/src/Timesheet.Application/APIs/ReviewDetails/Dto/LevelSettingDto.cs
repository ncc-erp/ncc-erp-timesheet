using Abp.Application.Services.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.APIs.ReviewDetails.Dto
{
    public class LevelSettingDto 
    {
        [JsonProperty("id")]
        public UserLevel Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        public int? Salary { get; set; }
        [JsonProperty("subLevels")]
        public List<SubLevelDto> SubLevels { get; set; }
    }
    public class SubLevelDto
    {
        [JsonProperty("id")]
        public byte Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("salary")]
        public int Salary { get; set; }
    }
}
