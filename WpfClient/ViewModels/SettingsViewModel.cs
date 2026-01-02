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
        private readonly IAstronomyServiceClient astronomyServiceClient;
        private readonly NowViewModel nowViewModel;
        private readonly WeekViewModel weekViewModel;

        private LocationCacheDataContract locationData;
        public LocationCacheDataContract Location
        {
            get => locationData;
            set => Set(ref locationData, value);
        }

        private string latText;
        public string LatText
        {
            get => latText;
            set => Set(ref latText, value);
        }

        private string lonText;
        public string LonText
        {
            get => lonText;
            set => Set(ref lonText, value);
        }

        private string statusText;
        public string Status
        {
            get => statusText;
            set => Set(ref statusText, value);
        }

        public AsyncRelayCommand LoadLocationCommand { get; }
        public AsyncRelayCommand UpdateLocationCommand { get; }
        public AsyncRelayCommand ForceRefreshAllCommand { get; }

        public SettingsViewModel(IAstronomyServiceClient svc, NowViewModel now, WeekViewModel week)
        {
            astronomyServiceClient = svc;
            nowViewModel = now;
            weekViewModel = week;

            LoadLocationCommand = new AsyncRelayCommand(LoadLocationAsync);
            UpdateLocationCommand = new AsyncRelayCommand(UpdateLocationAsync);
            ForceRefreshAllCommand = new AsyncRelayCommand(ForceRefreshAllAsync);
        }

        public async Task LoadLocationAsync()
        {
            try
            {
                Status = "Status: Loading location data";
                Location = await astronomyServiceClient.GetLocationDataAsync().ConfigureAwait(true);

                LatText = Location.Latitude.ToString(CultureInfo.InvariantCulture);
                LonText = Location.Longitude.ToString(CultureInfo.InvariantCulture);

                Status = "Status: OK";
            }
            catch
            {
                Status = "Status: Data couldn't be loaded";
            }
        }

        public async Task UpdateLocationAsync()
        {
            if (!double.TryParse(LatText, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                !double.TryParse(LonText, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
            {
                Status = "Status: Incorrect coordinates (use dot as a separator!)";
                return;
            }

            try
            {
                Status = "Status: Refreshing location data";
                await astronomyServiceClient.UpdateLocationDataAsync(lat, lon).ConfigureAwait(true);

                
                await ForceRefreshAllAsync().ConfigureAwait(true);
                await LoadLocationAsync().ConfigureAwait(true);

                Status = "Status: Location data updated";
            }
            catch
            {
                Status = "Status: Data couldn't be updated";
            }
        }

        private async Task ForceRefreshAllAsync()
        {
            try
            {
                Status = "Status: Refreshing...";
                await nowViewModel.RefreshAsync().ConfigureAwait(true);
                await weekViewModel.RefreshAsync().ConfigureAwait(true);
                await LoadLocationAsync();
                Status = "Status: OK";
            }
            catch
            {
                Status = "Status: Data couldn't be refreshed";
            }

        }
    }
}
