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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Timesheet.Constants;

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
            var mkList = ExtractMarkdown(MezonMessage);
            var mentions = ExtractMentions(MezonMessage);

            Post(MezonUrl, new
            {
                type = MezonConstant.MEZON_NOTI_TYPE,
                message = new
                {
                    t = MezonMessage,
                    mk = mkList,
                    mentions = mentions
                }
            });
        }

        public List<object> ExtractMarkdown(string message)
        {
            var mkList = new List<object>();
            string markdownPattern = MezonConstant.MARKDOWN_PATTERN;
            var matches = Regex.Matches(message, markdownPattern);
            if (matches.Count == 0) return mkList;

            for (int i = 0; i < matches.Count; i++)
            {
                var currentMatch = matches[i];
                if (i + 1 < matches.Count && matches[i + 1].Value == currentMatch.Value)
                {
                    mkList.Add(new
                    {
                        type = currentMatch.Value == MezonConstant.TRIPLE_BACKTICKS ? MezonConstant.MULTILINE_MARKDOWN_CODE_TYPE : MezonConstant.INLINE_MARKDOWN_CODE_TYPE,
                        s = currentMatch.Index,
                        e = matches[i + 1].Index + matches[i + 1].Length
                    });
                    i++;
                }
            }
            return mkList;
        }

        public List<object> ExtractMentions(string message)
        {
            var mentions = new List<object>();
            string mentionPattern = MezonConstant.MENTION_PATTERN;
            var matches = Regex.Matches(message, mentionPattern);
            if (matches.Count == 0 )  return mentions; 

            foreach (Match match in matches)
            {
                mentions.Add(new
                {
                    username = match.Groups[1].Value,
                    s = match.Index,
                    e = match.Index + match.Length
                });
            }
            return mentions;
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
