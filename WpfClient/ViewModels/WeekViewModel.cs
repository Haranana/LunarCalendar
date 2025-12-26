using Contracts;
using Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfClient.MVVM;
using WpfClient.Services;

namespace WpfClient.ViewModels
{
    public class WeekViewModel : ViewModelBase
    {
        private readonly AstronomyServiceClient astronomyServiceClient;

        public ObservableCollection<DailyCacheDataContract> Days { get; } = new ObservableCollection<DailyCacheDataContract>();

        private DailyCacheDataContract selectedDay;
        public DailyCacheDataContract SelectedDay
        {
            get => selectedDay;
            set => Set(ref selectedDay, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private ImageSource currentMoonPhaseImage;
        public ImageSource CurrentMoonPhaseImage
        {
            get => currentMoonPhaseImage;
            set => Set(ref currentMoonPhaseImage, value);
        }

        private string morningAstronomicalTwilight;
        public string MorningAstronomicalTwilight
        {
            get => morningAstronomicalTwilight;
            set => Set(ref morningAstronomicalTwilight, value);
        }

        private string morningCivilTwilight;
        public string MorningCivilTwilight
        {
            get => morningCivilTwilight;
            set => Set(ref morningCivilTwilight, value);
        }

        private string morningNauticalTwilight;
        public string MorningNauticalTwilight
        {
            get => morningNauticalTwilight;
            set => Set(ref morningNauticalTwilight, value);
        }

        private string eveningAstronomicalTwilight;
        public string EveningAstronomicalTwilight
        {
            get => eveningAstronomicalTwilight;
            set => Set(ref eveningAstronomicalTwilight, value);
        }

        private string eveningCivilTwilight;
        public string EveningCivilTwilight
        {
            get => eveningCivilTwilight;
            set => Set(ref eveningCivilTwilight, value);
        }

        private string eveningNauticalTwilight;
        public string EveningNauticalTwilight
        {
            get => eveningNauticalTwilight;
            set => Set(ref eveningNauticalTwilight, value);
        }

        private string morningGoldenHour;
        public string MorningGoldenHour
        {
            get => morningGoldenHour;
            set => Set(ref morningGoldenHour, value);
        }

        private string eveningGoldenHour;
        public string EveningGoldenHour
        {
            get => eveningGoldenHour;
            set => Set(ref eveningGoldenHour, value);
        }

        private string morningBlueHour;
        public string MorningBlueHour
        {
            get => morningBlueHour;
            set => Set(ref morningBlueHour, value);
        }

        private string eveningBlueHour;
        public string EveningBlueHour
        {
            get => eveningBlueHour;
            set => Set(ref eveningBlueHour, value);
        }

        private string dayLength;
        public string DayLength
        {
            get => dayLength;
            set => Set(ref dayLength, value);
        }

        private string nightLength;
        public string NightLength
        {
            get => nightLength;
            set => Set(ref nightLength, value);
        }


        private string sunrise;
        public string Sunrise
        {
            get => sunrise;
            set => Set(ref sunrise, value);
        }

        private string sunset;
        public string Sunset
        {
            get => sunset;
            set => Set(ref sunset, value);
        }

        private string moonrise;
        public string Moonrise
        {
            get => moonrise;
            set => Set(ref moonrise, value);
        }

        private string moonset;
        public string Moonset
        {
            get => moonset;
            set => Set(ref moonset, value);
        }

        private string moonPhaseName;
        public string MoonPhaseName
        {
            get => moonPhaseName;
            set => Set(ref moonPhaseName, value);
        }


        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand SelectDayCommand { get; }

        public WeekViewModel(AstronomyServiceClient svc)
        {
            astronomyServiceClient = svc;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            SelectDayCommand = new RelayCommand(obj =>
            {
                if (obj is DailyCacheDataContract day)
                {
                    SelectedDay = day;
                    SetViewTexts(day);
                }
            });
            Application.Current.Dispatcher.InvokeAsync(async () => { await RefreshAsync(); });

            
        }

        public async Task RefreshAsync()
        {
            try
            {
                Status = "Status: Refreshing...";
                var week = await astronomyServiceClient.GetFreshWeeklyAsync().ConfigureAwait(true);

                Days.Clear();
                foreach (var d in week.DailyCacheDatas.OrderBy(x => x.LocalDate))
                    Days.Add(d);

                SelectedDay = Days.FirstOrDefault();
                if(SelectedDay!= null)
                {
                    SetViewTexts(SelectedDay);
                }
                Status = "Status: Ok";
            }
            catch
            {
                Status = "Status: Error...";
            }
        }

        private void SetViewTexts(DailyCacheDataContract dailyData)
        {

            MorningBlueHour = dailyData.MorningBlueHourBegin?.ToString("HH:mm") + " - " + dailyData.MorningBlueHourEnd?.ToString("HH:mm");
            EveningBlueHour = dailyData.EveningBlueHourBegin?.ToString("HH:mm") + " - " + dailyData.EveningBlueHourEnd?.ToString("HH:mm");
            MorningGoldenHour = dailyData.MorningGoldenHourBegin?.ToString("HH:mm") + " - " + dailyData.MorningGoldenHourEnd?.ToString("HH:mm");
            EveningGoldenHour = dailyData.EveningGoldenHourBegin?.ToString("HH:mm") + " - " + dailyData.EveningGoldenHourEnd?.ToString("HH:mm");

            MorningNauticalTwilight = dailyData.MorningNauticalTwilightBegin?.ToString("HH:mm") + " - " + dailyData.MorningNauticalTwilightEnd?.ToString("HH:mm");
            MorningCivilTwilight = dailyData.MorningCivilTwilightBegin?.ToString("HH:mm") + " - " + dailyData.MorningCivilTwilightEnd?.ToString("HH:mm");
            MorningAstronomicalTwilight = dailyData.MorningAstronomicalTwilightBegin?.ToString("HH:mm") + " - " + dailyData.MorningAstronomicalTwilightEnd?.ToString("HH:mm");

            EveningNauticalTwilight = dailyData.EveningNauticalTwilightBegin?.ToString("HH:mm") + " - " + dailyData.EveningNauticalTwilightEnd?.ToString("HH:mm");
            EveningCivilTwilight = dailyData.EveningCivilTwilightBegin?.ToString("HH:mm") + " - " + dailyData.EveningCivilTwilightEnd?.ToString("HH:mm");
            EveningAstronomicalTwilight = dailyData.EveningAstronomicalTwilightBegin?.ToString("HH:mm") + " - " + dailyData.EveningAstronomicalTwilightEnd?.ToString("HH:mm");

            DayLength = dailyData.DayLength.ToString(@"hh\:mm");
            NightLength = dailyData.NightLength.ToString(@"hh\:mm");

            Moonrise = dailyData.Moonrise?.ToString("HH:mm");
            Moonset = dailyData.Moonset?.ToString("HH:mm");
            Sunrise = dailyData.Sunrise?.ToString("HH:mm");
            Sunset = dailyData.Sunset?.ToString("HH:mm");
            switch (dailyData.MoonPhase)
            {
                case MoonPhases.NewMoon:
                    MoonPhaseName = "New Moon";
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/NewMoon.png"));
                    break;
                case MoonPhases.WaxingCrescent:
                    MoonPhaseName = "Waxing Crescent";
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/WaxingCrescent.png"));
                    break;
                case MoonPhases.WaningCrescent:
                    MoonPhaseName = "Waning Crescent";
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/WaningCrescent.png"));
                    break;
                case MoonPhases.FullMoon:
                    MoonPhaseName = "Full Moon";
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/FullMoon.png"));
                    break;
                case MoonPhases.WaxingGibbous:
                    MoonPhaseName = "Waxing Gibbous";
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/WaxingGibbous.png"));
                    break;
                case MoonPhases.WaningGibbous:
                    MoonPhaseName = "Waning Gibbous";
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/WaningGibbous.png"));
                    break;
                case MoonPhases.FirstQuarter:
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/FirstQuarter.png"));
                    MoonPhaseName = "First Quarter";
                    break;
                case MoonPhases.LastQuarter:
                    CurrentMoonPhaseImage = new BitmapImage(new Uri("pack://application:,,,/Assets/LastQuarter.png"));
                    MoonPhaseName = "Last Quarter";
                    break;
            }
        }
    }
}
