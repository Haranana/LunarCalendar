using Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfClient.MVVM;
using WpfClient.Services;

namespace WpfClient.ViewModels
{
    public class WeekViewModel : ViewModelBase
    {
        private readonly AstronomyServiceClient _svc;

        public ObservableCollection<DailyCacheDataContract> Days { get; } = new ObservableCollection<DailyCacheDataContract>();

        private DailyCacheDataContract _selectedDay;
        public DailyCacheDataContract SelectedDay
        {
            get => _selectedDay;
            set => Set(ref _selectedDay, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand SelectDayCommand { get; }

        public WeekViewModel(AstronomyServiceClient svc)
        {
            _svc = svc;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            SelectDayCommand = new RelayCommand(o =>
            {
                if (o is DailyCacheDataContract d) SelectedDay = d;
            });
        }

        public async Task RefreshAsync()
        {
            try
            {
                Status = "Refreshing data";
                var week = await _svc.GetFreshWeeklyAsync().ConfigureAwait(true);

                Days.Clear();
                foreach (var d in week.DailyCacheDatas.OrderBy(x => x.LocalDate))
                    Days.Add(d);

                SelectedDay = Days.FirstOrDefault();
                Status = $"OK: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            }
            catch
            {
                Status = "Data couldn't be refreshed";
            }
        }
    }
}
