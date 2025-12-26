using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class IpGeoClient
    {

        private readonly HttpClient HttpClient;
        private readonly string ApiKey;
        private readonly string ApiUrl;
        private LoggingService loggingService;

        public IpGeoClient(HttpClient httpClient , LoggingService loggingService)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException("HttpClient is null");
            this.loggingService = loggingService;

            NameValueCollection AllAppSettings = ConfigurationManager.AppSettings;           
            ApiUrl = AllAppSettings["IPGEO_URL"];
            ApiKey = Environment.GetEnvironmentVariable("IPGEO_API_KEY");

            loggingService.WriteInfo("IPGEO_API_KEY present=" + (!string.IsNullOrWhiteSpace(ApiKey)));

            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException("missing API KEY environment variable");

            if (string.IsNullOrWhiteSpace(ApiUrl))
                throw new InvalidOperationException("config is missing API URL");
        }

        public async Task<AstronomyTimeSeriesDto> getAstronomyWeekAsync(double lat, double lon, DateTimeOffset beg, DateTimeOffset end , string ianaId)
        {

            string url = BuildUrlAstronomyTimeSeries(lat, lon, beg.DateTime, end.DateTime, ianaId);

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            using (HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {

                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    loggingService.WriteWarning($"API astronomy timeSeries request failed, error: {response.StatusCode}. lat={lat}, lon={lon}, tz={ianaId}");
                    throw new HttpRequestException($"API error {response.StatusCode} : {responseBody}");

                }
                else
                {
                    var bodyToWrite = responseBody.Length > 5000 ? responseBody.Substring(0, 5000) : responseBody;
                    loggingService.WriteInfo($"API call to url: {url}{Environment.NewLine}API returned: {response.StatusCode}{Environment.NewLine}{bodyToWrite}");
                }

                    return JsonConvert.DeserializeObject<AstronomyTimeSeriesDto>(responseBody);
            }
        }


        public async Task<AstronomyResponseDto> GetAstronomyAsync(
            double lat, double lon, string ianaId, DateTime? date = null
        ){
            string url = BuildUrlAstronomy(lat, lon, ianaId, date);

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            using (HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {

                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    loggingService.WriteWarning($"API astronomy request failed, error: {response.StatusCode}. lat={lat}, lon={lon}, tz={ianaId}");
                    throw new HttpRequestException($"API error {response.StatusCode} : {responseBody}");
                }
                else
                {
                    var bodyToWrite = responseBody.Length > 1000 ? responseBody.Substring(0, 1000) : responseBody;
                    loggingService.WriteInfo($"API call to url: {url}{Environment.NewLine}API returned: {response.StatusCode}{Environment.NewLine}{bodyToWrite}");
                }

                return JsonConvert.DeserializeObject<AstronomyResponseDto>(responseBody);
            }
        }

        private string BuildUrlAstronomy(double lat, double lon, string ianaId, DateTime? date)
        {
            string url = $"{ApiUrl}/v2/astronomy?apiKey={Uri.EscapeDataString(ApiKey)}" +
                $"&lat={Uri.EscapeDataString(lat.ToString(CultureInfo.InvariantCulture))}&long={Uri.EscapeDataString(lon.ToString(CultureInfo.InvariantCulture))}&time_zone={Uri.EscapeDataString(ianaId)}";

            if (date.HasValue) url += $"&date={date.Value:yyyy-MM-dd}";

            return url;
        }

        private string BuildUrlAstronomyTimeSeries(double lat, double lon, DateTime beg, DateTime end, string ianaId)
        {
            string url = $"{ApiUrl}/v2/astronomy/timeSeries?apiKey={Uri.EscapeDataString(ApiKey)}"
                + $"&lat={Uri.EscapeDataString(lat.ToString(CultureInfo.InvariantCulture))}&long={Uri.EscapeDataString(lon.ToString(CultureInfo.InvariantCulture))}"
                + $"&dateStart={Uri.EscapeDataString(beg.ToString("yyyy-MM-dd"))}&dateEnd={Uri.EscapeDataString(end.ToString("yyyy-MM-dd"))}&time_zone={Uri.EscapeDataString(ianaId)}";

            return url;
        }
    }
}
