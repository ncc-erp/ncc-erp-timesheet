using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static Ncc.Entities.Enum.StatusEnum;

namespace Timesheet.DomainServices.Dto
{
    public class CheckInCheckOutPunishmentSettingDto
    {
        [JsonProperty("id")]
        public CheckInCheckOutPunishmentType Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("note")]
        public string Note { get; set; }
        [JsonProperty("money")]
        public int Money { get; set; }    
    }
    public class InputToUpdateSettingDto
    {
        public CheckInCheckOutPunishmentType Id { get; set; }
        public int Money { get; set; }
    }
    public class CheckInCheckOutPunishmentSettingAndPercentTrackerConfigDto
    {
        public string PercentOfTrackerOnWorking { get; set; }
        public List<CheckInCheckOutPunishmentSettingDto> CheckInCheckOutPunishmentSetting { get; set; }
    }
}
