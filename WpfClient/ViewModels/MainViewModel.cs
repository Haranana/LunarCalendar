using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfClient.MVVM;
using WpfClient.Services;

namespace WpfClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AstronomyServiceClient _svc;

        public NowViewModel Now { get; }
        public NowViewDayModel NowDay { get; }
        public WeekViewModel Week { get; }
        public SettingsViewModel Settings { get; }

        private ViewModelBase _currentPage;
        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set => Set(ref _currentPage, value);
        }

        public RelayCommand GoNowCommand { get; }

        public RelayCommand GoNowDayCommand { get; }
        public RelayCommand GoWeekCommand { get; }
        public RelayCommand GoSettingsCommand { get; }

        public MainViewModel()
        {
            _svc = new AstronomyServiceClient();

            Now = new NowViewModel(_svc);
            NowDay = new NowViewDayModel(_svc);
            Week = new WeekViewModel(_svc);
            Settings = new SettingsViewModel(_svc, Now, Week);

            GoNowCommand = new RelayCommand(() => CurrentPage = Now);
            GoNowDayCommand = new RelayCommand(()=>CurrentPage = NowDay);
            GoWeekCommand = new RelayCommand(() => CurrentPage = Week);
            GoSettingsCommand = new RelayCommand(() => CurrentPage = Settings);

            CurrentPage = Now;

            Now.RefreshCommand.Execute(null);
            Week.RefreshCommand.Execute(null);
            Settings.LoadLocationCommand.Execute(null);
        }
    }
}
