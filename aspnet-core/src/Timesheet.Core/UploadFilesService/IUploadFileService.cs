using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.UploadFilesService
{
    public interface IUploadFileService
    {
        Task<string> UploadAvatarAsync(IFormFile file, string tenantName);

        Task<string> UploadFileAsync(IFormFile file, string[] allowFileTypes, string filePath);

        Task<string> UploadTeamBuildingAsync(IFormFile file, string tenantName);

    }
}
