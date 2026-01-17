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
    public interface IIpGeoClient
    {
        Task<AstronomyTimeSeriesDto> GetAstronomyRangeAsync(double lat, double lon, DateTimeOffset beg, DateTimeOffset end, string ianaId);
        Task<AstronomyResponseDto> GetAstronomyAsync(double lat, double lon, string ianaId, DateTime? date = null);
    }

    /// <summary>
    /// Dedicated class for communication with IpGeolocation API
    /// </summary>
    public class IpGeoClient : IIpGeoClient
    {

        private readonly HttpClient HttpClient;
        private readonly string ApiKey;
        private readonly string ApiUrl;
        private readonly ILogger loggingService;

        public IpGeoClient(HttpClient httpClient , ILogger loggingService)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException("HttpClient is null");
            this.loggingService = loggingService;

            NameValueCollection AllAppSettings = ConfigurationManager.AppSettings;           
            ApiUrl = AllAppSettings["IPGEO_URL"];
            ApiKey = Environment.GetEnvironmentVariable("IPGEO_API_KEY");



            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                loggingService.WriteError("Ip Geo client couldn't be initialized, missing API KEY in env");
                throw new InvalidOperationException("missing API KEY environment variable");
            }

            if (string.IsNullOrWhiteSpace(ApiUrl))
            {
                loggingService.WriteError("Ip Geo client couldn't be initialized, config is missing API URL");
                throw new InvalidOperationException("config is missing API URL");
            }

            loggingService.WriteInfo("Ip Geo client initialized");
        }

        /// <summary>
        /// Fetches astronomy data from outer API for given coordinates, timeZone and each day in specified range
        /// </summary>
        /// <param name="lat">Geographical latitude in deegres (range -90..90)</param>
        /// <param name="lon">Geographical longitude in deegres (range: -180..180)</param>
        /// <param name="beg">First day for which data is fetched (shouldn't be later than end parameter )</param> 
        /// <param name="end">Last day for which data is fetched (shouldn't be earlier than beg parameter)</param>
        /// <param name="ianaId">ID of given time zone in IANA format (e.g. <c>Europe/Warsaw</c>)</param>
        /// <returns>Deserialized API response of type <see cref="AstronomyTimeSeriesDto"/></returns>
        /// <exception cref="HttpRequestException">
        /// If API return status is not success (e.g. 4xx/5xx)
        /// </exception>
        /// <exception cref="JsonException">
        /// If Response can't be deserialized to <see cref="AstronomyTimeSeriesDto"/>.
        /// </exception>
        /// <example>
        /// <code>
        /// var dto = await client.GetAstronomyRangeAsync(52.2297, 21.0122, DateTimeOffset.Now , DateTimeOffset.Now.AddDays(5) ,"Europe/Warsaw");
        /// </code>
        /// </example>
        public async Task<AstronomyTimeSeriesDto> GetAstronomyRangeAsync(double lat, double lon, DateTimeOffset beg, DateTimeOffset end , string ianaId)
        {

            string url = BuildUrlAstronomyTimeSeries(lat, lon, beg.DateTime, end.DateTime, ianaId);

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            using (HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {

                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    loggingService.WriteWarning($"API astronomy timeSeries request failed, error: {response.StatusCode}");
                    throw new HttpRequestException($"API error {response.StatusCode} : {responseBody}");

                }
                else
                {
                    loggingService.WriteInfo($"API astronomy timeSeries request successful");
                }

                    return JsonConvert.DeserializeObject<AstronomyTimeSeriesDto>(responseBody);
            }
        }

        /// <summary>
        /// Fetches astronomy data from outer API for given coordinates and timeZone
        /// </summary>
        /// <param name="lat">Geographical latitude in deegres (range -90..90)</param>
        /// <param name="lon">Geographical longitude in deegres (range: -180..180)</param>
        /// <param name="ianaId">ID of given time zone in IANA format (e.g. <c>Europe/Warsaw</c>)</param>
        /// <param name="date">
        /// Date for which astronomy data is fetched, if <c>null</c>, fetches data for current date
        /// (according to endpoint parameters and <paramref name="ianaId"/>).
        /// </param>
        /// <returns>Deserialized API response of type <see cref="AstronomyResponseDto"/></returns>
        /// <exception cref="HttpRequestException">
        /// If API return status is not success (e.g. 4xx/5xx)
        /// </exception>
        /// <exception cref="JsonException">
        /// If Response can't be deserialized to <see cref="AstronomyResponseDto"/>.
        /// </exception>
        /// <example>
        /// <code>
        /// var dto = await client.GetAstronomyAsync(52.2297, 21.0122, "Europe/Warsaw");
        /// </code>
        /// </example>
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
                    loggingService.WriteInfo($"API astronomy request successful");
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
