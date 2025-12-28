using Contracts;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfClient.MVVM;
using WpfClient.Services;

namespace WpfClient.ViewModels
{
    public class NowViewDayModel : ViewModelBase
    {
        private readonly AstronomyServiceClient serviceClient;
        private string currentDateAndTime;
        public string CurrentDateAndTime
        {
            get => currentDateAndTime;
            set => Set(ref currentDateAndTime, value);
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

        private string currentStatus;
        public string Status
        {
            get => currentStatus;
            set => Set(ref currentStatus, value);
        }

        private InstantCacheDataContract data;
        public InstantCacheDataContract Data
        {
            get => data;
            set => Set(ref data, value);
        }

        private DailyCacheDataContract dailyData;
        public DailyCacheDataContract DailyData
        {
            get => dailyData;
            set => Set(ref dailyData, value);
        }

        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand SetDayCommand { get; }
        public RelayCommand SetNightCommand { get; }

        public NowViewDayModel(AstronomyServiceClient svc)
        {
            serviceClient = svc;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            Application.Current.Dispatcher.InvokeAsync(async () => await RefreshAsync());
        }

        public async Task RefreshAsync()
        {
            try
            {
                Status = "Status: Refreshing...";

                Data = await serviceClient.GetFreshInstantAsync();

                WeeklyCacheDataContract wd = await serviceClient.GetFreshWeeklyAsync();
                DailyData = GetTodaysDataFromWeekly(wd);
                SetViewTexts(Data, DailyData);

                Status = "Status: OK";
            }
            catch (Exception ex)
            {
                Status = "Status" + ex.GetType().Name + ": " + ex.Message;
            }
        }

        private void SetViewTexts(InstantCacheDataContract data, DailyCacheDataContract dailyData)
        {
            CurrentDateAndTime = data.Date.ToString("dd-MM-yyyy") + " - " + Data.CurrentTime.ToString(@"hh\:mm");
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


        }
        private DailyCacheDataContract GetTodaysDataFromWeekly(WeeklyCacheDataContract weeklyCache)
        {
            return weeklyCache.DailyCacheDatas[1];
        }
    }
}
