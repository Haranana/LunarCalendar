using GeoTimeZone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace Core
{

    public enum MoonPhases
    {
        NewMoon,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbous,
        FullMoon,
        WaningGibbous,
        LastQuarter,
        WaningCrescent,
    }

    /// <summary>
    /// Utility methods for resolving time zones based on geographic coordinates
    /// </summary>
    public static class TimeZoneUtil
    {
        /// <summary>
        /// Resolves the local <see cref="TimeZoneInfo"/> for given coordinates
        /// </summary>
        /// <param name="lat">Geographical latitude in degrees (range: -90..90).</param>
        /// <param name="lon">Geographical longitude in degrees (range: -180..180).</param>
        /// <returns>Windows time zone converted from the resolved IANA id</returns>
        public static TimeZoneInfo GetTimeZone(double lat, double lon)
        {

            var tz = TimeZoneLookup.GetTimeZone(lat, lon);
            string iana = tz.Result; 


            return TZConvert.GetTimeZoneInfo(iana);
        }

        /// <summary>
        /// Resolves the IANA time zone id for given coordinates
        /// </summary>
        /// <param name="lat">Geographical latitude in degrees (range: -90..90).</param>
        /// <param name="lon">Geographical longitude in degrees (range: -180..180).</param>
        /// <returns>IANA time zone id (e.g. <c>Europe/Warsaw</c>)</returns>
        public static string GetIanaTimeZoneId(double lat, double lon)
        {
            return TimeZoneLookup.GetTimeZone(lat, lon).Result;
        }
    }

    /// Astronomy related helper methods with focus on moon phase and lunar age calculations)
    /// </summary>
    /// <remarks>
    /// Every method with <see cref="DateTimeOffset"/> parameter expects the value to be in universal time
    /// </remarks>
    public static class Astronomy
    {               
        private const Double SynodicMonth = 29.530588; //in days
        private static readonly DateTimeOffset NewMoonRef = new DateTimeOffset(2000, 1, 6, 18, 13, 0 , TimeSpan.Zero);

        /// <summary>
        /// Calculates synodic age (in days) for a given UTC date and time
        /// </summary>
        /// <param name="date">UTC instant</param>
        /// <returns>Synodic age (in days) in range [0, <see cref="SynodicMonth"/>).</returns>
        public static double GetSynodicAge(DateTimeOffset date)
        {
            return Mod((ToJulianDate(date) - ToJulianDate(NewMoonRef)), SynodicMonth);
        }

        /// <summary>
        /// Calculates synodic age (in days) for a current moment
        /// </summary>
        /// <returns>Synodic age (in days) in range [0, <see cref="SynodicMonth"/>).</returns>
        public static double GetSynodicAgeNow()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return Mod((ToJulianDate(now) - ToJulianDate(NewMoonRef)), SynodicMonth);
        }

        /// <summary>
        /// Returns appr. dates and times of the next major moon phases (New Moon, First Quarter, Full Moon, Last Quarter)
        /// </summary>
        /// <param name="tz">Target time zone for returned timestamps</param>
        /// <returns>
        /// Dictionary storing next occurrences of major <see cref="MoonPhases"/> as local times specified in <paramref name="tz"/>
        /// </returns>
        public static Dictionary<MoonPhases, DateTimeOffset> GetNextMoonPhasesDates(TimeZoneInfo tz)
        {
            double lunarAge = GetSynodicAgeNow();
            Dictionary<MoonPhases, DateTimeOffset> result = new Dictionary<MoonPhases, DateTimeOffset>();

            double firstQuarterMoment = SynodicMonth / 4.0;
            double fullMoonMoment = SynodicMonth / 2.0;
            double LastQuarterMoment = (3.0 * SynodicMonth) / 4.0;

            double deltaFirstQuarter = firstQuarterMoment >= lunarAge ? firstQuarterMoment - lunarAge : firstQuarterMoment - lunarAge + SynodicMonth;
            DateTimeOffset firstQuarterUtc = DateTimeOffset.UtcNow.AddDays(deltaFirstQuarter);
            DateTimeOffset localFirstQuarter = UtcToLocal(firstQuarterUtc, tz);
            result.Add(MoonPhases.FirstQuarter, localFirstQuarter);

            double deltaFullMoon = fullMoonMoment >= lunarAge ? fullMoonMoment - lunarAge : fullMoonMoment - lunarAge + SynodicMonth;
            DateTimeOffset fullMoonUtc = DateTimeOffset.UtcNow.AddDays(deltaFullMoon);
            DateTimeOffset localFullMoon = UtcToLocal(fullMoonUtc, tz);
            result.Add(MoonPhases.FullMoon, localFullMoon);


            double deltaLastQuarter = LastQuarterMoment >= lunarAge ? LastQuarterMoment - lunarAge : LastQuarterMoment - lunarAge + SynodicMonth;
            DateTimeOffset lastQuarterUtc = DateTimeOffset.UtcNow.AddDays(deltaLastQuarter);
            DateTimeOffset localLastQuarter = UtcToLocal(lastQuarterUtc, tz);
            result.Add(MoonPhases.LastQuarter, localLastQuarter);

            double deltaNewMoon = SynodicMonth - lunarAge;
            DateTimeOffset newMoonUtc = DateTimeOffset.UtcNow.AddDays(deltaNewMoon);
            DateTimeOffset localNewMoon = UtcToLocal(newMoonUtc, tz);
            result.Add(MoonPhases.NewMoon, localNewMoon);


            return result;
        }

        /// <summary>
        /// Returns the moon phase category for a given UTC date and time
        /// </summary>
        /// <param name="date">UTC instant</param>
        /// <param name="epsilon">
        /// Phase boundary tolerance in days, by default it's 2.5 hours around major phases
        /// </param>
        /// <returns>Approximated <see cref="MoonPhases"/> for the given instant.</returns>
        public static MoonPhases GetMoonPhase(DateTimeOffset date, double epsilon = 2.5 / 24.0)
        {
            double lunarAge = GetSynodicAge(date);
            double now = ToJulianDate(date);
            double lastNewMoon = now - lunarAge;


            double newMoonApproxTime = lastNewMoon + SynodicMonth;
            double firstQuarterApproxTime = lastNewMoon + 0.25 * SynodicMonth;
            double fullMoonApproxTime = lastNewMoon + 0.5 * SynodicMonth;
            double lastQuarterApproxTime = lastNewMoon + 0.75 * SynodicMonth;

            if (Math.Abs(now - lastNewMoon) < epsilon)
            {
                return MoonPhases.NewMoon;
            }
            if (Math.Abs(now - firstQuarterApproxTime) < epsilon)
            {
                return MoonPhases.FirstQuarter;
            }
            if (now > lastNewMoon && now < firstQuarterApproxTime)
            {
                return MoonPhases.WaxingCrescent;
            }
            if (Math.Abs(now - fullMoonApproxTime) < epsilon)
            {
                return MoonPhases.FullMoon;
            }
            if (now > firstQuarterApproxTime && now < fullMoonApproxTime)
            {
                return MoonPhases.WaxingGibbous;
            }
            if (Math.Abs(now - lastQuarterApproxTime) < epsilon)
            {
                return MoonPhases.LastQuarter;
            }
            if (now > fullMoonApproxTime && now < lastQuarterApproxTime)
            {
                return MoonPhases.WaningGibbous;
            }
            if (Math.Abs(now - newMoonApproxTime) < epsilon)
            {
                return MoonPhases.NewMoon;
            }
            return MoonPhases.WaningCrescent;

        }

        private static double ToJulianDate(DateTimeOffset dto)
        {
            const double UnixEpochJD = 2440587.5;              
            double days = dto.ToUnixTimeMilliseconds() / 86400000.0; 
            return UnixEpochJD + days;
        }

        private static  DateTimeOffset UtcToLocal(DateTimeOffset dto , TimeZoneInfo timeZoneInfo)
        {
            return TimeZoneInfo.ConvertTime(dto, timeZoneInfo);
        }

        private static double Mod(double a, double m)
        {
            return ((a % m) + m) % m;
        }
    }
}
