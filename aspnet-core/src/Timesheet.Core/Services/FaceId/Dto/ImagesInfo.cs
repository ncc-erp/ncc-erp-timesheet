using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Constants;

namespace Timesheet.Services.FaceId.Dto
{
    public class ImagesInfo
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("img")]
        public string Img { get; set; }
        [JsonProperty("checkInAt")]
        public DateTime CheckInAt { get; set; }
        public string GetUrl()
        {
            return ConstantFaceId.ImageCheckInPath + Img;
        }
    }
}
