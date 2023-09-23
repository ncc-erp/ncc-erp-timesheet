using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Services.FaceId.Dto;

namespace Timesheet.Users.Dto
{
    public class UpdateAvatarFileDto
    {
        public ImagesInfo ImagesInfo { get; set; }
        public IFormFile File { get; set; }
    }
}
