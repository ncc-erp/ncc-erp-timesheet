using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Ncc.Authorization.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Constants;
using Timesheet.Helper;
using Timesheet.Uitls;

namespace Timesheet.UploadFilesService
{
    public class InternalUploadFileService : IUploadFileService
    {
        private readonly string WWWRootFolder = "wwwroot";

        public async Task<string> UploadAvatarAsync(IFormFile file, string tenantName)
        {
            FileUtils.CheckValidFile(file, ConstantUploadFile.AllowImageFileTypes);
            var avatarFolder = Path.Combine(WWWRootFolder, ConstantUploadFile.AvatarFolder, tenantName);
            string fileLocation = UploadImages.CreateFolderIfNotExists(avatarFolder);

            var fileName = $"{DateTimeUtils.NowToYYYYMMddHHmmss()}_{Guid.NewGuid()}.{FileUtils.GetFileExtension(file)}";
            var filePath = $"{ConstantUploadFile.AvatarFolder?.TrimEnd('/')}/{tenantName}/{fileName}";

            await UploadImages.UploadFileAsync(fileLocation, file, fileName);

            return filePath;
        }

        public Task<string> UploadFileAsync(IFormFile file, string[] allowFileTypes, string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadTeamBuildingAsync(IFormFile file, string tenantName)
        {
            FileUtils.CheckValidFile(file, ConstantTeamBuildingFile.AllowFileTypes);
            var avatarFolder = Path.Combine(WWWRootFolder, ConstantTeamBuildingFile.ParentFolder, tenantName, ConstantTeamBuildingFile.FileFolder);
            string fileLocation = UploadImages.CreateFolderIfNotExists(avatarFolder);

            var fileName = $"{DateTimeUtils.NowToYYYYMMddHHmmss()}_{Guid.NewGuid()}.{FileUtils.GetFileExtension(file)}";
            var filePath = $"{ConstantTeamBuildingFile.ParentFolder?.TrimEnd('/')}/{tenantName}/{ConstantTeamBuildingFile.FileFolder}/{fileName}";
            await UploadImages.UploadFileAsync(fileLocation, file, fileName);
            return filePath;
        }
    }
}