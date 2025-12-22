using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Policy;

    public static class AstronomyMapper
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static InstantCacheData MapToInstantCacheData(
            AstronomyDto a,
            DateTimeOffset updateTime,
            TimeZoneInfo userTimeZone)
        {
            if (a == null) throw new ArgumentNullException("Astronomy dto is null");
            if (userTimeZone == null) throw new ArgumentNullException("User Time Zone Info is null");

            return new InstantCacheData
            {
                LastUpdateTime = updateTime,

                SunAltitude = a.SunAltitude,
                SunDistance = a.SunDistance,
                SunAzimuth = a.SunAzimuth,

                MoonIlluminationPercentage = ParseDoublePercent(a.MoonIlluminationPercentage),
                MoonAltitude = a.MoonAltitude,
                MoonDistance = a.MoonDistance,
                MoonAzimuth = a.MoonAzimuth,
                MoonParallacticAngle = a.MoonParallacticAngle,

                LunarAge = Astronomy.GetSynodicAge(updateTime.ToUniversalTime()),
                MoonPhase = Astronomy.GetMoonPhase(updateTime.ToUniversalTime()),


                NextMoonPhases = Astronomy.GetNextMoonPhasesDates(userTimeZone).Select(
                    kvp => new MoonPhaseTimeDto { Phase = kvp.Key, TimeUtc = kvp.Value.ToUniversalTime() }).ToList()

            };
        }

        public static WeeklyCacheData MapToWeeklyCacheData(
            AstronomyDto[] dto,
            DateTimeOffset updateTime,
            TimeZoneInfo userTimeZone)
        {
            if (dto == null || dto.Length == 0) throw new ArgumentNullException("Astronomy data is empty or null");
            if (userTimeZone == null) throw new ArgumentNullException("User time zone is null");

            var list = dto.Where(a => a != null).Select(a => MapDaily(a, userTimeZone)).ToList();

            return new WeeklyCacheData
            {
                LastUpdateTime = updateTime,
                DailyCacheDatas = list
            };
        }

        private static DailyCacheData MapDaily(AstronomyDto a, TimeZoneInfo tz)
        {
            var localDate = ParseDate(a.Date);
            var localNoon = new DateTime(localDate.Year, localDate.Month, localDate.Day, 12, 0, 0, DateTimeKind.Unspecified);
            var dtoLocal = new DateTimeOffset(localNoon, tz.GetUtcOffset(localNoon));
            var dtoUtc = dtoLocal.ToUniversalTime();
            

            return new DailyCacheData
            {
                LocalDate = localDate,

                SunAltitude = a.SunAltitude,
                SunDistance = a.SunDistance,
                SunAzimuth = a.SunAzimuth,
                DayLength = ParseTimeSpan(a.DayLength),

                MorningAstronomicalTwilightBegin = ParseLocalTime(localDate, a.Morning.AstronomicalTwilightBegin, tz),
                MorningAstronomicalTwilightEnd = ParseLocalTime(localDate, a.Morning.AstronomicalTwilightEnd, tz),
                MorningNauticalTwilightBegin = ParseLocalTime(localDate, a.Morning.NauticalTwilightBegin, tz),
                MorningNauticalTwilightEnd = ParseLocalTime(localDate, a.Morning.NauticalTwilightEnd, tz),
                MorningCivilTwilightBegin = ParseLocalTime(localDate, a.Morning.CivilTwilightBegin, tz),
                MorningCivilTwilightEnd = ParseLocalTime(localDate, a.Morning.CivilTwilightEnd, tz),
                MorningBlueHourBegin = ParseLocalTime(localDate, a.Morning.BlueHourBegin, tz),
                MorningBlueHourEnd = ParseLocalTime(localDate, a.Morning.BlueHourEnd, tz),
                MorningGoldenHourBegin = ParseLocalTime(localDate, a.Morning.GoldenHourBegin, tz),
                MorningGoldenHourEnd = ParseLocalTime(localDate, a.Morning.GoldenHourEnd, tz),

                MoonAltitude = a.MoonAltitude,
                MoonDistance = a.MoonDistance,
                MoonAzimuth = a.MoonAzimuth,
                MoonParallacticAngle = a.MoonParallacticAngle,
                MoonIlluminationPercentage = ParseDoublePercent(a.MoonIlluminationPercentage),

                NightLength = TimeSpan.FromDays(1).Subtract(ParseTimeSpan(a.DayLength)),
                LunarAge = Astronomy.GetSynodicAge(dtoUtc),
                MoonPhase = Astronomy.GetMoonPhase(dtoUtc),

                EveningAstronomicalTwilightBegin = ParseLocalTime(localDate, a.Evening.AstronomicalTwilightBegin, tz),
                EveningAstronomicalTwilightEnd = ParseLocalTime(localDate, a.Evening.AstronomicalTwilightEnd, tz),
                EveningNauticalTwilightBegin = ParseLocalTime(localDate, a.Evening.NauticalTwilightBegin, tz),
                EveningNauticalTwilightEnd = ParseLocalTime(localDate, a.Evening.NauticalTwilightEnd, tz),
                EveningCivilTwilightBegin = ParseLocalTime(localDate, a.Evening.CivilTwilightBegin, tz),
                EveningCivilTwilightEnd = ParseLocalTime(localDate, a.Evening.CivilTwilightEnd, tz),
                EveningBlueHourBegin = ParseLocalTime(localDate, a.Evening.BlueHourBegin, tz),
                EveningBlueHourEnd = ParseLocalTime(localDate, a.Evening.BlueHourEnd, tz),
                EveningGoldenHourBegin = ParseLocalTime(localDate, a.Evening.GoldenHourBegin, tz),
                EveningGoldenHourEnd = ParseLocalTime(localDate, a.Evening.GoldenHourEnd, tz),
            };
        }

        /* NOT in use
        public static LocationCacheData MapToLocationCacheData(LocationDto dto, DateTimeOffset updateTime, TimeZoneInfo userTimeZoneInfo, string ianaTimeZoneId)
        {
            return new LocationCacheData
            {
                LastUpdateTime = updateTime,
                UserTimeZoneInfo = userTimeZoneInfo,
                Latitude = Double.Parse(dto.Latitude, CultureInfo.InvariantCulture),
                Longitude = Double.Parse(dto.Longitude, CultureInfo.InvariantCulture),
                City = dto.City,
                CountryName = dto.CountryName,
                IanaTimeZoneId = ianaTimeZoneId,
            };
        }*/

        private static DateTime ParseDate(string yyyyMmDd)
        {
            if (string.IsNullOrWhiteSpace(yyyyMmDd))
                throw new FormatException("AstronomyDto.Date is null");

            return DateTime.ParseExact(yyyyMmDd.Trim(), "yyyy-MM-dd", Inv, DateTimeStyles.None);
        }

        //returns DateTimeOffset of given date and hours in utc
        private static DateTimeOffset? ParseLocalTime(DateTime date, string hhMm, TimeZoneInfo tz)
        {
            if (string.IsNullOrWhiteSpace(hhMm))
            {
                //throw new FormatException("Missing time value");
                return null;
            }

            var s = hhMm.Trim();
            if (!TimeSpan.TryParseExact(s, new[] { @"hh\:mm", @"h\:mm", @"hh\:mm\:ss", @"h\:mm\:ss" }, Inv, out var tod)) {
                //throw new FormatException("Invalid time format ");
                return null;
            }
                

            var localUnspec = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified).Add(tod);

            if (tz.IsInvalidTime(localUnspec))
            {
                //do { localUnspec = localUnspec.AddMinutes(1); }
                //while (tz.IsInvalidTime(localUnspec));
                return null;
            }

            var localWithOffset = new DateTimeOffset(localUnspec, tz.GetUtcOffset(localUnspec));
            return localWithOffset.ToUniversalTime();
        }

        private static TimeSpan ParseTimeSpan(string hms)
        {
            if (string.IsNullOrWhiteSpace(hms))
                return default;

            var s = hms.Trim();

            if (TimeSpan.TryParseExact(s, new[] { @"hh\:mm\:ss", @"h\:mm\:ss", @"hh\:mm", @"h\:mm", @"mm\:ss" }, Inv, out var ts))
                return ts;

            if (TimeSpan.TryParse(s, Inv, out ts))
                return ts;

            throw new FormatException($"Invalid TimeSpan format '{hms}'.");
        }

        private static double ParseDoublePercent(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0.0;

            var t = s.Trim().Replace("%", "");

            if (double.TryParse(t, NumberStyles.Float, Inv, out var v))
                return v;

            throw new FormatException($"Invalid percentage value '{s}'.");
        }
    }


}
