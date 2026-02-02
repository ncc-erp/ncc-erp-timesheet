using Abp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ncc.IoC;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Timesheet.DomainServices;
using Timesheet.Services.W2.Dto;

namespace Timesheet.Services.W2
{
    public class W2Service : BaseDomainService, IW2Service
    {
        private ILogger<W2Service> logger;
        private readonly string baseAddress;
        private readonly string securityCode;
        public W2Service(ILogger<W2Service> logger, IWorkScope workScope, IConfiguration configuration) : base(workScope)
        {
            this.logger = logger;

            baseAddress = configuration.GetValue<string>("W2Service:BaseAddress");
            securityCode = configuration.GetValue<string>("W2Service:SecurityCode");

        }
        public virtual WorkFromHomeRequestDto GetWfhRequest(string email, string date)
        {
            try
            {
                // Bypass the certificate
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                //Gọi api bên W2
                using (var client = new HttpClient(clientHandler))
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-Secret-Key", securityCode);

                    var response = client.GetAsync("api/app/workflow-instance/wfh-status?email=" + email + "&date=" + date).Result;
                    logger.LogInformation($"GetWfhRequest response: {response}");

                    if (response.IsSuccessStatusCode)
                    {
                        //Convert data
                        var resultString = response.Content.ReadAsStringAsync().Result;
                        logger.LogInformation($"GetWfhRequest response content: {resultString}");
                        return JsonConvert.DeserializeObject<WorkFromHomeRequestDto>(resultString);
                    } else
                    {
                        logger.LogError("GetWfhRequest return a failed response");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"GetWfhRequest Error: {ex.Message}");
            }
            return default;
        }

        public virtual System.Collections.Generic.List<W2RequestStatusDto> GetRequestStatus(string email, string date, string mezonId)
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                using (var client = new HttpClient(clientHandler))
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-Secret-Key", securityCode);

                    var url = $"api/app/workflow-instance/request-status?mezonId={mezonId}&email={email}&date={date}";
                    var response = client.GetAsync(url).Result;
                    logger.LogInformation($"GetRequestStatus response: {response}");

                    if (response.IsSuccessStatusCode)
                    {
                        var resultString = response.Content.ReadAsStringAsync().Result;
                        logger.LogInformation($"GetRequestStatus response content: {resultString}");
                        return JsonConvert.DeserializeObject<System.Collections.Generic.List<W2RequestStatusDto>>(resultString);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"GetRequestStatus Error: {ex.Message}");
            }
            return default;
        }
    }
}
