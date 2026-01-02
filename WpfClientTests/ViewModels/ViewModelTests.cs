using Contracts;
using Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfClient.Services;
using WpfClient.ViewModels;

namespace WpfClient.ViewModels.Tests
{
    internal class FakeAstronomyServiceClient : IAstronomyServiceClient
    {
        public int GetFreshInstantCalls { get; private set; }
        public int GetFreshWeeklyCalls { get; private set; }
        public int GetLocationCalls { get; private set; }
        public int UpdateLocationCalls { get; private set; }

        public Func<Task<InstantCacheDataContract>> GetFreshInstantAsyncImpl { get; set; }
        public Func<Task<WeeklyCacheDataContract>> GetFreshWeeklyAsyncImpl { get; set; }
        public Func<Task<LocationCacheDataContract>> GetLocationDataAsyncImpl { get; set; }
        public Func<double, double, Task> UpdateLocationDataAsyncImpl { get; set; }

        public Task<InstantCacheDataContract> GetFreshInstantAsync()
        {
            GetFreshInstantCalls++;
            return GetFreshInstantAsyncImpl();
        }

        public Task<WeeklyCacheDataContract> GetFreshWeeklyAsync()
        {
            GetFreshWeeklyCalls++;
            return GetFreshWeeklyAsyncImpl();
        }

        public Task<LocationCacheDataContract> GetLocationDataAsync()
        {
            GetLocationCalls++;
            return GetLocationDataAsyncImpl();
        }

        public Task UpdateLocationDataAsync(double lat, double lon)
        {
            UpdateLocationCalls++;
            return UpdateLocationDataAsyncImpl(lat, lon);
        }
    }

    internal class StubImageProvider : IImageProvider
    {
        public BitmapImage getMoonImage(MoonPhases phase) => null;
    }

    internal static class ViewModelTestsUtil
    {
        public static void SetPrivateField(object obj, string fieldName, object value)
        {
            var privField = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (privField == null) return;
            privField.SetValue(obj, value);
        }
    }

    internal static class MockDtoFactory
    {
        public static DailyCacheDataContract MakeDaily(string yyyyMmDd, MoonPhases phase)
        {
            DateTime d = DateTime.Parse(yyyyMmDd);

            return new DailyCacheDataContract
            {
                LocalDate = d,
                DayLength = TimeSpan.FromHours(8),
                NightLength = TimeSpan.FromHours(16),

                Sunrise = new DateTimeOffset(d.Year, d.Month, d.Day, 7, 10, 0, TimeSpan.Zero),
                Sunset = new DateTimeOffset(d.Year, d.Month, d.Day, 15, 10, 0, TimeSpan.Zero),

                Moonrise = new DateTimeOffset(d.Year, d.Month, d.Day, 10, 40, 0, TimeSpan.Zero),
                Moonset = new DateTimeOffset(d.Year, d.Month, d.Day, 21, 24, 0, TimeSpan.Zero),

                MorningBlueHourBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 6, 40, 0, TimeSpan.Zero),
                MorningBlueHourEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 7, 5, 0, TimeSpan.Zero),
                EveningBlueHourBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 16, 10, 0, TimeSpan.Zero),
                EveningBlueHourEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 16, 40, 0, TimeSpan.Zero),

                MorningGoldenHourBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 7, 5, 0, TimeSpan.Zero),
                MorningGoldenHourEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 7, 40, 0, TimeSpan.Zero),
                EveningGoldenHourBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 15, 10, 0, TimeSpan.Zero),
                EveningGoldenHourEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 15, 40, 0, TimeSpan.Zero),

                MorningAstronomicalTwilightBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 5, 0, 0, TimeSpan.Zero),
                MorningAstronomicalTwilightEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 6, 0, 0, TimeSpan.Zero),
                MorningNauticalTwilightBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 6, 0, 0, TimeSpan.Zero),
                MorningNauticalTwilightEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 6, 30, 0, TimeSpan.Zero),
                MorningCivilTwilightBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 6, 30, 0, TimeSpan.Zero),
                MorningCivilTwilightEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 7, 0, 0, TimeSpan.Zero),

                EveningAstronomicalTwilightBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 18, 0, 0, TimeSpan.Zero),
                EveningAstronomicalTwilightEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 19, 0, 0, TimeSpan.Zero),
                EveningNauticalTwilightBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 17, 30, 0, TimeSpan.Zero),
                EveningNauticalTwilightEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 18, 0, 0, TimeSpan.Zero),
                EveningCivilTwilightBegin = new DateTimeOffset(d.Year, d.Month, d.Day, 16, 0, 0, TimeSpan.Zero),
                EveningCivilTwilightEnd = new DateTimeOffset(d.Year, d.Month, d.Day, 16, 30, 0, TimeSpan.Zero),

                MoonPhase = phase
            };
        }

        public static WeeklyCacheDataContract MakeWeekly(params DailyCacheDataContract[] days)
            => new WeeklyCacheDataContract { DailyCacheDatas = new List<DailyCacheDataContract>(days) };

        public static InstantCacheDataContract MakeInstant(MoonPhases phase)
            => new InstantCacheDataContract
            {
                Date = new DateTime(2025, 1, 1),
                CurrentTime = new TimeSpan(9, 15, 0),
                MoonPhase = phase,
                NextMoonPhases = new List<MoonPhaseTimeContract>
                {
                new MoonPhaseTimeContract { Phase = MoonPhases.NewMoon, TimeUtc = new DateTimeOffset(2025,1,10,0,0,0,TimeSpan.Zero) },
                new MoonPhaseTimeContract { Phase = MoonPhases.FirstQuarter, TimeUtc = new DateTimeOffset(2025,1,17,0,0,0,TimeSpan.Zero) },
                new MoonPhaseTimeContract { Phase = MoonPhases.FullMoon, TimeUtc = new DateTimeOffset(2025,1,25,0,0,0,TimeSpan.Zero) },
                new MoonPhaseTimeContract { Phase = MoonPhases.LastQuarter, TimeUtc = new DateTimeOffset(2025,2,2,0,0,0,TimeSpan.Zero) },
                }
            };

        public static LocationCacheDataContract MakeLocation(double lat, double lon)
            => new LocationCacheDataContract { Latitude = lat, Longitude = lon, City = "Warsaw", CountryName = "Poland" };
    }


    [TestFixture, Apartment(ApartmentState.STA)]
    public class WeekViewModelTests
    {
        [Test]
        public async Task RefreshAsync_Success()
        {
            //Arrange
            var client = new FakeAstronomyServiceClient
            {
                GetFreshWeeklyAsyncImpl = () => Task.FromResult(
                    MockDtoFactory.MakeWeekly(
                        MockDtoFactory.MakeDaily("2025-01-03", MoonPhases.FullMoon),
                        MockDtoFactory.MakeDaily("2025-01-01", MoonPhases.FullMoon),
                        MockDtoFactory.MakeDaily("2025-01-02", MoonPhases.FullMoon)
                    )
                )
            };

            var weekViewModel = new WeekViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(weekViewModel, "imageProvider", new StubImageProvider());

            //Act
            await weekViewModel.RefreshAsync();

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(weekViewModel.Status, Is.EqualTo("Status: Ok"));
                Assert.That(weekViewModel.Days.Select(date => date.LocalDate).ToArray(),
                    Is.EqualTo(new[] { new DateTime(2025, 1, 1), new DateTime(2025, 1, 2), new DateTime(2025, 1, 3) }));

                Assert.That(weekViewModel.SelectedDay.LocalDate, Is.EqualTo(new DateTime(2025, 1, 1)));
                Assert.That(weekViewModel.DayLength, Is.EqualTo("08:00"));
                Assert.That(weekViewModel.Sunrise, Is.EqualTo("07:10"));
                Assert.That(weekViewModel.MoonPhaseName, Is.EqualTo("Full Moon"));
            });
        }

        [Test]
        public async Task RefreshAsync_WhenException_StatusIsError()
        {
            //Arrange
            var client = new FakeAstronomyServiceClient
            {
                GetFreshWeeklyAsyncImpl = () => throw new InvalidOperationException("someException")
            };

            var vm = new WeekViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(vm, "imageProvider", new StubImageProvider());

            //Act
            await vm.RefreshAsync();

            //Assert
            Assert.That(vm.Status, Is.EqualTo("Status: Error..."));
        }

        [Test]
        public void SelectDayCommand_UpdatesDay_And_Values()
        {
            //Arrange
            var client = new FakeAstronomyServiceClient
            {
                GetFreshWeeklyAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeWeekly())
            };

            WeekViewModel viewModel = new WeekViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(viewModel, "imageProvider", new StubImageProvider());
            DailyCacheDataContract day = MockDtoFactory.MakeDaily("2025-01-02", MoonPhases.NewMoon);

            //Act
            viewModel.SelectDayCommand.Execute(day);

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.SelectedDay, Is.SameAs(day));
                Assert.That(viewModel.MoonPhaseName, Is.EqualTo("New Moon"));
                Assert.That(viewModel.Sunset, Is.EqualTo("15:10"));
            });
        }
    }

    [TestFixture, Apartment(ApartmentState.STA)]
    public class NowViewModelTests
    {
        [Test]
        public async Task RefreshAsync_Success()
        {
            //Arrange
            var client = new FakeAstronomyServiceClient
            {
                GetFreshInstantAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeInstant(MoonPhases.WaxingCrescent)),
                GetFreshWeeklyAsyncImpl = () => Task.FromResult(
                    MockDtoFactory.MakeWeekly(
                        MockDtoFactory.MakeDaily("2025-01-01", MoonPhases.FullMoon),
                        MockDtoFactory.MakeDaily("2025-01-02", MoonPhases.FullMoon),
                        MockDtoFactory.MakeDaily("2025-01-03", MoonPhases.FullMoon)
                    )
                )
            };

            var viewModel = new NowViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(viewModel, "imageProvider", new StubImageProvider());

            //Act
            await viewModel.RefreshAsync();

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Status, Is.EqualTo("Status: OK"));
                Assert.That(viewModel.DailyData.LocalDate, Is.EqualTo(new System.DateTime(2025, 1, 2)));
                Assert.That(viewModel.CurrentDateAndTime, Is.EqualTo("01-01-2025 - 09:15"));
                Assert.That(viewModel.MoonPhaseName, Is.EqualTo("Waxing Crescent"));
                Assert.That(viewModel.NextNewMoonDate, Is.EqualTo("10-01-2025"));
                Assert.That(viewModel.NextFirstQuarterDate, Is.EqualTo("17-01-2025"));
                Assert.That(viewModel.NextFullMoonDate, Is.EqualTo("25-01-2025"));
                Assert.That(viewModel.NextLastQuarterDate, Is.EqualTo("02-02-2025"));
                Assert.That(viewModel.MorningBlueHour, Does.Contain("06:40"));
                Assert.That(viewModel.EveningBlueHour, Does.Contain("16:10"));
            });
            Assert.That(client.GetFreshInstantCalls, Is.EqualTo(1));
            Assert.That(client.GetFreshWeeklyCalls, Is.EqualTo(1));
        }

        [Test]
        public async Task RefreshAsync_WhenException_StatusException()
        {
            //Arrange
            FakeAstronomyServiceClient client = new FakeAstronomyServiceClient
            {
                GetFreshInstantAsyncImpl = () => throw new System.InvalidOperationException("MockException"),
                GetFreshWeeklyAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeWeekly())
            };

            NowViewModel view = new NowViewModel(client, autoRefresh: false);

            //Act
            await view.RefreshAsync();

            //Assert
            Assert.That(view.Status, Does.Contain("InvalidOperationException"));
            Assert.That(view.Status, Does.Contain("MockException"));
        }
    }

    [TestFixture, Apartment(ApartmentState.STA)]
    public class NowViewDayModelTests
    {
        [Test]
        public async Task RefreshAsync_Success()
        {
            //Arrange
            FakeAstronomyServiceClient client = new FakeAstronomyServiceClient
            {
                GetFreshInstantAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeInstant(MoonPhases.FullMoon)),
                GetFreshWeeklyAsyncImpl = () => Task.FromResult(
                    MockDtoFactory.MakeWeekly(
                        MockDtoFactory.MakeDaily("2025-01-01", MoonPhases.FullMoon),
                        MockDtoFactory.MakeDaily("2025-01-02", MoonPhases.FullMoon), 
                        MockDtoFactory.MakeDaily("2025-01-03", MoonPhases.FullMoon)
                    )
                )
            };
            NowViewDayModel vm = new NowViewDayModel(client, autoRefresh: false);

            //Act
            await vm.RefreshAsync();

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(vm.Status, Is.EqualTo("Status: OK"));
                Assert.That(vm.DayLength, Is.EqualTo("08:00"));
                Assert.That(vm.NightLength, Is.EqualTo("16:00"));
                Assert.That(vm.MorningAstronomicalTwilight, Does.Contain("05:00"));
                Assert.That(vm.EveningCivilTwilight, Does.Contain("16:00"));
            });
        }
    }

    [TestFixture, Apartment(ApartmentState.STA)]
    public class SettingsViewModelTests
    {
        [Test]
        public async Task LoadLocationCommand_Success()
        {
            //Arrange
            FakeAstronomyServiceClient client = new FakeAstronomyServiceClient
            {
                GetLocationDataAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeLocation(52.1, 21.2)),
                GetFreshInstantAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeInstant(MoonPhases.FullMoon)),
                GetFreshWeeklyAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeWeekly(
                    MockDtoFactory.MakeDaily("2025-01-01",MoonPhases.FullMoon), MockDtoFactory.MakeDaily("2025-01-02", MoonPhases.FullMoon)))
            };

            NowViewModel now = new NowViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(now, "imageProvider", new StubImageProvider());

            WeekViewModel week = new WeekViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(week, "imageProvider", new StubImageProvider());

            SettingsViewModel viewModel = new SettingsViewModel(client, now, week);

            //Act
            await viewModel.LoadLocationAsync();

            //Assert
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Status, Is.EqualTo("Status: OK"));
                Assert.That(viewModel.Location.City, Is.EqualTo("Warsaw"));
                Assert.That(viewModel.Location.Latitude, Is.InRange(52.1 - 1e-6, 52.1 + 1e-6));
                Assert.That(viewModel.LonText, Is.EqualTo(21.2.ToString(CultureInfo.InvariantCulture)));
            });
        }

        [Test]
        public async Task UpdateLocationCommand_WhenInvalidInput_DoesNotCallService()
        {
            //Arrange
            FakeAstronomyServiceClient client = new FakeAstronomyServiceClient
            {
                UpdateLocationDataAsyncImpl = (_, __) => Task.CompletedTask,
                GetLocationDataAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeLocation(1, 2)),
                GetFreshInstantAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeInstant(MoonPhases.FullMoon)),
                GetFreshWeeklyAsyncImpl = () => Task.FromResult(MockDtoFactory.MakeWeekly(
                    MockDtoFactory.MakeDaily("2025-01-01", MoonPhases.FullMoon), MockDtoFactory.MakeDaily("2025-01-02", MoonPhases.FullMoon)))
            };

            NowViewModel now = new NowViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(now, "imageProvider", new StubImageProvider());

            WeekViewModel week = new WeekViewModel(client, autoRefresh: false);
            ViewModelTestsUtil.SetPrivateField(week, "imageProvider", new StubImageProvider());

            SettingsViewModel viewModel = new SettingsViewModel(client, now, week)
            {
                LatText = "52,2",
                LonText = "21.0"
            };

            //Act
            await viewModel.UpdateLocationAsync();

            //Assert
            Assert.That(viewModel.Status, Does.Contain("Incorrect coordinates"));
            Assert.That(client.UpdateLocationCalls, Is.EqualTo(0));
        }
    }
}