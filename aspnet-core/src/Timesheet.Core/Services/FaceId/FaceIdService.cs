using Abp.Configuration;
using Abp.Extensions;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Web;
using Timesheet.Constants;
using Timesheet.DomainServices.Dto;
using Timesheet.Services.FaceId.Dto;
using Ncc.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using Abp.Configuration;
using Timesheet.DomainServices;
using Ncc.IoC;
using System.Text;

namespace Timesheet.Services.FaceIdService
{
    public class FaceIdService: BaseDomainService
    {
        private HttpClient httpClient;
        private ILogger<FaceIdService> logger;
        private readonly string baseAddress;
        private readonly string securityCode;
        private ISettingManager settingManager;

        public FaceIdService(HttpClient httpClient, ILogger<FaceIdService> logger, ISettingManager _settingManager, IWorkScope workScope) : base(workScope) 
        {
            this.httpClient = httpClient;
            this.logger = logger;
            baseAddress = ConstantFaceId.baseAddress;
            securityCode = ConstantFaceId.securityCode;

            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("X-Secret-Key", securityCode);
            this.settingManager = _settingManager;  

            //HttpClientHandler clientHandler = new HttpClientHandler();
            //clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

        }

        public async Task<List<ImagesInfo>> GetAllImagesAsync()
        {
            return await GetAsync<List<ImagesInfo>>($"employees/most-recent-images");
        }
        public List<ImagesInfo> GetAllImages()
        {
            return GetUseEncodingAsync<List<ImagesInfo>>($"employees/most-recent-images").Result;
        }

        public virtual List<UserCheckInDto> GetEmployeeCheckInOutMini(DateTime date)
        {
            /*
             NOTE: phải viết hàm riêng để cho API này vì secret key bị khác với key của ConstantFaceId.securityCode
             */

            try
            {
                // Bypass the certificate
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                //Gọi api bên Face ID
                using (var client = new HttpClient(clientHandler))
                {

                    client.BaseAddress = new Uri(settingManager.GetSettingValue(AppSettingNames.CheckInInternalUrl));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-Account-Id", settingManager.GetSettingValue(AppSettingNames.CheckInInternalAccount));
                    client.DefaultRequestHeaders.Add("X-Secret-Key", settingManager.GetSettingValue(AppSettingNames.CheckInInternalXSecretKey));

                    var response = client.GetAsync("v1/exports/payroll/employees-timesheet-mini?date=" + date.ToString("yyyy-MM-dd")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        //Convert data
                        var resultString = response.Content.ReadAsStringAsync().Result;
                        logger.LogInformation($"GetEmployeeCheckInOutMini {date.ToString("yyyy-MM-dd")} response: {resultString}");
                        return JsonConvert.DeserializeObject<List<UserCheckInDto>>(resultString);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"GetEmployeeCheckInOutMini Error: {ex.Message}");
            }
            
            return default;
        }

        private async Task<T> GetAsync<T>(string Url)
        {
            string fullUrl = $"{httpClient.BaseAddress}/{Url}";
            logger.LogInformation($"Get: {fullUrl}");
            try
            {
                var response = await httpClient.GetAsync(Url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogInformation($"Get: {fullUrl} response: {responseContent}");
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Get: {fullUrl} Error: {e.Message}");
            }
            return default;
        }

        private async Task<T> GetUseEncodingAsync<T>(string Url)
        {
            string baseAddress = httpClient.BaseAddress?.ToString().TrimEnd('/');
            string url = Url?.TrimStart('/');

            string fullUrl = $"{baseAddress}/{url}";
            logger.LogInformation($"Get: {fullUrl}");

            try
            {
                var response = await httpClient.GetAsync(Url);
                if (response.IsSuccessStatusCode)
                {
                    var byteArray = await response.Content.ReadAsByteArrayAsync();
                    var result = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                    logger.LogInformation($"Get: {fullUrl} response: {result}");

                    if (result.IsNullOrEmpty()) return default;

                    return JsonConvert.DeserializeObject<T>(result);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Get: {fullUrl} Error: {e.Message}");
            }
            return default;
        }
    }
}