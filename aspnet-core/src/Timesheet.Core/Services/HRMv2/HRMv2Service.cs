using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Services.HRMv2.Dto;
using Timesheet.Services.Project.Dto;
using static Timesheet.Services.HRM.Dto.UpdateToHrmv2Dto;

namespace Timesheet.Services.HRMv2
{
    public class HRMv2Service : IHRMv2Service
    {
        private HttpClient httpClient;
        private readonly ILogger<HRMv2Service> logger;

        public HRMv2Service(HttpClient httpClient, IConfiguration configuration, ILogger<HRMv2Service> logger)
        {
            this.logger = logger;
            this.httpClient = httpClient;

            var baseAddress = configuration.GetValue<string>("HRMv2Service:BaseAddress");
            var securityCode = configuration.GetValue<string>("HRMv2Service:SecurityCode");

            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("X-Secret-Key", securityCode);
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


        protected virtual async Task<T> PostAsync<T>(string url, object input)
        {
            var fullUrl = $"{ httpClient.BaseAddress }/{ url}";
            var strInput = JsonConvert.SerializeObject(input);
            var contentString = new StringContent(strInput, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(url, contentString);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogInformation($"Post: {fullUrl} input: {strInput} response: { responseContent}");
                    return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception ex)
            {
                logger.LogError($"Post: {fullUrl} error: { ex.Message}");
            }
            return default;
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
        public void UpdateAvatarToHrm(UpdateAvatarDto input)
        {
            var url = $"/api/services/app/Timesheet/UpdateAvatarFromTimesheet";
            Post(url, input);
        }

        public void CreateRequestHrmv2(InputCreateRequestHrmv2Dto input)
        {
            var url = "api/services/app/Timesheet/ReviewInternFromTimesheet";
            Post(url, input);
        }

        public async Task<string> ComplainPayslipMail(InputComplainPayslipMail input)
        {
            var url = "api/services/app/Timesheet/ComplainPayslipMail";
            var response = await PostAsync<AbpResponseResult<string>>(url, input);
            return response?.Result;
        }

        public async Task<string> ConfirmPayslipMail(InputConfirmPayslipMail input)
        {
            var url = $"api/services/app/Timesheet/ConfirmPayslipMail";
            var response = await PostAsync<AbpResponseResult<string>>(url, input);
            return response?.Result;
        }


        public async Task<GetUserInfoByEmailDto> GetUserInfoByEmail(string email)
        {
            var url = $"api/services/app/Timesheet/GetUserInfoByEmail?email={email}";
            var response =  await GetAsync<GetUserInfoByEmailDto>(url);
            return response;
        }
        public async Task<GetInfoToUPDateProfile> GetInfoToUpdate(string email)
        {
            var url = $"api/services/app/Timesheet/GetInfoToUpdate?email={email}";
            var response = await GetAsync<GetInfoToUPDateProfile>(url);
            return response;
        }
        public async Task<List<ItemInfoDto>> GetAllBanks()
        {
            var url = $"api/services/app/Timesheet/GetAllBanks";
            var response = await GetAsync<List<ItemInfoDto>>(url);
            return response;
        }


        public async Task<ResultUpdateInfo> UpdateUserInfo(UpdateUserInfoDto input)
        {
            var url = "api/services/app/Timesheet/CreateRequestUpdateUserInfo";
            var response = await PostAsync<AbpResponseResult<ResultUpdateInfo>>(url, input);
            return response.Result;
        }

        public async Task<GetResultConnectDto> CheckConnectToHRM()
        {
            var res = await GetAsync<GetResultConnectDto>("api/services/app/Public/CheckConnect");
            if (res == null)
            {
                return new GetResultConnectDto
                {
                    IsConnected = false,
                    Message = "Can not connect to HRM"
                };
            }

            if (res.IsConnected == false)
            {
                return new GetResultConnectDto
                {
                    IsConnected = false,
                    Message = res.Message
                };
            }

            return res;
        }

    }
}
