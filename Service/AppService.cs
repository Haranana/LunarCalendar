using Contracts;
using Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AstronomyService : IAstronomyService
    {

        private readonly CacheStorage cacheStorage;
        private readonly IpGeoClient ipGeoClient;
        private readonly SemaphoreSlim refreshGate = new SemaphoreSlim(1, 1);

        public AstronomyService(CacheStorage cacheStorage, IpGeoClient ipGeoClient)
        {
            this.cacheStorage = cacheStorage;
            this.ipGeoClient = ipGeoClient;
        }

        public LocationCacheDataContract GetLocationData()
        {
            return ContractMapper.LocationCacheToContract(cacheStorage.LocationCacheData);
        }

        public async Task UpdateLocationData(double lat, double lon)
        {
            LocationCacheData oldLoc = cacheStorage.LocationCacheData;
            TimeZoneInfo tz = TimeZoneUtil.GetTimeZone(lat, lon);
            string ianaId = TimeZoneUtil.GetIanaTimeZoneId(lat, lon);

            await refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                cacheStorage.SetLocation(new LocationCacheData
                {
                    LastUpdateTime = DateTimeOffset.Now,
                    Latitude = lat,
                    Longitude = lon,
                    UserTimeZoneInfo = tz,
                    IanaTimeZoneId = ianaId,
                    City = oldLoc.City,
                    CountryName = oldLoc.CountryName
                    
                });
                cacheStorage.InvalidateInstant();
                cacheStorage.InvalidateWeekly();


                LocationCacheData loc = cacheStorage.LocationCacheData;

                AstronomyTimeSeriesDto timeSeriesDto = await ipGeoClient.getAstronomyWeekAsync(
                    loc.Latitude,
                    loc.Longitude,
                    DateTimeOffset.Now.AddDays(-1),
                    DateTimeOffset.Now.AddDays(5),
                    loc.IanaTimeZoneId
                );

                AstronomyResponseDto astronomyDto = await ipGeoClient.GetAstronomyAsync(loc.Latitude, loc.Longitude, ianaId);

                
                cacheStorage.RefreshInstantData(astronomyDto.Astronomy);
                cacheStorage.RefreshWeeklyData(timeSeriesDto);
            }
            finally
            {
                refreshGate.Release();
            }
        }

        public async Task<InstantCacheDataContract> GetFreshInstant()
        {
            

            if (cacheStorage.IsInstantFresh(TimeSpan.FromMinutes(15)))
            {
                return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData);
            }


            await refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (cacheStorage.IsInstantFresh(TimeSpan.FromMinutes(15)))
                {
                    return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData);

                }

                LocationCacheData loc = cacheStorage.LocationCacheData;
                AstronomyResponseDto dto = await ipGeoClient.GetAstronomyAsync(
                    loc.Latitude,
                    loc.Longitude,
                    loc.IanaTimeZoneId
                );

                cacheStorage.RefreshInstantData(dto.Astronomy);
            }
            finally
            {
                refreshGate.Release();
            }

            return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData);
        }


        public async Task<WeeklyCacheDataContract> GetFreshWeekly()
        {
            if (cacheStorage.IsWeeklyFresh())
            {             
                return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData);
            }


            await refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (cacheStorage.IsWeeklyFresh())
                {
                    return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData);
                }

                LocationCacheData loc = cacheStorage.LocationCacheData;

                AstronomyTimeSeriesDto dto = await ipGeoClient.getAstronomyWeekAsync(
                    loc.Latitude,
                    loc.Longitude,
                    DateTimeOffset.Now.AddDays(-1),
                    DateTimeOffset.Now.AddDays(5),
                    loc.IanaTimeZoneId
                );
                cacheStorage.RefreshWeeklyData(dto);
            }
            finally
            {
                refreshGate.Release();
            }

            return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData);
        }

    }
    public partial class AppService : ServiceBase
    {
        private CacheStorage cacheStorage;
        private IpGeoClient ipGeoClient;
        private AstronomyService astronomyService;
        private ServiceHost serviceHost;
       
        public AppService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            cacheStorage = new CacheStorage();
            ipGeoClient = new IpGeoClient(new System.Net.Http.HttpClient());
            astronomyService = new AstronomyService(cacheStorage, ipGeoClient);
            serviceHost = new ServiceHost(astronomyService);
            serviceHost.Open();

            Task.Run(async () =>
            {
                try
                {
                   await FetchAndUpdateAstronomyCache();
                }
                catch (Exception ex)
                {
                   //log to eventLog to be implemented
                }
            });

        }



        public async Task FetchAndUpdateAstronomyCache()
        {
            LocationCacheData loc = cacheStorage.LocationCacheData;

            AstronomyTimeSeriesDto timeSeriesDto = await ipGeoClient.getAstronomyWeekAsync(
                loc.Latitude,
                loc.Longitude,
                DateTimeOffset.Now.AddDays(-1),
                DateTimeOffset.Now.AddDays(5),
                loc.IanaTimeZoneId
            );

            AstronomyResponseDto astronomyDto = await ipGeoClient.GetAstronomyAsync(loc.Latitude, loc.Longitude, loc.IanaTimeZoneId);


            cacheStorage.RefreshInstantData(astronomyDto.Astronomy);
            cacheStorage.RefreshWeeklyData(timeSeriesDto);
        }

        protected override void OnStop()
        {
            try { serviceHost?.Close(); }
            catch { serviceHost?.Abort(); }
        }


    }
}
