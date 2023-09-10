using Abp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Timesheet.Uitls
{
    public class UrlUtils
    {
        public static void CheckValidUrlFileContentType(string url, string[] allowFileTypes)
        {
            string urlExt = Path.GetExtension(url);
            //if (urlExt == "")
            //    throw new UserFriendlyException("Url file is invalid");
            urlExt = urlExt.Substring(1).ToLower();
            if (!allowFileTypes.Contains(urlExt))
                throw new UserFriendlyException($"Wrong file type {urlExt}. Allow file types: {string.Join(", ", allowFileTypes)}");
        }
        public static void CheckValidUrl(string url)
        {
            Uri uriResult;
            bool isValidUrl=Uri.TryCreate(url,UriKind.Absolute, out uriResult) &&
                (uriResult.Scheme==Uri.UriSchemeHttp || uriResult.Scheme==Uri.UriSchemeHttps);
            if (!isValidUrl)
            {
                throw new UserFriendlyException("Invalid Url");
            }
        }
        public static string FormatUrlWithFullDomain(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            if (url.StartsWith("http://") || url.StartsWith("https://")) return url;
            if (Constants.ConstantUploadFile.Provider == Constants.ConstantUploadFile.AMAZONE_S3)
            {
                return Constants.ConstantAmazonS3.CloudFront.TrimEnd('/') + "/" + url.TrimStart('/');
            }
            else
            {
                return Constants.ConstantInternalUploadFile.RootUrl.TrimEnd('/') + "/" + url;
            }
        }
    }
}
