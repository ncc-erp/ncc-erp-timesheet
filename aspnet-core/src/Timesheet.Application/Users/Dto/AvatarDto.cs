using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using Timesheet.Uitls;

namespace Timesheet.Users.Dto
{
    public class AvatarDto
    {
        public IFormFile File { get; set; }
        public long UserId { get; set; }
    }

    public class GetAvatarPathDto
    {
        public string AvatarPath { get; set; }
        public string AvatarFullPath => FileUtils.FullFilePath(AvatarPath);
    }
}
