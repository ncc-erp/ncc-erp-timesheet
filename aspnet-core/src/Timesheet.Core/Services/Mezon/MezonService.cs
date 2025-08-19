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
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Abp.Extensions;
using Ncc.Authorization.Users;
using System.Linq;
using Ncc.IoC;

namespace Timesheet.Services.Mezon
{
    public class MezonService : IMezonService
    {
        private static Dictionary<string, string> _mezonUserIdCache = new Dictionary<string, string>();
        private readonly ILogger<MezonService> logger;
        private readonly ISettingManager _settingManager;
        private HttpClient _httpClient;
        private const string serviceName = "MezonService";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly IWorkScope WorkScope;
        
        public MezonService(HttpClient httpClient, ISettingManager settingManager, ILogger<MezonService> logger, IConfiguration configuration, IWorkScope workScope)
        {
            this.logger = logger;
            this._settingManager = settingManager;
            this._httpClient = httpClient;
            this.WorkScope = workScope;
            _clientId = configuration.GetValue<string>($"{serviceName}:ClientId");
            _clientSecret = configuration.GetValue<string>($"{serviceName}:ClientSecret");
            _redirectUri = configuration.GetValue<string>($"{serviceName}:RedirectUri");
            var baseAddress = configuration.GetValue<string>($"{serviceName}:BaseAddress");
            httpClient.BaseAddress = new Uri(baseAddress);
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
                string username = match.Groups[1].Value;
                string mezonUserId = GetMezonUserIdByUsername(username);
                
                mentions.Add(new
                {
                    username = username,
                    user_id = mezonUserId,
                    s = match.Index,
                    e = match.Index + match.Length
                });
            }
            return mentions;
        }
        
        public string GetMezonUserIdByUsername(string username)
        {
            if (_mezonUserIdCache.TryGetValue(username, out string cachedUserId))
            {
                return cachedUserId;
            }

            try
            {
                var user = WorkScope.GetRepo<User>()
                    .FirstOrDefault(u => u.UserName == username);
                
                string mezonUserId = user?.MezonUserId;
                
                _mezonUserIdCache[username] = mezonUserId;
                
                return mezonUserId;
            }
            catch (Exception ex)
            {
                logger.LogError($"GetMezonUserIdByUsername({username}) Error: {ex.Message}");
                return null;
            }
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

        protected async Task<T> PostAsync<T>(string url, object input)
        {
            var fullUrl = $"{_httpClient.BaseAddress}/{url}";

            try
            {
                var body = new FormUrlEncodedContent(input as Dictionary<string, string>);

                var response = await _httpClient.PostAsync(url, body);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    logger.LogInformation($"Post: {fullUrl} input: {body} response: {responseContent}");
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Post: {fullUrl} error: {ex.Message}");
            }
            return default;
        }

        public async Task<OAuth2TokenResponse> GetTokenAsync(OAuth2Request request)
        {
            return await PostAsync<OAuth2TokenResponse>("oauth2/token", new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", request.Code },
                { "scope", request.Scope },
                { "state", request.State },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "redirect_uri", _redirectUri }
            });
        }

        public async Task<UserInfoResponse> GetUserInfoAsync(string accessToken)
        {
            var body = new Dictionary<string, string>()
            {
                { "access_token", Uri.EscapeDataString(accessToken) },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "redirect_uri", _redirectUri }
            };
            return await PostAsync<UserInfoResponse>("userinfo", body);
        }

        public string GenerateOAuthUrl()
        {
            var state = Guid.NewGuid().ToString("N").Truncate(11);

            return $"{_httpClient.BaseAddress}/oauth2/auth?" +
                   $"client_id={_clientId}&" +
                   $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
                   $"response_type=code&" +
                   $"scope=openid+offline&" +
                   $"state={state}";
        }

        public MezonServiceConfig GetConfig()
        {
            return new MezonServiceConfig
            {
                ServiceName = serviceName,
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                RedirectUri = _redirectUri
            };
        }
    }
}
