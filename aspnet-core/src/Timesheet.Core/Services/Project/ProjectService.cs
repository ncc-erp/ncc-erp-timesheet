using Abp.Configuration;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Services.AuthenticateDto;
using Timesheet.Services.Project.Dto;

namespace Timesheet.Services.Project
{
    public class ProjectService : IProjectService
    {
        private HttpClient httpClient;
        private readonly string baseAddress;
        private readonly string securityCode;
        private readonly ILogger<ProjectService> logger;

        public ProjectService(HttpClient httpClient, IConfiguration configuration, ILogger<ProjectService> logger)
        {
            this.logger = logger;
            this.httpClient = httpClient;

            baseAddress = configuration.GetValue<string>("ProjectService:BaseAddress");
            securityCode = configuration.GetValue<string>("ProjectService:SecurityCode");


            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("X-Secret-Key", securityCode);
        }

        public async Task<List<UpdateStarRateToProjectDto>> UpdateStarRateToProject(List<UpdateStarRateToProjectDto> input)
        {
            var url = "/api/services/app/User/UpdateStarRateFromTimesheet";
            return await PostAsync<List<UpdateStarRateToProjectDto>>(url, input);
        }

        public void UpdateAvatarToProject(UpdateAvatarDto input)
        {
            var url = $"/api/services/app/TimeSheet/UpdateAvatarFromTimesheet";
            Post(url, input);
        }
        public void UpdateAllAvatarToProject(List<UpdateAvatarDto> input)
        {
            var url = $"/api/services/app/TS/UpdateAllAvatarFromTimesheet";
            Post(url, input);
        }


        public async Task<T> GetAsync<T>(string url)
        {
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogInformation($"Get: {baseAddress}/{url} => Response: {responseContent}");
                    JObject responseJObj = JObject.Parse(responseContent);
                    var result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj["result"]));
                    if (result == null)
                    {
                        result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj));
                    }
                    return result;
                }
                else
                {
                    return default;
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Get: {baseAddress}/{url} => Exception: {e}");
                return default;
            }

        }

        public void Post(string url, object input)
        {
            string strInput = JsonConvert.SerializeObject(input);
            try
            {
                logger.LogInformation($"Post: {baseAddress}/{url} input: {strInput}");
                var contentString = new StringContent(strInput, Encoding.UTF8, "application/json");
                httpClient.PostAsync(url, contentString);
            }
            catch (Exception e)
            {
                logger.LogError($"Post: {baseAddress}/{url} input: {strInput} Error: {e.Message}");
            }

        }
        public async Task<T> PostAsync<T>(string url, object input)
        {
            string strInput = JsonConvert.SerializeObject(input);

            try
            {
                logger.LogInformation($"Post: {baseAddress}/{url} input: {strInput}");

                var contentString = new StringContent(strInput, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(url, contentString);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    logger.LogInformation($"Post: {baseAddress}/{url} input: {strInput} response: {responseContent}");

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
                logger.LogError($"Post: {baseAddress}/{url} input: {strInput} Error: {e.Message}");
            }

            return default;


        }


        public List<UserInOutProject> GetTempUsersInOutProjectHistory( string projectCode)
        {
            var url = $"/api/services/app/public/GetTempUsersInOutProjectHistory?projectCode={projectCode}";
            return GetAsync<List<UserInOutProject>>(url).Result;
        }

        public List<string> GetCurrentTempEmailsInProject(string projectCode)
        {
            var url = $"/api/services/app/public/GetCurrentTempEmailsInProject?projectCode={projectCode}";
            return GetAsync<List<string>>(url).Result;
        }

        public List<CurrentTempProjectUserDto> GetAllCurrentTempProjectUser()
        {
            var url = $"/api/services/app/public/GetAllCurrentTempProjectUser";
            return GetAsync<List<CurrentTempProjectUserDto>>(url).Result;
        }

        public async Task<GetResultConnectDto> CheckConnectToProject()
        {
            var res = await GetAsync<GetResultConnectDto>($"/api/services/app/public/CheckConnect");
            if (res == null)
            {
                return new GetResultConnectDto
                {
                    IsConnected = false,
                    Message = "Can not connect to Project"
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
        public List<GetProjectPMNameDto> GetProjectPMName()
        {
            var url = $"/api/services/app/TimeSheet/GetListPMByProjectCode";
            return GetAsync<List<GetProjectPMNameDto>>(url).Result;
        }
        //TODO: unable to test, result = null


    }
}
