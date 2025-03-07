namespace Timesheet.Services.Mezon.Dto
{
    public class OAuth2Request
    {
        public string Code { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }
    }
}
