using Abp.Collections.Extensions;
using Abp.UI;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ncc.Authorization.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Constants;
using Timesheet.Uitls;

namespace Timesheet.UploadFilesService
{
    public class AmazonS3Service : IUploadFileService
    {
        private readonly ILogger<AmazonS3Service> logger;
        private readonly IAmazonS3 s3Client;

        public AmazonS3Service(ILogger<AmazonS3Service> logger, IAmazonS3 _s3Client)
        {
            this.logger = logger;
            this.s3Client = _s3Client;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string[] allowFileTypes, string filePath)
        {
            var strAlowFileType = string.Join(", ", allowFileTypes);
            logger.LogInformation($"UploadFile() fileName: {file.FileName}, contentType: {file.ContentType}, allowFileTypes: {strAlowFileType}, filePath: {filePath}");
            FileUtils.CheckValidFile(file, allowFileTypes);

            var key = $"{ConstantAmazonS3.Prefix?.TrimEnd('/')}/{filePath}";

            logger.LogInformation($"UploadImageFile() Key: {key}");
            var request = new PutObjectRequest()
            {
                BucketName = ConstantAmazonS3.BucketName,
                Key = key,
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            var response = await s3Client.PutObjectAsync(request);
            logger.LogDebug(JsonConvert.SerializeObject(response));
            return key;
        }

      

        public async Task<string> UploadAvatarAsync(IFormFile file, string tenantName)
        {
            var filePath = $"{ConstantUploadFile.AvatarFolder?.TrimEnd('/')}/{tenantName}/{DateTimeUtils.NowToYYYYMMddHHmmss()}_{Guid.NewGuid()}.{FileUtils.GetFileExtension(file)}";
            return await UploadFileAsync(file, ConstantUploadFile.AllowImageFileTypes, filePath);
        }

        public async Task<string> UploadTeamBuildingAsync(IFormFile file, string tenantName)
        {
            var filePath = $"{ConstantTeamBuildingFile.ParentFolder?.TrimEnd('/')}/{tenantName}/{ConstantTeamBuildingFile.FileFolder}/{DateTimeUtils.NowToYYYYMMddHHmmss()}_{Guid.NewGuid()}.{FileUtils.GetFileExtension(file)}";
            return await UploadFileAsync(file, ConstantTeamBuildingFile.AllowFileTypes, filePath);
        }
    }
}
