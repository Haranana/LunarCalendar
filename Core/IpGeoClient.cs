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

        public IpGeoClient(HttpClient httpClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException("HttpClient is null");

            NameValueCollection AllAppSettings = ConfigurationManager.AppSettings;           
            ApiUrl = AllAppSettings["IPGEO_URL"];
            ApiKey = Environment.GetEnvironmentVariable("IPGEO_API_KEY");

            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException("App.config is missing API URL");

            if (string.IsNullOrWhiteSpace(ApiUrl))
                throw new InvalidOperationException("missing API KEY environment variable");
        }

        public async Task<AstronomyResponseDto> GetAstronomyAsync(
            double lat, double lon, DateTime? date = null
        ){
            string url = BuildUrl(lat, lon, date);

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            using (HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
            {

                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"API error {response.StatusCode} : {responseBody}");

                return JsonConvert.DeserializeObject<AstronomyResponseDto>(responseBody);
            }
        }

        private string BuildUrl(double lat, double lon, DateTime? date)
        {
            string url = $"{ApiUrl}/v2/astronomy?apiKey={Uri.EscapeDataString(ApiKey)}&lat={Uri.EscapeDataString(lat.ToString())}&long={Uri.EscapeDataString(lon.ToString())}";

            if (date.HasValue) url += $"&date={date.Value:yyyy-MM-dd}";

            return url;
        }
    }
}
