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

namespace Timesheet.Services.Mezon
{
    public class MezonService : IMezonService
    {
        private readonly ILogger<MezonService> logger;
        private readonly ISettingManager _settingManager;
        private HttpClient _httpClient;
        private const string serviceName = "MezonService";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        public MezonService(HttpClient httpClient, ISettingManager settingManager, ILogger<MezonService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this._settingManager = settingManager;
            this._httpClient = httpClient;
            _clientId = configuration.GetValue<string>($"{serviceName}:ClientId");
            _clientSecret = configuration.GetValue<string>($"{serviceName}:ClientSecret");
            _redirectUri = configuration.GetValue<string>($"{serviceName}:RedirectUri");
            //var baseAddress = configuration.GetValue<string>($"{serviceName}:BaseAddress");
            var baseAddress = "http://172.16.100.114:3000";
            httpClient.BaseAddress = new Uri(baseAddress);
        }
        // get token
        private string GetAccessToken()
        {
            string loginUrl = "api/TokenAuth/Authenticate"; 
            try
            {
                var loginData = new
                {
                    username = "admin",
                    password = "123qwe",
                    tenantId = 1 
                };
                var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                using (var client = new HttpClient(clientHandler))
                {
                    client.BaseAddress = new Uri("http://localhost:21023");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    const string securityCode = "12345678";
                    client.DefaultRequestHeaders.Add("Security-Code", securityCode);
                    logger.LogInformation($"Added Security-Code for login: {securityCode}");

                    var response = client.PostAsync(loginUrl, content).Result;
                    logger.LogInformation($"Login response status: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        logger.LogInformation($"Login successful: Response = {responseContent}");
                        var loginResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (loginResult?.result?.accessToken != null)
                        {
                            string token = loginResult.result.accessToken.ToString();
                            logger.LogInformation($"Retrieved accessToken: {token.Substring(0, 10)}..."); 
                            return token;
                        }
                        else
                        {
                            logger.LogWarning($"Response does not contain 'result.accessToken': {responseContent}");
                            return null;
                        }
                    }
                    else
                    {
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        logger.LogWarning($"Login failed with status code: {response.StatusCode}, Error: {errorContent}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"GetAccessToken() Error: {ex.Message}");
                return null;
            }
        }
        public OpenTalkListDto[] GetOpenTalkLog(DateTime? day = null)
        {
            try
            {
                // Thử endpoint getAllOpentalkTime trên http://172.16.100.114:3000
                string url = "getAllOpentalkTime";
                if (day.HasValue)
                {
                    url += $"?date={day.Value.ToString("dd/MM/yyyy")}";
                }

                var originalBaseAddress = _httpClient.BaseAddress;
                _httpClient.BaseAddress = new Uri("http://172.16.100.114:3000");
                logger.LogInformation($"Temporarily set BaseAddress to http://172.16.100.114:3000 for {url}");

                var result = Get<OpenTalkListDto[]>(url);
                if (result != null)
                {
                    _httpClient.BaseAddress = originalBaseAddress;
                    logger.LogInformation($"Successfully retrieved data from getAllOpentalkTime");
                    return result;
                }

                // Nếu không thành công, thử endpoint createOpentalkLog trên http://localhost:21023
                logger.LogInformation($"getAllOpentalkTime failed, trying createOpentalkLog on http://localhost:21023");
                url = "api/services/app/MezonSetting/createOpentalkLog";
                if (day.HasValue)
                {
                    url += $"?date={day.Value.ToString("yyyy/MM/dd")}";
                }

                _httpClient.BaseAddress = new Uri("http://localhost:21023");
                logger.LogInformation($"Set BaseAddress to http://localhost:21023 for {url}");

                result = Get<OpenTalkListDto[]>(url);
                if (result == null)
                {
                    logger.LogWarning($"GetOpenTalkLog failed for {url}: No data returned.");
                }

                _httpClient.BaseAddress = originalBaseAddress;
                logger.LogInformation($"Restored BaseAddress to {originalBaseAddress}");
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError($"GetOpenTalkLog failed: {ex.Message}");
                return default;
            }
        }
        //public OpenTalkListDto[] GetOpenTalkLog(DateTime? day = null)
        //{
        //    string url;
        //    if (day.HasValue)
        //    {
        //        url = "api/GetAllTotalTimeJoinOpentalkDay?time=" + day.Value.ToString("yyyy'/'MM'/'dd");
        //    } else
        //    {
        //        url = "api/GetAllTotalTimeJoinOpentalkDay";
        //    }
        //    return Get<OpenTalkListDto[]>(url);
        //}
        //public T Get<T>(string url)
        //{
        //    try
        //    {
        //        // Bypass the certificate
        //        HttpClientHandler clientHandler = new HttpClientHandler();
        //        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        //        //Gọi api bên Mezon
        //        using (var client = new HttpClient(clientHandler))
        //        {

        //            client.BaseAddress = new Uri("http://172.16.100.114:3000");
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            client.DefaultRequestHeaders.Add("Security-Code", _settingManager.GetSettingValueForApplication(AppSettingNames.MezonSecurityCode));

        //            var response = client.GetAsync(url).Result;

        //            if (response.IsSuccessStatusCode)
        //            {
        //                //Convert data
        //                var responseContent = response.Content.ReadAsStringAsync().Result;
        //                logger.LogInformation($"GET {client.BaseAddress}{url} => Response: {responseContent}");
        //                JArray responseJObj = JArray.Parse(responseContent);
        //                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(responseJObj));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"GetOpenTalkLog() Error: {ex.Message}");
        //    }

        //    return default;

        //}
        public T Get<T>(string url)
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                using (var client = new HttpClient(clientHandler))
                {
                    client.BaseAddress = _httpClient.BaseAddress;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    const string securityCode = "12345678";
                    client.DefaultRequestHeaders.Add("Security-Code", securityCode);
                    logger.LogInformation($"Added Security-Code: {securityCode}");

                    if (url.Contains("api/services/app/MezonSetting/createOpentalkLog"))
                    {
                        var token = GetAccessToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            logger.LogInformation($"Added Bearer Token for {url}: {token.Substring(0, 10)}...");
                        }
                        else
                        {
                            logger.LogWarning("No access token available for {url}.");
                        }
                    }

                    var response = client.GetAsync(url).Result;
                    logger.LogInformation($"Response status code: {response.StatusCode} for URL: {url}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        logger.LogInformation($"GET {client.BaseAddress}{url} => Response: {responseContent}");

                        if (typeof(T) == typeof(OpenTalkListDto[]))
                        {
                            // Kiểm tra xem phản hồi có phải là mảng JSON không
                            if (responseContent.Trim().StartsWith("["))
                            {
                                // Phản hồi là mảng JSON, deserialize trực tiếp thành OpenTalkListDto[]
                                return JsonConvert.DeserializeObject<T>(responseContent);
                            }
                            else
                            {
                                // Phản hồi là đối tượng JSON, kiểm tra thuộc tính 'result'
                                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                                if (jsonResponse?.result != null)
                                {
                                    return JsonConvert.DeserializeObject<T>(jsonResponse.result.ToString());
                                }
                                else
                                {
                                    logger.LogWarning($"Response does not contain 'result' field: {responseContent}");
                                    return default;
                                }
                            }
                        }

                        return JsonConvert.DeserializeObject<T>(responseContent);
                    }
                    else
                    {
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        logger.LogWarning($"GET {client.BaseAddress}{url} failed with status code: {response.StatusCode}, Error: {errorContent}");
                        return default;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Get() Error: {url}, Message: {ex.Message}");
                return default;
            }
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
