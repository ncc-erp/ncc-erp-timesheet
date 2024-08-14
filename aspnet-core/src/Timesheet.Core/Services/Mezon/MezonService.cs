using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using Abp.Configuration;
using System.Threading.Tasks;
using Timesheet.Services.Mezon.Dto;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ncc.Configuration;

namespace Timesheet.Services.Mezon
{
    public class MezonService : IMezonService
    {
        private HttpClient httpClient;
        private readonly ILogger<MezonService> logger;
        public MezonService(HttpClient httpClient, ISettingManager settingManager, ILogger<MezonService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;

            var baseAddress = settingManager.GetSettingValueForApplication(AppSettingNames.MezonBaseAddress);
            var securityCode = settingManager.GetSettingValueForApplication(AppSettingNames.MezonSecurityCode);

            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("Security-Code", securityCode);
        }
        public async Task<OpenTalkListDto[]> GetOpenTalkLog()
        {
            var url = "api/GetAllTotalTimeJoinOpentalkDay";
            return await GetAsync<OpenTalkListDto[]>(url);
        }
        public async Task<OpenTalkListDto[]> GetOpenTalkLogByDay(DateTime day)
        {
            var url = "api/GetAllTotalTimeJoinOpentalkDay?time="+day.ToString("yyyy'/'MM'/'dd");
            return await GetAsync<OpenTalkListDto[]>(url);
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

                    JArray responseJObj = JArray.Parse(responseContent);
                    var result = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj));
                    return result;
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Get: {fullUrl} => Exception: {e}");

            }
            return default;

        }
    }
}
