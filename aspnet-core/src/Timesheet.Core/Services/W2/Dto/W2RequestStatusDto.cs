using System;

namespace Timesheet.Services.W2.Dto
{
    public class W2RequestStatusDto
    {
        public string Email { get; set; }
        public string MezonId { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public string Type { get; set; }
        public string Meta { get; set; }
    }

    public class W2RequestMetaDto
    {
        public string Reason { get; set; }
        public string Reason_label { get; set; }
    }
}
