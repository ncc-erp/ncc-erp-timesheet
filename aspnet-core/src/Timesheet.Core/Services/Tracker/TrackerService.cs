using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Services.HRM;
using static Timesheet.Services.HRM.Dto.UpdateToHrmDto;
using Timesheet.Services.Tracker.Dto;
using Microsoft.Office.Interop.Word;

namespace Timesheet.Services.Tracker
{
    public class TrackerService : ITrackerService
    {
        private HttpClient httpClient;
        private readonly ILogger<TrackerService> logger;

        public TrackerService(HttpClient httpClient, IConfiguration configuration, ILogger<TrackerService> logger)
        {
            this.logger = logger;
            this.httpClient = httpClient;

            var baseAddress = configuration.GetValue<string>("TrackerService:BaseAddress", "http://tracker.komu.vn:5600");
            var securityCode = configuration.GetValue<string>("TrackerService:SecurityCode" , "1BCD4F3799EE95C4");

            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("X-Secret-Key", securityCode);
        }

        public virtual List<GetSUerAndActiveTimeTrackerDto> GetTimeTrackerToDay(DateTime day, List<string> userNames)
        { 
            var url = "api/0/report?day="+day.ToString("yyyy/MM/dd");
            return  PostAsync<List<GetSUerAndActiveTimeTrackerDto>>(url, new GetUserTimeTrackerToDayDto { emails = userNames }).Result;
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

                var responseContent = await response.Content.ReadAsStringAsync();

                logger.LogInformation($"Post: {fullUrl} input: {strInput} response: {responseContent}");

                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception e)
            {
                logger.LogError($"Post: {fullUrl} input: {strInput} Error: {e.Message}");
            }

            return default;
        }
    }
}
