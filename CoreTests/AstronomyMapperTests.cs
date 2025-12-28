using Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NUnit.Framework.Constraints.Tolerance;

namespace Core.Tests
{
    [TestFixture()]
    public class AstronomyMapperTests
    {
        private static TimeZoneInfo Warsaw => TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        private static AstronomyDto MakeDto(
            string date = "2025-01-01",
            string currentTime = "09:10:58.985",
            string dayLength = "08:00:00",
            string sunrise = "07:10",
            string sunset = "15:10",
            string moonIllum = "12.5%")
        {
            return new AstronomyDto
            {
                Date = date,
                CurrentTime = currentTime,
                DayLength = dayLength,
                Sunrise = sunrise,
                Sunset = sunset,

                SunAltitude = 1.23,
                SunDistance = 2.34,
                SunAzimuth = 3.45,

                MoonIlluminationPercentage = moonIllum,
                MoonAltitude = 4.56,
                MoonDistance = 5.67,
                MoonAzimuth = 6.78,
                MoonParallacticAngle = 7.89,

                Morning = new MorningDto
                {
                    AstronomicalTwilightBegin = "05:00",
                    AstronomicalTwilightEnd = "06:00",
                    NauticalTwilightBegin = "06:00",
                    NauticalTwilightEnd = "06:30",
                    CivilTwilightBegin = "06:30",
                    CivilTwilightEnd = "07:00",
                    BlueHourBegin = "06:40",
                    BlueHourEnd = "07:05",
                    GoldenHourBegin = "07:05",
                    GoldenHourEnd = "07:40",
                },
                Evening = new EveningDto
                {
                    AstronomicalTwilightBegin = "18:00",
                    AstronomicalTwilightEnd = "19:00",
                    NauticalTwilightBegin = "17:30",
                    NauticalTwilightEnd = "18:00",
                    CivilTwilightBegin = "16:00",
                    CivilTwilightEnd = "16:30",
                    BlueHourBegin = "16:10",
                    BlueHourEnd = "16:40",
                    GoldenHourBegin = "15:10",
                    GoldenHourEnd = "15:40",
                },

                Moonrise = "10:40",
                Moonset = "21:24",
            };
        }

        [Test]
        public void MapToInstantCacheData_NullDto_ThrowsExceptions()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                AstronomyMapper.MapToInstantCacheData(null, DateTimeOffset.UtcNow, Warsaw));

            Assert.That(ex.Message, Does.Contain("Astronomy dto"));
        }

        [Test]
        public void MapToInstantCacheData_NullTimeZone_ThrowsException()
        {          
            var dto = MakeDto();
            var ex = Assert.Throws<ArgumentNullException>(() =>  AstronomyMapper.MapToInstantCacheData(dto, DateTimeOffset.UtcNow, null));

            Assert.That(ex.Message, Does.Contain("User Time Zone"));
        }

        [Test]
        public void MapToInstantCacheData_ParsesDate_AndMapsFields()
        {
            //Arrange
            var dto = MakeDto(date: "2025-01-02", currentTime: "09:10:58.985", moonIllum: "12.5%");
            var update = new DateTimeOffset(2025, 1, 2, 12, 0, 0, TimeSpan.Zero);

            //Assert
            var mapped = AstronomyMapper.MapToInstantCacheData(dto, update, Warsaw);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(mapped.LastUpdateTime, Is.EqualTo(update));

                Assert.That(mapped.Date, Is.EqualTo(new DateTime(2025, 1, 2)));
                Assert.That(mapped.CurrentTime, Is.EqualTo(TimeSpan.ParseExact("09:10:58.985", @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture)));

                Assert.That(mapped.SunAltitude, Is.EqualTo(dto.SunAltitude));
                Assert.That(mapped.SunDistance, Is.EqualTo(dto.SunDistance));
                Assert.That(mapped.SunAzimuth, Is.EqualTo(dto.SunAzimuth));

                Assert.That(mapped.MoonIlluminationPercentage, Is.EqualTo(12.5));
                Assert.That(mapped.MoonAzimuth, Is.EqualTo(dto.MoonAzimuth));
            });
        }

        [Test]
        public void MapToWeeklyCacheData_NullOrEmpty_ThrowsExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => AstronomyMapper.MapToWeeklyCacheData(null, DateTimeOffset.UtcNow, Warsaw));

            Assert.Throws<ArgumentNullException>(() =>  AstronomyMapper.MapToWeeklyCacheData(Array.Empty<AstronomyDto>(), DateTimeOffset.UtcNow, Warsaw));
        }

        [Test]
        public void MapToWeeklyCacheData_FiltersNullEntries()
        {
            //Arrange            
            var update = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var dto1 = MakeDto(date: "2025-01-01");
            AstronomyDto dtoNull = null;
            var dto2 = MakeDto(date: "2025-01-02");
            AstronomyDto[] astronomyDtos = { dto1, dtoNull, dto2 };

            //Act
            var weekly = AstronomyMapper.MapToWeeklyCacheData(astronomyDtos, update, Warsaw);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(weekly.LastUpdateTime, Is.EqualTo(update));
                Assert.That(weekly.DailyCacheDatas, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void MapToWeeklyCacheData_CalulcateNightsLength_Correctly()
        {
            //Arrange
            var dto = MakeDto(date: "2025-01-01", dayLength: "08:00:00");

            //Act
            var weekly = AstronomyMapper.MapToWeeklyCacheData(new[] { dto }, DateTimeOffset.UtcNow, Warsaw);
            var day = weekly.DailyCacheDatas.Single();

            //Assert
            Assert.That(day.DayLength, Is.EqualTo(TimeSpan.FromHours(8)));
            Assert.That(day.NightLength, Is.EqualTo(TimeSpan.FromHours(16)));
        }

        [Test]
        public void MapToWeeklyCacheData_ConvertsLocalTimeToUtc_ForGivenTimeZone()
        {
            //Arrange
            var dto = MakeDto(date: "2025-01-01", sunrise: "07:10");

            //Act
            var weekly = AstronomyMapper.MapToWeeklyCacheData(new[] { dto }, DateTimeOffset.UtcNow, Warsaw);
            var day = weekly.DailyCacheDatas.Single();

            //Assert
            Assert.That(day.Sunrise.HasValue, Is.True);
            Assert.That(day.Sunrise.Value, Is.EqualTo(new DateTimeOffset(2025, 1, 1, 6, 10, 0, TimeSpan.Zero)));
        }

        [Test]
        public void MapToWeeklyCacheData_InvalidLocalTime_ReturnsNull()
        {
            // at 2025-03-30 in Europe/Warsaw time there's a jump from 2:00 to 3:00
            // so null is expected
            
            //Arrange
            var dto = MakeDto(date: "2025-03-30", sunrise: "02:30");

            //Act
            var weekly = AstronomyMapper.MapToWeeklyCacheData(new[] { dto }, DateTimeOffset.UtcNow, Warsaw);
            var day = weekly.DailyCacheDatas.Single();

            //Assert
            Assert.That(day.Sunrise, Is.Null);
        }
    }

}