using Abp.Configuration;
using Amazon.Util.Internal;
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
using Timesheet.Constants;
using Timesheet.Services.Komu.Dto;
using Timesheet.Services.Project.Dto;
using Timesheet.Uitls;

namespace Timesheet.Services.Komu
{
    public class KomuService
    {
        private ILogger<KomuService> logger;
        private HttpClient httpClient;
        private readonly string _channelIdDevMode;
        private readonly string _userNameDevMode;
        private readonly string _channelIdDevModeConfig;
        private readonly string _userNameDevModeConfig;
        private readonly string _isNotifyToKomu;
        private int MAX_LENGTH = 2000;

        public KomuService(HttpClient httpClient, ILogger<KomuService> logger, IConfiguration configuration, ISettingManager settingManager)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            _channelIdDevModeConfig = settingManager.GetSettingValueForApplication(AppSettingNames.KomuChannelIdDevMode);
            _userNameDevModeConfig = settingManager.GetSettingValueForApplication(AppSettingNames.KomuUserNameDevMode);
            _channelIdDevMode = configuration.GetValue<string>("KomuService:DevModeChannelId");
            _userNameDevMode = configuration.GetValue<string>("KomuService:DevModeUserName");
            _isNotifyToKomu = configuration.GetValue<string>("KomuService:EnableKomuNotification");
            var baseAddress = configuration.GetValue<string>("KomuService:BaseAddress");
            var secretCode = configuration.GetValue<string>("KomuService:SecurityCode");

            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("X-Secret-Key", secretCode);
        }
        public async Task<ulong?> GetKomuUserId(string userName)
        {
            var komuUser = await PostAsync<KomuUserDto>(KomuUrlConstant.KOMU_USERID, new { username = userName });
            if (komuUser != null)
                return komuUser.KomuUserId;

            return default;

        }

        public void NotifyToChannel(string komuMessage, string channelId)
        {

            if (_isNotifyToKomu != "true")
            {
                logger.LogInformation("_isNotifyToKomu=" + _isNotifyToKomu + " => stop");
                return;
            }

            var channelIdToSend = !string.IsNullOrEmpty(_channelIdDevModeConfig) ? _channelIdDevModeConfig : _channelIdDevMode;
            string messageToSend = komuMessage;

            if (string.IsNullOrEmpty(channelIdToSend))
            {
                channelIdToSend = channelId;
            }
            else
            {
                messageToSend = "[DEV-MODE] " + komuMessage;
            }

            if (komuMessage.Contains("\n"))
            {
                var listMessage = CommonUtils.SeparateMessage(messageToSend, MAX_LENGTH, "\n");
                foreach(var message in listMessage)
                {
                    Post(KomuUrlConstant.KOMU_CHANNELID, new { message = message, channelid = channelIdToSend });
                }
            }
            else if (komuMessage.Contains(" "))
            {
                var listMessage = CommonUtils.SeparateMessage(messageToSend, MAX_LENGTH, " ");
                foreach (var message in listMessage)
                {
                    Post(KomuUrlConstant.KOMU_CHANNELID, new { message = message, channelid = channelIdToSend });
                }
            }
            else
            {
                Post(KomuUrlConstant.KOMU_CHANNELID, new { message = messageToSend, channelid = channelIdToSend });
            }

        }

        public void NotifyToChannel(string[] arrMessage, string channelId)
        {

            if (_isNotifyToKomu != "true")
            {
                logger.LogInformation("_isNotifyToKomu=" + _isNotifyToKomu + " => stop");
                return;
            }

            var channelIdToSend = !string.IsNullOrEmpty(_channelIdDevModeConfig) ? _channelIdDevModeConfig : _channelIdDevMode;

            if (string.IsNullOrEmpty(channelIdToSend))
            {
                channelIdToSend = channelId;
            }

            var listMessage = CommonUtils.SeparateMessage(arrMessage, MAX_LENGTH, "\n");
            foreach (var message in listMessage)
            {
                Post(KomuUrlConstant.KOMU_CHANNELID, new { message = string.Format("{0}{1}", !string.IsNullOrEmpty(channelIdToSend) ? "[DEV-MODE] ":"",message), channelid = channelIdToSend });
            }
        }

        public void Post(string url, object input)
        {
            var fullUrl = $"{this.httpClient.BaseAddress}/{url}";
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
            string strInput = JsonConvert.SerializeObject(input);
            var fullUrl = $"{this.httpClient.BaseAddress}/{url}";
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

        public async Task NotifyToChannelAwait(string[] arrMessage, string channelId)
        {

            if (_isNotifyToKomu != "true")
            {
                logger.LogInformation("_isNotifyToKomu=" + _isNotifyToKomu + " => stop");
                return;
            }

            var channelIdToSend = !string.IsNullOrEmpty(_channelIdDevModeConfig) ? _channelIdDevModeConfig : _channelIdDevMode;

            if (string.IsNullOrEmpty(channelIdToSend))
            {
                channelIdToSend = channelId;
            }

            var listMessage = CommonUtils.SeparateMessage(arrMessage, MAX_LENGTH, "\n");
            foreach (var message in listMessage)
            {
                await PostAsync<dynamic>(KomuUrlConstant.KOMU_CHANNELID, new { message = string.Format("{0}{1}", !string.IsNullOrEmpty(channelIdToSend) ? "[DEV-MODE] " : "", message), channelid = channelIdToSend });
            }
        }

        public void SendMessageToUser(string komuMessage, string userName)
        {

            if (_isNotifyToKomu != "true")
            {
                logger.LogInformation("_isNotifyToKomu=" + _isNotifyToKomu + " => stop");
                return;
            }

            var userNameToSend = !string.IsNullOrEmpty(_userNameDevModeConfig) ? _userNameDevModeConfig : _userNameDevMode;
            string messageToSend = komuMessage;

            if (string.IsNullOrEmpty(userNameToSend))
            {
                userNameToSend = userName;
            }
            else
            {
                messageToSend = "[DEV-MODE] " + komuMessage;
            }

            if (userNameToSend.Equals("UNKNOW-USER"))
            {
                //Nothing todo: [DEV_MODE] not config userName
                return;
            }

            if (komuMessage.Contains("\n"))
            {
                var listMessage = CommonUtils.SeparateMessage(messageToSend, MAX_LENGTH, "\n");
                foreach (var message in listMessage)
                {
                  Post(KomuUrlConstant.KOMU_USER_ONLY, new KomuSendMessageToUserDto { message = message, username = userNameToSend });
                }
            }
            else if (komuMessage.Contains(" "))
            {
                var listMessage = CommonUtils.SeparateMessage(messageToSend, MAX_LENGTH, " ");
                foreach (var message in listMessage)
                {
                    Post(KomuUrlConstant.KOMU_USER_ONLY, new KomuSendMessageToUserDto { message = message, username = userNameToSend });
                }
            }
            else
            {
                Post(KomuUrlConstant.KOMU_USER_ONLY, new KomuSendMessageToUserDto { message = messageToSend, username = userNameToSend });
            }
        }
    }
}
