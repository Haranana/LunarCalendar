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

    public static class TimeZoneUtil
    {
        public static TimeZoneInfo GetTimeZone(double lat, double lon)
        {

            var tz = TimeZoneLookup.GetTimeZone(lat, lon);
            string iana = tz.Result; 


            return TZConvert.GetTimeZoneInfo(iana);
        }

        public static string GetIanaTimeZoneId(double lat, double lon)
        {
            return TimeZoneLookup.GetTimeZone(lat, lon).Result;
        }
    }

   

    //every method asking for DateTimeOffset expects data in utc
    public static class Astronomy
    {               
        public const Double SynodicMonth = 29.530588; //in days
        //public readonly TimeZoneInfo timeZoneInfo;
        public static readonly DateTimeOffset NewMoonRef = new DateTimeOffset(2000, 1, 6, 18, 13, 0 , TimeSpan.Zero);


        /*
        public Astronomy(TimeZoneInfo timeZoneInfo)
        {
            this.timeZoneInfo = timeZoneInfo;
        }*/

        public static double GetSynodicAge(DateTimeOffset date)
        {
            return Mod((ToJulianDate(date) - ToJulianDate(NewMoonRef)), SynodicMonth);
        }

        public static double GetSynodicAgeNow()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return Mod((ToJulianDate(now) - ToJulianDate(NewMoonRef)), SynodicMonth);
        }

        //returns synodic age for given days interval, (-1 , 5) -> from yesterday to 5 days in future at 12:00
        public static List<double> GetSynodicAgeInterval(int from, int to)
        {
            List<double> result = new List<double>();
            for (int i = from; i <= to; i++)
            {
                DateTimeOffset iDayTime = DateTimeOffset.UtcNow.Date.AddDays(i).AddHours(12);
                result.Add(Mod((ToJulianDate(iDayTime) - ToJulianDate(NewMoonRef)), SynodicMonth));
            }

            return result;
        }

        //returns time of the next 4 MoonPhases, time is in given offset

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

        //returns moon phases (including crescents and gibbeons) on each day in specified interval from todays day
        // eg. from = -2, to = 3, means that interval starts 2 days in the past and ends 3 days in the future inlcuding those days
        public static List<MoonPhases> GetMoonPhasesInterval(int from, int to)
        {
            List<MoonPhases> result = new List<MoonPhases> ();
            DateTimeOffset now = DateTimeOffset.UtcNow;

            for (int i = from; i <= to; i++)
            {
                result.Add(GetMoonPhase(now.Date.AddDays(i).AddHours(12)));
            }

            return result;
        }

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

        public static MoonPhases GetMoonPhaseNow(double epsilon = 2.5/24.0)
        {
            double lunarAge = GetSynodicAgeNow();
            double now = ToJulianDate(DateTimeOffset.UtcNow);
            double lastNewMoon = now - lunarAge;


            double newMoonApproxTime = lastNewMoon + SynodicMonth;
            double firstQuarterApproxTime = lastNewMoon + 0.25*SynodicMonth;
            double fullMoonApproxTime = lastNewMoon + 0.5*SynodicMonth;
            double lastQuarterApproxTime = lastNewMoon + 0.75*SynodicMonth;

            if (Math.Abs(now - lastNewMoon) < epsilon){
                return MoonPhases.NewMoon;
            }
            if (Math.Abs(now - firstQuarterApproxTime) < epsilon){
                return MoonPhases.FirstQuarter;
            }
            if(now > lastNewMoon && now < firstQuarterApproxTime){
                return MoonPhases.WaxingCrescent;
            }
            if (Math.Abs(now - fullMoonApproxTime) < epsilon){
                return MoonPhases.FullMoon;
            }
            if(now > firstQuarterApproxTime && now < fullMoonApproxTime){
                return MoonPhases.WaxingGibbous;
            }
            if (Math.Abs(now - lastQuarterApproxTime) < epsilon){
                return MoonPhases.LastQuarter;
            }
            if(now > fullMoonApproxTime && now < lastQuarterApproxTime){
                return MoonPhases.WaningGibbous;
            }
            if(Math.Abs(now - newMoonApproxTime) < epsilon){
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
