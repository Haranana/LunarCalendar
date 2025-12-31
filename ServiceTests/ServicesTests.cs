using Core;
using Microsoft.Testing.Platform.Logging;
using NUnit.Framework;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Tests
{
    [TestFixture()]
    public class ServicesTests
    {
        [TestFixture]
        public class AstronomyServiceTests
        {
            private CacheStorage cacheStorage;
            private FakeIpGeoClient ipGeoClient;
            private FakeLogger loggingService;
            private AstronomyService astronomyService;

            [SetUp]
            public void SetUp()
            {
                cacheStorage = new CacheStorage();
                cacheStorage.SetLocation(new LocationCacheData
                {
                    Latitude = 52.2298,
                    Longitude = 21.0117,
                    IanaTimeZoneId = "Etc/UTC",
                    UserTimeZoneInfo = TimeZoneInfo.Utc,
                    City = "Warsaw",
                    CountryName = "Poland",
                    LastUpdateTime = DateTimeOffset.UtcNow
                });

                ipGeoClient = new FakeIpGeoClient();
                loggingService = new FakeLogger();
                astronomyService = new AstronomyService(cacheStorage, ipGeoClient, loggingService);
            }

            [Test]
            public async Task GetFreshInstant_WhenCacheIsFresh_DoesNotCallApi()
            {
                //Arrange
                cacheStorage.SetInstant(new InstantCacheData
                {
                    LastUpdateTime = DateTimeOffset.UtcNow,
                    Date = DateTime.UtcNow.Date,
                    CurrentTime = TimeSpan.Zero,
                    NextMoonPhases = new List<MoonPhaseTimeDto>()
                });

                //Act
                var result = await astronomyService.GetFreshInstant();

                //Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(ipGeoClient.GetAstronomyCalls, Is.EqualTo(0));
            }

            [Test]
            public async Task GetFreshInstant_WhenCacheExpired_CallsApi()
            {
                // Arrange: brak danych => stale
                cacheStorage.InvalidateInstant();

                ipGeoClient.AstronomyToReturn = MakeAstronomyResponseDto(city: "Torun", country: "Poland");

                // Act
                var result = await astronomyService.GetFreshInstant();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(ipGeoClient.GetAstronomyCalls, Is.EqualTo(1));
                Assert.That(cacheStorage.InstantCacheData, Is.Not.Null);
                Assert.That(loggingService.Info.Any(message => message.Contains("requesting instant data refresh")), Is.True);
            }

            [Test]
            public async Task GetFreshInstant_WhenApiThrows_ReturnsCachedAndLogsError()
            {
                // Arrange
                cacheStorage.SetInstant(new InstantCacheData
                {
                    LastUpdateTime = DateTimeOffset.UtcNow - TimeSpan.FromHours(3),
                    Date = DateTime.UtcNow.Date,
                    CurrentTime = TimeSpan.Zero,
                    NextMoonPhases = new List<MoonPhaseTimeDto>()
                });

                ipGeoClient.ThrowOnGetAstronomy = new InvalidOperationException("someException");

                // Act
                var result = await astronomyService.GetFreshInstant();

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(ipGeoClient.GetAstronomyCalls, Is.EqualTo(1));
                Assert.That(loggingService.Errors.Count, Is.GreaterThanOrEqualTo(1));
                Assert.That(loggingService.Errors.Any(e => e.Message.Contains("Refreshing instant data failed")), Is.True);
            }

            [Test]
            public async Task GetFreshWeekly_WhenCacheIsFresh_DoesNotCallApi()
            {
                //Arrange
                cacheStorage.SetWeekly(new WeeklyCacheData
                {
                    LastUpdateTime = DateTimeOffset.UtcNow,
                    DailyCacheDatas = new List<DailyCacheData>()
                });

                //Act
                var result = await astronomyService.GetFreshWeekly();

                //Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(ipGeoClient.GetAstronomyRangeCalls, Is.EqualTo(0));
            }

            [Test]
            public async Task GetFreshWeekly_WhenCacheIsExpired_CallsApi()
            {
                //Arrange
                cacheStorage.InvalidateWeekly();

                ipGeoClient.TimeSeriesToReturn = MakeTimeSeriesDto7Days();

                //Act
                var result = await astronomyService.GetFreshWeekly();

                //Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(ipGeoClient.GetAstronomyRangeCalls, Is.EqualTo(1));
                Assert.That(cacheStorage.WeeklyCacheData, Is.Not.Null);
                Assert.That(loggingService.Info.Any(message => message.Contains("requesting weekly data refresh")), Is.True);
            }

            [Test]
            public async Task AstronomyWorker_FetchAndUpdateAstronomyCacheAsync_CallsApiAndUpdatesCache()
            {
                //Arrange
                var worker = new AstronomyWorker(cacheStorage, ipGeoClient, loggingService);

                ipGeoClient.TimeSeriesToReturn = MakeTimeSeriesDto7Days();
                ipGeoClient.AstronomyToReturn = MakeAstronomyResponseDto(city: "Torun", country: "Poland");

                cacheStorage.InvalidateInstant();
                cacheStorage.InvalidateWeekly();
                
                //Act
                await worker.FetchAndUpdateAstronomyCacheAsync();

                //Assert
                Assert.That(ipGeoClient.GetAstronomyCalls, Is.EqualTo(1));
                Assert.That(ipGeoClient.GetAstronomyRangeCalls, Is.EqualTo(1));
                Assert.That(cacheStorage.InstantCacheData, Is.Not.Null);
                Assert.That(cacheStorage.WeeklyCacheData, Is.Not.Null);
            }


            private static AstronomyResponseDto MakeAstronomyResponseDto(string city, string country)
            {
                return new AstronomyResponseDto
                {
                    Location = new LocationDto { City = city, CountryName = country },
                    Astronomy = MakeAstronomyDtoForInstant("2025-01-01", "12:00:00")
                };
            }

            private static AstronomyTimeSeriesDto MakeTimeSeriesDto7Days()
            {
                var arr = new AstronomyDto[7];
                for (int i = 0; i < 7; i++)
                {
                    arr[i] = MakeAstronomyDtoForDaily($"2025-01-{(i + 1):00}");
                }

                return new AstronomyTimeSeriesDto { Astronomy = arr };
            }

            private static AstronomyDto MakeAstronomyDtoForInstant(string date, string currentTime)
            {
                return new AstronomyDto
                {
                    Date = date,
                    CurrentTime = currentTime,

                    SunAltitude = 1,
                    SunDistance = 2,
                    SunAzimuth = 3,

                    MoonIlluminationPercentage = "20%",
                    MoonAltitude = 4,
                    MoonDistance = 5,
                    MoonAzimuth = 6,
                    MoonParallacticAngle = 7,
                };
            }

            private static AstronomyDto MakeAstronomyDtoForDaily(string date)
            {
                return new AstronomyDto
                {
                    Date = date,

                    SunAltitude = 1,
                    SunDistance = 2,
                    SunAzimuth = 3,

                    DayLength = "08:00:00",
                    Sunrise = "07:10",
                    Sunset = "15:10",

                    MoonIlluminationPercentage = "10%",
                    MoonAltitude = 4,
                    MoonDistance = 5,
                    MoonAzimuth = 6,
                    MoonParallacticAngle = 7,
                    Moonrise = "10:40",
                    Moonset = "21:24",

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
                    }
                };
            }
        }

        internal sealed class FakeIpGeoClient : IIpGeoClient
        {
            public int GetAstronomyCalls { get; private set; }
            public int GetAstronomyRangeCalls { get; private set; }

            public Exception ThrowOnGetAstronomy { get; set; }
            public Exception ThrowOnGetAstronomyRange { get; set; }

            public AstronomyResponseDto AstronomyToReturn { get; set; }
            public AstronomyTimeSeriesDto TimeSeriesToReturn { get; set; }

            public Func<double, double, string, Task<AstronomyResponseDto>> GetAstronomyAsyncImpl { get; set; }
            public Func<double, double, DateTimeOffset, DateTimeOffset, string, Task<AstronomyTimeSeriesDto>> GetAstronomyRangeAsyncImpl { get; set; }
            public Task<AstronomyResponseDto> GetAstronomyAsync(double lat, double lon, string ianaTz, DateTime? date)
            {
                GetAstronomyCalls++;

                if (ThrowOnGetAstronomy != null) throw ThrowOnGetAstronomy;

                if (GetAstronomyAsyncImpl != null)
                    return GetAstronomyAsyncImpl(lat, lon, ianaTz);

                return Task.FromResult(AstronomyToReturn);
            }

            public Task<AstronomyTimeSeriesDto> GetAstronomyRangeAsync(double lat, double lon, DateTimeOffset start, DateTimeOffset end, string ianaTz)
            {
                GetAstronomyRangeCalls++;

                if (ThrowOnGetAstronomyRange != null) throw ThrowOnGetAstronomyRange;

                if (GetAstronomyRangeAsyncImpl != null)
                    return GetAstronomyRangeAsyncImpl(lat, lon, start, end, ianaTz);

                return Task.FromResult(TimeSeriesToReturn);
            }
        }

        internal sealed class FakeLogger : Core.ILogger
        {
            public List<string> Info { get; } = new List<string>();
            public List<(string Message, Exception Ex)> Errors { get; } = new List<(string, Exception)>();

            public void WriteInfo(string message) => Info.Add(message);

            public void WriteWarning(string message) => Info.Add(message);

            public void WriteError(string message) => Errors.Add((message, null));

            public void WriteError(string message, Exception ex) => Errors.Add((message, ex));
        }
    }
}