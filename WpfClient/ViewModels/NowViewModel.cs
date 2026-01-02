using Contracts;
using Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using WpfClient.MVVM;
using WpfClient.Services;

namespace WpfClient.ViewModels
{
    public class NowViewModel : ViewModelBase
    {
        private readonly IAstronomyServiceClient serviceClient;
        private IImageProvider imageProvider;

        private string nextFullMoonDate;
        public string NextFullMoonDate
        {
            get => nextFullMoonDate;
            set => Set(ref nextFullMoonDate, value);
        }

        private string nextFirstQuarterDate;
        public string NextFirstQuarterDate
        {
            get => nextFirstQuarterDate;
            set => Set(ref nextFirstQuarterDate, value);
        }

        private string nextNewMoonDate;
        public string NextNewMoonDate
        {
            get => nextNewMoonDate;
            set => Set(ref nextNewMoonDate, value);
        }

        private string nextLastQuarterDate;
        public string NextLastQuarterDate
        {
            get => nextLastQuarterDate;
            set => Set(ref nextLastQuarterDate, value);
        }

        private ImageSource currentMoonPhaseImage; 
        public ImageSource CurrentMoonPhaseImage {
            get => currentMoonPhaseImage;
            set => Set(ref currentMoonPhaseImage, value);
        }

        private string currentDateAndTime;
        public string CurrentDateAndTime
        {
            get => currentDateAndTime;
            set => Set(ref currentDateAndTime, value);
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

        private string moonPhaseName;
        public string MoonPhaseName
        {
            get => moonPhaseName;
            set => Set(ref moonPhaseName, value);
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

        private string debugText;
        public string DebugText
        {
            get => debugText;
            set => Set(ref debugText, value);
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

        public NowViewModel(IAstronomyServiceClient svc, bool autoRefresh = true)
        {
            serviceClient = svc;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            imageProvider = new ImageProvider();
            if (autoRefresh) Application.Current.Dispatcher.InvokeAsync(async () => await RefreshAsync());
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
            MorningBlueHour = dailyData.MorningBlueHourBegin?.ToString("HH:mm") + " - " + dailyData.MorningBlueHourEnd?.ToString("HH:mm");
            EveningBlueHour = dailyData.EveningBlueHourBegin?.ToString("HH:mm") + " - " + dailyData.EveningBlueHourEnd?.ToString("HH:mm");
            CurrentMoonPhaseImage = imageProvider.getMoonImage(data.MoonPhase);
            switch (data.MoonPhase)
            {
                case MoonPhases.NewMoon:
                    MoonPhaseName = "New Moon";
                    break;
                case MoonPhases.WaxingCrescent:
                    MoonPhaseName = "Waxing Crescent";
                    break;
                case MoonPhases.WaningCrescent:
                    MoonPhaseName = "Waning Crescent";
                    break;
                case MoonPhases.FullMoon:
                    MoonPhaseName = "Full Moon";
                    break;
                case MoonPhases.WaxingGibbous:
                    MoonPhaseName = "Waxing Gibbous";
                    break;
                case MoonPhases.WaningGibbous:
                    MoonPhaseName = "Waning Gibbous";
                    break;
                case MoonPhases.FirstQuarter:
                    MoonPhaseName = "First Quarter";
                    break;
                case MoonPhases.LastQuarter:
                    MoonPhaseName = "Last Quarter";
                    break;
            }

            foreach(var i in data.NextMoonPhases){
                if(i.Phase == MoonPhases.NewMoon)
                {
                    NextNewMoonDate = i.TimeUtc.ToString("dd-MM-yyyy");

                }else if(i.Phase == MoonPhases.FirstQuarter)
                {
                    NextFirstQuarterDate = i.TimeUtc.ToString("dd-MM-yyyy");
                }
                else if(i.Phase == MoonPhases.FullMoon)
                {
                    NextFullMoonDate = i.TimeUtc.ToString("dd-MM-yyyy");
                }
                else if(i.Phase == MoonPhases.LastQuarter)
                {
                    NextLastQuarterDate = i.TimeUtc.ToString("dd-MM-yyyy");
                }
            }
        }

        private DailyCacheDataContract GetTodaysDataFromWeekly(WeeklyCacheDataContract weeklyCache)
        {
            return weeklyCache.DailyCacheDatas[1];
        }
    }
}
