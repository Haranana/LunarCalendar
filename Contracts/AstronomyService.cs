using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public static class ContractMapper
    {
        public static WeeklyCacheDataContract WeeklyCacheToContract(WeeklyCacheData dto)
        {
            var newDaily = new List<DailyCacheDataContract>();
            foreach(var e in dto.DailyCacheDatas){
                newDaily.Add(DailyCacheToContract(e));
            }
            return new WeeklyCacheDataContract
            {
                DailyCacheDatas = newDaily,
            };
        }

        public static DailyCacheDataContract DailyCacheToContract(DailyCacheData dto)
        {
            return new DailyCacheDataContract
            {
                LocalDate = dto.LocalDate,
                SunAltitude = dto.SunAltitude,
                SunDistance = dto.SunDistance,
                SunAzimuth = dto.SunAzimuth,
                DayLength = dto.DayLength,
                MorningAstronomicalTwilightBegin = dto.MorningAstronomicalTwilightBegin,
                MorningAstronomicalTwilightEnd = dto.MorningAstronomicalTwilightEnd,
                MorningNauticalTwilightBegin = dto.MorningNauticalTwilightBegin,
                MorningNauticalTwilightEnd = dto.MorningNauticalTwilightEnd,
                MorningCivilTwilightBegin = dto.MorningCivilTwilightBegin,
                MorningCivilTwilightEnd = dto.MorningCivilTwilightEnd,
                MorningBlueHourBegin = dto.MorningBlueHourBegin,
                MorningBlueHourEnd = dto.MorningBlueHourEnd,
                MorningGoldenHourBegin = dto.MorningGoldenHourBegin,
                MorningGoldenHourEnd = dto.MorningGoldenHourEnd,
                MoonIlluminationPercentage = dto.MoonIlluminationPercentage,
                MoonAltitude = dto.MoonAltitude,
                MoonDistance = dto.MoonDistance,
                MoonAzimuth = dto.MoonAzimuth,
                MoonParallacticAngle = dto.MoonParallacticAngle,
                NightLength = dto.NightLength,
                LunarAge = dto.LunarAge,
                MoonPhase = dto.MoonPhase,
                EveningAstronomicalTwilightBegin = dto.EveningAstronomicalTwilightBegin,
                EveningAstronomicalTwilightEnd = dto.EveningAstronomicalTwilightEnd,
                EveningNauticalTwilightBegin = dto.EveningNauticalTwilightBegin,
                EveningNauticalTwilightEnd = dto.EveningNauticalTwilightEnd,
                EveningCivilTwilightBegin = dto.EveningCivilTwilightBegin,
                EveningCivilTwilightEnd = dto.EveningCivilTwilightEnd,
                EveningBlueHourBegin = dto.EveningBlueHourBegin,
                EveningBlueHourEnd = dto.EveningBlueHourEnd,
                EveningGoldenHourBegin = dto.EveningGoldenHourBegin,
                EveningGoldenHourEnd = dto.EveningGoldenHourEnd,
            };
        }

        public static InstantCacheDataContract InstantCacheToContract(InstantCacheData dto)
        {
            return new InstantCacheDataContract
            { 
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
                NextMoonPhases = (List<MoonPhaseTimeDto>)dto.NextMoonPhases.Select(e=>new MoonPhaseTimeContract {Phase = e.Phase, TimeUtc = e.TimeUtc }),                
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
        public List<MoonPhaseTimeDto> NextMoonPhases { get; set; }
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
