using Abp.Collections.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;
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
    public class MinioService : IUploadFileService
    {
        private readonly ILogger<MinioService> logger;
        private readonly MinioClient minioClient;

        public MinioService(ILogger<MinioService> logger, MinioClient minioClient)
        {
            this.logger = logger;
            this.minioClient = minioClient;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string[] allowFileTypes, string filePath)
        {
            var strAllowFileType = string.Join(", ", allowFileTypes);
            logger.LogInformation($"UploadFile() fileName: {file.FileName}, contentType: {file.ContentType}, allowFileTypes: {strAllowFileType}, filePath: {filePath}");
            FileUtils.CheckValidFile(file, allowFileTypes);

            var key = $"{ConstantMinIO.Prefix?.TrimEnd('/')}/{filePath}";
            var bucketName = ConstantMinIO.BucketName;

            logger.LogInformation($"UploadFile() Key: {key}");

            try
            {
                // Check if the bucket exists
                bool found = await minioClient.BucketExistsAsync(bucketName).ConfigureAwait(false);
                        if (!found)
                        {
                            await minioClient.MakeBucketAsync(bucketName).ConfigureAwait(false);
                        }

                // Upload the file to the bucket
                using (var stream = file.OpenReadStream())
                        {
                            await minioClient.PutObjectAsync(bucketName, key, stream, stream.Length, file.ContentType).ConfigureAwait(false);
                        }
                logger.LogInformation($"Successfully uploaded {key}");
                return key;
            }
            catch (MinioException e)
            {
                logger.LogError(e, "File Upload Error");
                throw new UserFriendlyException("File upload failed. Please try again later.");
            }
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