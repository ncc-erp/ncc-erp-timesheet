using Newtonsoft.Json;
namespace Timesheet.Authorization.Users
{
    public class MezonUser
    {
        [JsonProperty("id")]
        public string mezon_user_id { get; set; }
        public string username { get; set; }
        public string display_name { get; set; }
        public string avatar_url { get; set; }
        public string mezon_id { get; set; }
        public string sub { get; set; }
        public string email { get; set; }
    }
}
