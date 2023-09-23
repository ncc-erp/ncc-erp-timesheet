using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timesheet.Services.Project.Dto;
using Timesheet.Services.HRM.Dto;
using static Timesheet.Services.HRM.Dto.UpdateToHrmDto;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Timesheet.Services.HRM
{
    public class HRMService : IHRMService
    {
        private HttpClient httpClient;
        private readonly ILogger<HRMService> logger;

        public HRMService(HttpClient httpClient, IConfiguration configuration, ILogger<HRMService> logger)
        {
            this.logger = logger;
            this.httpClient = httpClient;

            var baseAddress = configuration.GetValue<string>("HRMService:BaseAddress");
            var securityCode = configuration.GetValue<string>("HRMService:SecurityCode");

            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("X-Secret-Key", securityCode);
        }


        public async Task<string> UpdateLevel(CreateRequestFromTSDto input)
        {
            var url = "api/services/app/User/UpdateLevel";
            return await PostAsync<string>(url, input);
        }

        public async Task<UpdateLevelHRMDto> UpdateLevelAfterRejectEmail(UpdateLevelHRMDto input)
        {
            var url = "api/services/app/User/UpdateLevelAfterReject";
            return await PostAsync<UpdateLevelHRMDto>(url, input);
        }

        public async Task<UserContactDto> GetUserContract(long id)
        {
            var url = $"api/services/app/Users/GetUserToTimesheet?id={id}";
            return await GetAsync<UserContactDto>(url);
        }

        public void UpdateAvatarToHRM(UpdateAvatarDto input)
        {
            var url = $"api/services/app/Users/UpdateAvatarFromTimesheet";
            Post(url, input);
        }
        public void UpdateAllAvatarToHRM(List<UpdateAvatarDto> input)
        {
            var url = $"api/services/app/Users/UpdateAllAvatarFromTimesheet";
            Post(url, input);
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var fullUrl = $"{httpClient.BaseAddress}/{url}";
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    logger.LogInformation($"Get: {fullUrl} => Response: {responseContent}");

                    JObject responseJObj = JObject.Parse(responseContent);
                    var result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj["result"]));
                    if (result == null)
                    {
                        result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj));
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Get: {fullUrl} => Exception: {e}");
               
            }
            return default;

        }

        public void Post(string url, object input)
        {
            var fullUrl = $"{httpClient.BaseAddress}/{url}";
            string strInput = JsonConvert.SerializeObject(input);
            try
            {
                logger.LogInformation($"Post: {fullUrl} input: {strInput}");
                var contentString = new StringContent(strInput, Encoding.UTF8, "application/json");
                httpClient.PostAsync(url, contentString);
            }
            catch (Exception e)
            {
                logger.LogError($"Post: {fullUrl} input: {strInput} Error: {e.Message}");
            }

        }
        public async Task<T> PostAsync<T>(string url, object input)
        {
            var fullUrl = $"{httpClient.BaseAddress}/{url}";
            string strInput = JsonConvert.SerializeObject(input);

            try
            {
                logger.LogInformation($"Post: {fullUrl} input: {strInput}");

                var contentString = new StringContent(strInput, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, contentString);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    logger.LogInformation($"Post: {fullUrl} input: {strInput} response: {responseContent}");

                    JObject responseJObj = JObject.Parse(responseContent);

                    var result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj["result"]));
                    if (result == null)
                    {
                        result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj));
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Post: {fullUrl} input: {strInput} Error: {e.Message}");
            }

            return default;


        }
    
    }
}