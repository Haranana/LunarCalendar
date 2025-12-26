using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{

    public class AstronomyTimeSeriesDto
    {
        [JsonProperty("location")]
        public LocationDto Location { get; set; }

        [JsonProperty("astronomy")]
        public AstronomyDto[] Astronomy { get; set; }

    }

    public class AstronomyResponseDto
    {
        [JsonProperty("location")]
        public LocationDto Location { get; set; }

        [JsonProperty("astronomy")]
        public AstronomyDto Astronomy { get; set; }

    }
    public class LocationDto
    {
        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }
    }

    public class AstronomyDto
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("current_time")]
        public string CurrentTime { get; set; }


        [JsonProperty("morning")]
        public MorningDto Morning { get; set; }

        [JsonProperty("evening")]
        public EveningDto Evening { get; set; }

        [JsonProperty("sunrise")]
        public string Sunrise { get; set; }
        [JsonProperty("sunset")]
        public string Sunset { get; set; }

        [JsonProperty("day_length")]
        public string DayLength { get; set; }

        [JsonProperty("sun_altitude")]
        public double SunAltitude { get; set; }

        [JsonProperty("sun_distance")]
        public double SunDistance { get; set; }

        [JsonProperty("sun_azimuth")]
        public double SunAzimuth { get; set; }


        [JsonProperty("moonrise")]
        public string Moonrise { get; set; }
        [JsonProperty("moonset")]
        public string Moonset { get; set; }

        [JsonProperty("moon_altitude")]
        public double MoonAltitude { get; set; }

        [JsonProperty("moon_distance")]
        public double MoonDistance { get; set; }

        [JsonProperty("moon_azimuth")]
        public double MoonAzimuth { get; set; }

        [JsonProperty("moon_parallactic_angle")]
        public double MoonParallacticAngle { get; set; }

        [JsonProperty("moon_illumination_percentage")]
        public string MoonIlluminationPercentage { get; set; } 

    }

    public class MorningDto
    {
        [JsonProperty("astronomical_twilight_begin")]
        public string AstronomicalTwilightBegin { get; set; }

        [JsonProperty("astronomical_twilight_end")]
        public string AstronomicalTwilightEnd { get; set; }

        [JsonProperty("nautical_twilight_begin")]
        public string NauticalTwilightBegin { get; set; }

        [JsonProperty("nautical_twilight_end")]
        public string NauticalTwilightEnd { get; set; }

        [JsonProperty("civil_twilight_begin")]
        public string CivilTwilightBegin { get; set; }

        [JsonProperty("civil_twilight_end")]
        public string CivilTwilightEnd { get; set; }

        [JsonProperty("blue_hour_begin")]
        public string BlueHourBegin { get; set; }

        [JsonProperty("blue_hour_end")]
        public string BlueHourEnd { get; set; }

        [JsonProperty("golden_hour_begin")]
        public string GoldenHourBegin { get; set; }

        [JsonProperty("golden_hour_end")]
        public string GoldenHourEnd { get; set; }
    }

    public class EveningDto
    {
        [JsonProperty("golden_hour_begin")]
        public string GoldenHourBegin { get; set; }

        [JsonProperty("golden_hour_end")]
        public string GoldenHourEnd { get; set; }

        [JsonProperty("blue_hour_begin")]
        public string BlueHourBegin { get; set; }

        [JsonProperty("blue_hour_end")]
        public string BlueHourEnd { get; set; }

        [JsonProperty("civil_twilight_begin")]
        public string CivilTwilightBegin { get; set; }

        [JsonProperty("civil_twilight_end")]
        public string CivilTwilightEnd { get; set; }

        [JsonProperty("nautical_twilight_begin")]
        public string NauticalTwilightBegin { get; set; }

        [JsonProperty("nautical_twilight_end")]
        public string NauticalTwilightEnd { get; set; }

        [JsonProperty("astronomical_twilight_begin")]
        public string AstronomicalTwilightBegin { get; set; }

        [JsonProperty("astronomical_twilight_end")]
        public string AstronomicalTwilightEnd { get; set; }
    }
}
