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
        private readonly IAstronomyServiceClient astronomyServiceClient;

        public NowViewModel Now { get; }
        public NowViewDayModel NowDay { get; }
        public WeekViewModel Week { get; }
        public SettingsViewModel Settings { get; }

        private ViewModelBase currentPage;
        public ViewModelBase CurrentPage
        {
            get => currentPage;
            set => Set(ref currentPage, value);
        }

        public RelayCommand GoNowCommand { get; }

        public RelayCommand GoNowDayCommand { get; }
        public RelayCommand GoWeekCommand { get; }
        public RelayCommand GoSettingsCommand { get; }

        public MainViewModel(IAstronomyServiceClient astronomyServiceClient)
        {
            astronomyServiceClient = new AstronomyServiceClient();

            Now = new NowViewModel(astronomyServiceClient);
            NowDay = new NowViewDayModel(astronomyServiceClient);
            Week = new WeekViewModel(astronomyServiceClient);
            Settings = new SettingsViewModel(astronomyServiceClient, Now, Week);

            GoNowCommand = new RelayCommand(() => CurrentPage = Now);
            GoNowDayCommand = new RelayCommand(()=>CurrentPage = NowDay);
            GoWeekCommand = new RelayCommand(() => CurrentPage = Week);
            GoSettingsCommand = new RelayCommand(() => CurrentPage = Settings);

            CurrentPage = Now;

            Now.RefreshCommand.Execute(null);
            Week.RefreshCommand.Execute(null);
            Settings.LoadLocationCommand.Execute(null);
        }

        public async Task InitAsync()
        {
            Now.RefreshAsync();
            Week.RefreshAsync();
            Settings.LoadLocationAsync();
        }
    }
}
