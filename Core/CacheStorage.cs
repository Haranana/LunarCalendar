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
    public class CacheStorage
    {
        private WeeklyCacheData weeklyField;
        private InstantCacheData instantField;
        private LocationCacheData locationField = new LocationCacheData();

        public void InvalidateInstant() => SetInstant(null);
        public void InvalidateWeekly() => SetWeekly(null);
        public WeeklyCacheData WeeklyCacheData => Volatile.Read(ref weeklyField);
        public InstantCacheData InstantCacheData => Volatile.Read(ref instantField);
        public LocationCacheData LocationCacheData => Volatile.Read(ref locationField);

        public void SetWeekly(WeeklyCacheData newWeekly)
            => Interlocked.Exchange(ref weeklyField, newWeekly);

        public void SetInstant(InstantCacheData newInstant)
            => Interlocked.Exchange(ref instantField, newInstant);

        public void SetLocation(LocationCacheData newLocation)
            => Interlocked.Exchange(ref locationField, newLocation);

        private const int weeklyCacheExpectedSize = 7;

        public void RefreshWeeklyData(AstronomyTimeSeriesDto dto)
        {
            if (dto == null || dto.Astronomy == null || dto.Astronomy.Length != weeklyCacheExpectedSize) throw new Exception("Incorrect Astronomy dto data");
            SetWeekly(AstronomyMapper.MapToWeeklyCacheData(dto.Astronomy, DateTimeOffset.UtcNow, LocationCacheData.UserTimeZoneInfo));
        }

        public void RefreshInstantData(AstronomyDto dto)
        {
            if (dto == null ) throw new Exception("Incorrect Astronomy dto data");
            SetInstant(AstronomyMapper.MapToInstantCacheData(dto, DateTimeOffset.UtcNow,  LocationCacheData.UserTimeZoneInfo));
        }

        public bool IsInstantFresh(TimeSpan maxTtl)
        {
            return InstantCacheData != null
                && (DateTimeOffset.Now - InstantCacheData.LastUpdateTime) < maxTtl
                && (DateTimeOffset.UtcNow.Date == InstantCacheData.LastUpdateTime.Date);
        }

        public bool IsWeeklyFresh()
        {
            return (WeeklyCacheData != null) && (DateTimeOffset.UtcNow.Date == WeeklyCacheData.LastUpdateTime.Date);
        }

    }

    //data regarding interval of yesterday, today and +5 days at noon, needs to be updated daily
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


    public class InstantCacheData
    {
        public DateTimeOffset LastUpdateTime { get; set; }

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

    public class MoonPhaseTimeDto
    {
        public MoonPhases Phase { get; set; }
        public DateTimeOffset TimeUtc { get; set; }
    }

    //Data regarding location, should be changed only on location change by user or when empty
    //by default it points to Warsaw
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
