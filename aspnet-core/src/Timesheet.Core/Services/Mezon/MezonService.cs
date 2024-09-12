using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using Abp.Configuration;
using Timesheet.Services.Mezon.Dto;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Ncc.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace Timesheet.Services.Mezon
{
    public class MezonService : IMezonService
    {
        private readonly ILogger<MezonService> logger;
        private readonly ISettingManager _settingManager;
        public MezonService(HttpClient httpClient, ISettingManager settingManager, ILogger<MezonService> logger)
        {
            this.logger = logger;
            this._settingManager = settingManager;
        }
        public OpenTalkListDto[] GetOpenTalkLog(DateTime? day = null)
        {
            string url;
            if (day.HasValue)
            {
                url = "api/GetAllTotalTimeJoinOpentalkDay?time=" + day.Value.ToString("yyyy'/'MM'/'dd");
            } else
            {
                url = "api/GetAllTotalTimeJoinOpentalkDay";
            }
            return Get<OpenTalkListDto[]>(url);
        }
        public T Get<T>(string url)
        {
            try
            {
                // Bypass the certificate
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                //Gọi api bên Mezon
                using (var client = new HttpClient(clientHandler))
                {

                    client.BaseAddress = new Uri(_settingManager.GetSettingValueForApplication(AppSettingNames.MezonBaseAddress));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Security-Code", _settingManager.GetSettingValueForApplication(AppSettingNames.MezonSecurityCode));

                    var response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        //Convert data
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        logger.LogInformation($"GET {client.BaseAddress}{url} => Response: {responseContent}");
                        JArray responseJObj = JArray.Parse(responseContent);
                        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"GetOpenTalkLog() Error: {ex.Message}");
            }

            return default;

        }
        public void NotifyToChannel(string MezonUrl, string MezonMessage)
        {
            Post(MezonUrl, new { type = "TIMESHEET", message = new { t = MezonMessage } });
        }
        public void Post(string url, object input)
        {
            string strInput = JsonConvert.SerializeObject(input);
            try
            {
                // Bypass the certificate
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                //Gọi api bên Mezon
                using (var client = new HttpClient(clientHandler))
                {

                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    logger.LogInformation($"Post: {url} input: {strInput}");

                    var contentString = new StringContent(strInput, Encoding.UTF8, "application/json");

                    var test = client.PostAsync(url, contentString).Result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"MezonService Post: {url} input: {strInput} Error: {ex.Message}");
            }
        }
    }
}
