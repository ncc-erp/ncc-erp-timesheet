using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Timesheet.Uitls
{
    public class FileUtils
    {
        public static string FullFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }
            if (Constants.ConstantUploadFile.Provider == Constants.ConstantUploadFile.AMAZONE_S3)
            {
                return Constants.ConstantAmazonS3.CloudFront.TrimEnd('/') + "/" + filePath;
            }
            else
            {
                return Constants.ConstantInternalUploadFile.RootUrl.TrimEnd('/') + "/" + filePath;
            }
        }

        public static string GetFileExtension(IFormFile file)
        {
            if (file == default || string.IsNullOrEmpty(file.ContentType))
            {
                return "";
            }
     
            return Path.GetExtension(file.FileName).Substring(1).ToLower();
        }
        public static IFormFile CreateIFormFile(byte[] dataArr)
        {
            FormFile file = new FormFile(new MemoryStream(dataArr), 0, dataArr.Length, "avatar", "avatar.png");

            Dictionary<string, Microsoft.Extensions.Primitives.StringValues> dic = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
            dic.Add("Content-Type", "application/json");

            file.Headers = new HeaderDictionary(dic);
            return file;
        }
        public static void CheckValidFile(IFormFile file, string[] allowFileTypes)
        {
            var fileExt = FileUtils.GetFileExtension(file);
            if (!allowFileTypes.Contains(fileExt))
                throw new UserFriendlyException($"Wrong file type {file.ContentType}. Allow file types: {string.Join(", ", allowFileTypes)}");
        }
    }
}
