using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfClient.MVVM;
using WpfClient.Services;

namespace WpfClient.ViewModels
{
    public class NowViewModel : ViewModelBase
    {
        private readonly AstronomyServiceClient _svc;

        private bool _isNight;
        public bool IsNight
        {
            get => _isNight;
            set => Set(ref _isNight, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private InstantCacheDataContract _data;
        public InstantCacheDataContract Data
        {
            get => _data;
            set => Set(ref _data, value);
        }

        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand SetDayCommand { get; }
        public RelayCommand SetNightCommand { get; }

        public NowViewModel(AstronomyServiceClient svc)
        {
            _svc = svc;

            SetDayCommand = new RelayCommand(() => IsNight = false);
            SetNightCommand = new RelayCommand(() => IsNight = true);

            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        }

        public async Task RefreshAsync()
        {
            try
            {
                Status = "Odświeżanie...";
                Data = await _svc.GetFreshInstantAsync();
                Status = "OK";
            }
            catch (Exception ex)
            {
                Status = ex.GetType().Name + ": " + ex.Message;
            }
        }
    }
}
