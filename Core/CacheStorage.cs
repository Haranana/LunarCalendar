using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    //all Data and times should be stored as UTC, and if needed then converted in UI services or by API client
    /// <summary>
    /// Class for storing Astronomy and Location data in order to minimize amount of API requests.
    /// All Dates and times should be stored in universal time
    /// </summary>
    public class CacheStorage
    {
        private const int weeklyCacheExpectedSize = 7;

        private WeeklyCacheData weeklyField;
        private InstantCacheData instantField;
        private LocationCacheData locationField = new LocationCacheData();
       
        public WeeklyCacheData WeeklyCacheData => Volatile.Read(ref weeklyField);
        public InstantCacheData InstantCacheData => Volatile.Read(ref instantField);
        public LocationCacheData LocationCacheData => Volatile.Read(ref locationField);

        public void SetWeekly(WeeklyCacheData newWeekly)
         => Interlocked.Exchange(ref weeklyField, newWeekly);

        public void SetInstant(InstantCacheData newInstant)
            => Interlocked.Exchange(ref instantField, newInstant);

        public void SetLocation(LocationCacheData newLocation)
            => Interlocked.Exchange(ref locationField, newLocation);

        /// <summary>
        /// Invalidates instant data, which forces services to refresh it before next usage
        /// </summary>
        public void InvalidateInstant() => SetInstant(null);

        /// <summary>
        /// Invalidates instant data, which forces services to refresh it before next usage
        /// </summary>
        public void InvalidateWeekly() => SetWeekly(null);


        /// <summary>
        /// Save <see cref="AstronomyTimeSeriesDto"/> object as weekly data
        /// </summary>
        /// <param name="dto">object to be stored in cache</param>
        /// <exception cref="Exception">
        /// If given parameter is in incorrect format (null or has fields with unexpected length)
        public void RefreshWeeklyData(AstronomyTimeSeriesDto dto)
        {
            if (dto == null || dto.Astronomy == null || dto.Astronomy.Length != weeklyCacheExpectedSize) throw new Exception("Incorrect Astronomy dto data");
            SetWeekly(AstronomyMapper.MapToWeeklyCacheData(dto.Astronomy, DateTimeOffset.UtcNow, LocationCacheData.UserTimeZoneInfo));
        }

        /// <summary>
        /// Save <see cref="AstronomyDto"/> object as instant data
        /// </summary>
        /// <param name="dto">object to be stored in cache</param>
        /// <exception cref="Exception">
        /// If given parameter is in incorrect format (null)
        public void RefreshInstantData(AstronomyDto dto)
        {
            if (dto == null ) throw new Exception("Incorrect Astronomy dto data");
            SetInstant(AstronomyMapper.MapToInstantCacheData(dto, DateTimeOffset.UtcNow,  LocationCacheData.UserTimeZoneInfo));
        }

        /// <summary>
        /// Checks if instant data saved in storage isn't expired or null
        /// </summary>
        /// <param name="maxTtl">Interval of time after last update after which data is considered expired</param>
        /// <returns>False if data is expired, true if it's not</returns>
        public bool IsInstantFresh(TimeSpan maxTtl)
        {
            return InstantCacheData != null
                && (DateTimeOffset.UtcNow - InstantCacheData.LastUpdateTime) < maxTtl
                && (DateTimeOffset.UtcNow.Date == InstantCacheData.LastUpdateTime.Date);
        }

        /// <summary>
        /// Checks if instant data saved in storage isn't expired or null
        /// </summary>
        /// <returns>False if data is expired, true if it's not</returns>
        public bool IsWeeklyFresh()
        {
            return (WeeklyCacheData != null) && (DateTimeOffset.UtcNow.Date == WeeklyCacheData.LastUpdateTime.Date);
        }

    }

    /// <summary>
    /// Astronomy data regarding interval of several days (week by default).
    /// Considered to be expired after one day
    /// </summary>
    public class WeeklyCacheData
    {
        public DateTimeOffset LastUpdateTime {get; set; }
        public List<DailyCacheData> DailyCacheDatas { get; set; }
    }

    public class DailyCacheData
    {
        public DateTime LocalDate { get; set; }

        public double SunAltitude { get; set; }
        public double SunDistance { get; set; }
        public double SunAzimuth { get; set; }
        public TimeSpan DayLength { get; set; }
        public DateTimeOffset? Sunrise { get; set; }
        public DateTimeOffset? Sunset { get; set; }

        public DateTimeOffset? MorningAstronomicalTwilightBegin { get; set; }
        public DateTimeOffset? MorningAstronomicalTwilightEnd { get; set; }
        public DateTimeOffset? MorningNauticalTwilightBegin { get; set; }
        public DateTimeOffset? MorningNauticalTwilightEnd { get; set; }
        public DateTimeOffset? MorningCivilTwilightBegin { get; set; }
        public DateTimeOffset? MorningCivilTwilightEnd { get; set; }
        public DateTimeOffset? MorningBlueHourBegin { get; set; }
        public DateTimeOffset? MorningBlueHourEnd { get; set; }
        public DateTimeOffset? MorningGoldenHourBegin { get; set; }
        public DateTimeOffset? MorningGoldenHourEnd { get; set; }

        public DateTimeOffset? Moonrise { get; set; }
        public DateTimeOffset? Moonset { get; set; }
        public double MoonAltitude { get; set; }
        public double MoonDistance { get; set; }
        public double MoonAzimuth { get; set; }
        public double MoonParallacticAngle { get; set; }
        public double MoonIlluminationPercentage { get; set; }
        public TimeSpan NightLength  { get; set; }
        public double LunarAge { get; set; }
        public MoonPhases MoonPhase { get; set; }

        public DateTimeOffset? EveningAstronomicalTwilightBegin { get; set; }
        public DateTimeOffset? EveningAstronomicalTwilightEnd { get; set; }
        public DateTimeOffset? EveningNauticalTwilightBegin { get; set; }
        public DateTimeOffset? EveningNauticalTwilightEnd { get; set; }
        public DateTimeOffset? EveningCivilTwilightBegin { get; set; }
        public DateTimeOffset? EveningCivilTwilightEnd { get; set; }
        public DateTimeOffset? EveningBlueHourBegin { get; set; }
        public DateTimeOffset? EveningBlueHourEnd { get; set; }
        public DateTimeOffset? EveningGoldenHourBegin { get; set; }
        public DateTimeOffset? EveningGoldenHourEnd { get; set; }

    }

    /// <summary>
    /// Astronomy data regarding current moment.
    /// </summary>
    public class InstantCacheData
    {
        public DateTimeOffset LastUpdateTime { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("current_time")]
        public TimeSpan CurrentTime { get; set; }

        public double SunAltitude { get; set; }
        public double SunDistance { get; set; }
        public double SunAzimuth { get; set; }

        public double MoonIlluminationPercentage { get; set; }
        public double MoonAltitude { get; set; }
        public double MoonDistance { get; set; }
        public double MoonAzimuth { get; set; }
        public double MoonParallacticAngle { get; set; }
        public double LunarAge { get; set; }
        
        public MoonPhases MoonPhase { get; set; }
        public List<MoonPhaseTimeDto> NextMoonPhases { get; set; }

    }

    /// <summary>
    /// Stores Moon Phase and time of its next occurence
    /// </summary>
    public class MoonPhaseTimeDto
    {
        public MoonPhases Phase { get; set; }
        public DateTimeOffset TimeUtc { get; set; }
    }

    /// <summary>
    /// Stores coordinates and data associated with them
    /// </summary>
    public class LocationCacheData
    {
        private const double defaultLat = 52.2298;
        private const double defaultLon = 21.0117;
        private const string defaultCity = "Warsaw";
        private const string defaultCountryName = "Poland";

        public DateTimeOffset LastUpdateTime { get; set; }

        //calculated based on lat and lon
        public TimeZoneInfo UserTimeZoneInfo { get; set; } = TimeZoneUtil.GetTimeZone(defaultLat, defaultLon);

        public string IanaTimeZoneId { get; set; } = "Europe/Warsaw";

        public double Latitude { get; set; } = defaultLat;

        public double Longitude { get; set; } = defaultLon;

        //returned by API
        public string City { get; set; } = defaultCity;

        public string CountryName { get; set; } = defaultCountryName;
    }
}
