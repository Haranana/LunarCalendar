using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Contracts
{
    public static class ContractMapper
    {

        private static DateTimeOffset? ToTz(DateTimeOffset? utc, TimeZoneInfo tz) => utc.HasValue ? TimeZoneInfo.ConvertTime(utc.Value, tz) : utc;

        private static DateTimeOffset ToTz(DateTimeOffset utc, TimeZoneInfo tz) => TimeZoneInfo.ConvertTime(utc, tz);
        public static WeeklyCacheDataContract WeeklyCacheToContract(WeeklyCacheData dto, TimeZoneInfo tz)
        {
            var newDaily = new List<DailyCacheDataContract>();
            foreach(var e in dto.DailyCacheDatas){
                newDaily.Add(DailyCacheToContract(e, tz));
            }
            return new WeeklyCacheDataContract
            {
                DailyCacheDatas = newDaily,
            };
        }

        public static DailyCacheDataContract DailyCacheToContract(DailyCacheData dto, TimeZoneInfo tz)
        {
            return new DailyCacheDataContract
            {
                LocalDate = dto.LocalDate,
                SunAltitude = dto.SunAltitude,
                SunDistance = dto.SunDistance,
                SunAzimuth = dto.SunAzimuth,
                DayLength = dto.DayLength,

                Sunrise = ToTz(dto.Sunrise, tz),
                Sunset = ToTz(dto.Sunset, tz),

                MorningAstronomicalTwilightBegin = ToTz(dto.MorningAstronomicalTwilightBegin, tz),
                MorningAstronomicalTwilightEnd = ToTz(dto.MorningAstronomicalTwilightEnd, tz),
                MorningNauticalTwilightBegin = ToTz(dto.MorningNauticalTwilightBegin, tz),
                MorningNauticalTwilightEnd = ToTz(dto.MorningNauticalTwilightEnd, tz),
                MorningCivilTwilightBegin = ToTz(dto.MorningCivilTwilightBegin, tz),
                MorningCivilTwilightEnd = ToTz(dto.MorningCivilTwilightEnd, tz),
                MorningBlueHourBegin = ToTz(dto.MorningBlueHourBegin, tz),
                MorningBlueHourEnd = ToTz(dto.MorningBlueHourEnd, tz),
                MorningGoldenHourBegin = ToTz(dto.MorningGoldenHourBegin, tz),
                MorningGoldenHourEnd = ToTz(dto.MorningGoldenHourEnd, tz),

                MoonIlluminationPercentage = dto.MoonIlluminationPercentage,
                MoonAltitude = dto.MoonAltitude,
                MoonDistance = dto.MoonDistance,
                MoonAzimuth = dto.MoonAzimuth,
                MoonParallacticAngle = dto.MoonParallacticAngle,
                NightLength = dto.NightLength,
                LunarAge = dto.LunarAge,
                MoonPhase = dto.MoonPhase,

                Moonrise = ToTz(dto.Moonrise, tz),
                Moonset = ToTz(dto.Moonset, tz),

                EveningAstronomicalTwilightBegin = ToTz(dto.EveningAstronomicalTwilightBegin, tz),
                EveningAstronomicalTwilightEnd = ToTz(dto.EveningAstronomicalTwilightEnd, tz),
                EveningNauticalTwilightBegin = ToTz(dto.EveningNauticalTwilightBegin, tz),
                EveningNauticalTwilightEnd = ToTz(dto.EveningNauticalTwilightEnd, tz),
                EveningCivilTwilightBegin = ToTz(dto.EveningCivilTwilightBegin, tz),
                EveningCivilTwilightEnd = ToTz(dto.EveningCivilTwilightEnd, tz),
                EveningBlueHourBegin = ToTz(dto.EveningBlueHourBegin, tz),
                EveningBlueHourEnd = ToTz(dto.EveningBlueHourEnd, tz),
                EveningGoldenHourBegin = ToTz(dto.EveningGoldenHourBegin, tz),
                EveningGoldenHourEnd = ToTz(dto.EveningGoldenHourEnd, tz),
            };
        }

        public static InstantCacheDataContract InstantCacheToContract(InstantCacheData dto, TimeZoneInfo tz)
        {
            return new InstantCacheDataContract
            { 
                Date = dto.Date,
                CurrentTime = dto.CurrentTime,

                SunAltitude = dto.SunAltitude,
                SunDistance = dto.SunDistance,
                SunAzimuth = dto.SunAzimuth,
                MoonIlluminationPercentage  = dto.MoonIlluminationPercentage,
                MoonAltitude = dto.MoonAltitude,
                MoonDistance = dto.MoonDistance,
                MoonAzimuth = dto.MoonAzimuth,
                MoonParallacticAngle = dto.MoonParallacticAngle,
                LunarAge = dto.LunarAge,
                MoonPhase = dto.MoonPhase,
                NextMoonPhases = dto.NextMoonPhases.Select(e=>new MoonPhaseTimeContract {Phase = e.Phase, TimeUtc = ToTz( e.TimeUtc, tz )}).ToList(),                
            };
        }

        public static LocationCacheDataContract LocationCacheToContract(LocationCacheData dto)
        {

            return new LocationCacheDataContract
            {
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                City = dto.City,
                CountryName = dto.CountryName,
                IanaTimeZoneId= dto.IanaTimeZoneId,
            };
        }
    }

    [ServiceContract]
    public interface IAstronomyService
    {
        [OperationContract]
        LocationCacheDataContract GetLocationData();

        [OperationContract]
        Task UpdateLocationData(double lat, double lon);

        [OperationContract]
        Task<InstantCacheDataContract> GetFreshInstant();

        [OperationContract]
        Task<WeeklyCacheDataContract> GetFreshWeekly();

    }

    [DataContract]
    public class WeeklyCacheDataContract
    {
        [DataMember]
        public List<DailyCacheDataContract> DailyCacheDatas { get; set; }

    }

    [DataContract]
    public class DailyCacheDataContract
    {
        [DataMember]
        public DateTime LocalDate { get; set; }

        /* SunAltitude, SunDistance, SunAzimuth at noon local time */
        [DataMember]
        public double SunAltitude { get; set; }
        [DataMember]
        public double SunDistance { get; set; }
        [DataMember]
        public double SunAzimuth { get; set; }
        [DataMember]
        public TimeSpan DayLength { get; set; }
        [DataMember]
        public DateTimeOffset? Sunrise { get; set; }
        [DataMember]
        public DateTimeOffset? Sunset { get; set; }

        [DataMember]
        public DateTimeOffset? MorningAstronomicalTwilightBegin { get; set; }
        [DataMember]
        public DateTimeOffset? MorningAstronomicalTwilightEnd { get; set; }
        [DataMember]
        public DateTimeOffset? MorningNauticalTwilightBegin { get; set; }
        [DataMember]
        public DateTimeOffset? MorningNauticalTwilightEnd { get; set; }
        [DataMember]
        public DateTimeOffset? MorningCivilTwilightBegin { get; set; }
        [DataMember]
        public DateTimeOffset? MorningCivilTwilightEnd { get; set; }
        [DataMember]
        public DateTimeOffset? MorningBlueHourBegin { get; set; }
        [DataMember]
        public DateTimeOffset? MorningBlueHourEnd { get; set; }
        [DataMember]
        public DateTimeOffset? MorningGoldenHourBegin { get; set; }
        [DataMember]
        public DateTimeOffset? MorningGoldenHourEnd { get; set; }


        /* MoonAltitude, MoonDistance, MoonAzimuth, MoonParallacticAngle, MoonIlluminationPercentage at noon user defined time */
        [DataMember]
        public double MoonAltitude { get; set; }
        [DataMember]
        public double MoonDistance { get; set; }
        [DataMember]
        public double MoonAzimuth { get; set; }
        [DataMember]
        public double MoonParallacticAngle { get; set; }
        [DataMember]
        public double MoonIlluminationPercentage { get; set; }
        [DataMember]
        public TimeSpan NightLength { get; set; }
        [DataMember]
        public double LunarAge { get; set; }
        [DataMember]
        public MoonPhases MoonPhase { get; set; }
        [DataMember]
        public DateTimeOffset? Moonrise { get; set; }
        [DataMember]
        public DateTimeOffset? Moonset { get; set; }

        [DataMember]
        public DateTimeOffset? EveningAstronomicalTwilightBegin { get; set; }
        [DataMember]
        public DateTimeOffset? EveningAstronomicalTwilightEnd { get; set; }
        [DataMember]
        public DateTimeOffset? EveningNauticalTwilightBegin { get; set; }
        [DataMember]
        public DateTimeOffset? EveningNauticalTwilightEnd { get; set; }
        [DataMember]
        public DateTimeOffset? EveningCivilTwilightBegin { get; set; }
        [DataMember]
        public DateTimeOffset? EveningCivilTwilightEnd { get; set; }
        [DataMember]
        public DateTimeOffset? EveningBlueHourBegin { get; set; }
        [DataMember]
        public DateTimeOffset? EveningBlueHourEnd { get; set; }
        [DataMember]
        public DateTimeOffset? EveningGoldenHourBegin { get; set; }
        [DataMember]
        public DateTimeOffset? EveningGoldenHourEnd { get; set; }

    }

    [DataContract]
    public class InstantCacheDataContract
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public TimeSpan CurrentTime { get; set; }
        [DataMember]
        public double SunAltitude { get; set; }
        [DataMember]
        public double SunDistance { get; set; }
        [DataMember]
        public double SunAzimuth { get; set; }

        [DataMember]
        public double MoonIlluminationPercentage { get; set; }
        [DataMember]
        public double MoonAltitude { get; set; }
        [DataMember]
        public double MoonDistance { get; set; }
        [DataMember]
        public double MoonAzimuth { get; set; }
        [DataMember]
        public double MoonParallacticAngle { get; set; }
        [DataMember]
        public double LunarAge { get; set; }

        [DataMember]
        public MoonPhases MoonPhase { get; set; }
        [DataMember]
        public List<MoonPhaseTimeContract> NextMoonPhases { get; set; }
    }

    [DataContract]
    public class MoonPhaseTimeContract
    {
        [DataMember]
        public MoonPhases Phase { get; set; }
        [DataMember]
        public DateTimeOffset TimeUtc { get; set; }
    }

    [DataContract]
    public class LocationCacheDataContract
    {
        [DataMember]
        public double Latitude { get; set; }
        [DataMember]
        public double Longitude { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string CountryName { get; set; }
        [DataMember]
        public string IanaTimeZoneId { get; set; }
    }


}
