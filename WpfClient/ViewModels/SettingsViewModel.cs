using Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfClient.MVVM;
using WpfClient.Services;

namespace WpfClient.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AstronomyServiceClient _svc;
        private readonly NowViewModel _now;
        private readonly WeekViewModel _week;

        private LocationCacheDataContract _location;
        public LocationCacheDataContract Location
        {
            get => _location;
            set => Set(ref _location, value);
        }

        private string _latText;
        public string LatText
        {
            get => _latText;
            set => Set(ref _latText, value);
        }

        private string _lonText;
        public string LonText
        {
            get => _lonText;
            set => Set(ref _lonText, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public AsyncRelayCommand LoadLocationCommand { get; }
        public AsyncRelayCommand UpdateLocationCommand { get; }
        public AsyncRelayCommand ForceRefreshAllCommand { get; }

        public SettingsViewModel(AstronomyServiceClient svc, NowViewModel now, WeekViewModel week)
        {
            _svc = svc;
            _now = now;
            _week = week;

            LoadLocationCommand = new AsyncRelayCommand(LoadLocationAsync);
            UpdateLocationCommand = new AsyncRelayCommand(UpdateLocationAsync);
            ForceRefreshAllCommand = new AsyncRelayCommand(ForceRefreshAllAsync);
        }

        private async Task LoadLocationAsync()
        {
            try
            {
                Status = "Wczytywanie lokalizacji...";
                Location = await _svc.GetLocationDataAsync().ConfigureAwait(true);

                LatText = Location.Latitude.ToString(CultureInfo.InvariantCulture);
                LonText = Location.Longitude.ToString(CultureInfo.InvariantCulture);

                Status = "OK";
            }
            catch
            {
                Status = "Nie udało się wczytać lokalizacji.";
            }
        }

        private async Task UpdateLocationAsync()
        {
            // twardo: parsujemy tylko invariant (kropka), bo tak masz w backendzie konsekwentnie
            if (!double.TryParse(LatText, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                !double.TryParse(LonText, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
            {
                Status = "Błędne lat/lon (użyj kropki).";
                return;
            }

            try
            {
                Status = "Aktualizacja lokalizacji...";
                await _svc.UpdateLocationDataAsync(lat, lon).ConfigureAwait(true);

                await LoadLocationAsync().ConfigureAwait(true);
                await ForceRefreshAllAsync().ConfigureAwait(true);

                Status = "Zaktualizowano.";
            }
            catch
            {
                Status = "Błąd aktualizacji lokalizacji.";
            }
        }

        private async Task ForceRefreshAllAsync()
        {
            try
            {
                Status = "Wymuszanie odświeżenia...";
                await _now.RefreshAsync().ConfigureAwait(true);
                await _week.RefreshAsync().ConfigureAwait(true);
                Status = "OK";
            }
            catch
            {
                Status = "Błąd odświeżenia.";
            }
        }
    }
}
