using Contracts;
using Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
        private readonly IIpGeoClient ipGeoClient;
        private ILogger loggingService;
        private readonly SemaphoreSlim refreshGate = new SemaphoreSlim(1, 1);

        public AstronomyService(CacheStorage cacheStorage, IIpGeoClient ipGeoClient, ILogger loggingService)
        {
            this.cacheStorage = cacheStorage;
            this.ipGeoClient = ipGeoClient;
            this.loggingService = loggingService;
        }

        public LocationCacheDataContract GetLocationData()
        {
            return ContractMapper.LocationCacheToContract(cacheStorage.LocationCacheData);
        }

        public async Task UpdateLocationData(double lat, double lon)
        {
            loggingService.WriteInfo($"Requested location update for lat: {lat} and lon:{lon}");
            LocationCacheData oldLoc = cacheStorage.LocationCacheData;
            TimeZoneInfo tz = TimeZoneUtil.GetTimeZone(lat, lon);
            string ianaId = TimeZoneUtil.GetIanaTimeZoneId(lat, lon);

            await refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                cacheStorage.SetLocation(new LocationCacheData
                {
                    LastUpdateTime = DateTimeOffset.UtcNow,
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

                AstronomyTimeSeriesDto timeSeriesDto = await ipGeoClient.GetAstronomyRangeAsync(
                    loc.Latitude,
                    loc.Longitude,
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(5),
                    loc.IanaTimeZoneId
                );
                
                AstronomyResponseDto astronomyDto = await ipGeoClient.GetAstronomyAsync(loc.Latitude, loc.Longitude, ianaId);
                cacheStorage.SetLocation(new LocationCacheData
                {
                    LastUpdateTime = DateTimeOffset.UtcNow,
                    IanaTimeZoneId = loc.IanaTimeZoneId,
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude,
                    City = astronomyDto.Location.City,
                    CountryName = astronomyDto.Location.CountryName,
                });

                cacheStorage.RefreshInstantData(astronomyDto.Astronomy);
                cacheStorage.RefreshWeeklyData(timeSeriesDto);
            }
            catch
            {
                loggingService.WriteError($"Location updated failed for lat: {lat} and lon:{lon}");
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
                return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);
            }


            await refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (cacheStorage.IsInstantFresh(TimeSpan.FromMinutes(15)))
                {
                    return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);

                }
                loggingService.WriteInfo("requesting instant data refresh");

                LocationCacheData loc = cacheStorage.LocationCacheData;
                AstronomyResponseDto dto = await ipGeoClient.GetAstronomyAsync(
                    loc.Latitude,
                    loc.Longitude,
                    loc.IanaTimeZoneId
                );

                cacheStorage.RefreshInstantData(dto.Astronomy);
            }
            catch(Exception ex)
            {
                loggingService.WriteError("Refreshing instant data failed", ex);
                if (cacheStorage.InstantCacheData != null)
                    return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);
                throw;
            }
            finally
            {
                refreshGate.Release();
            }

            return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);
        }


        public async Task<WeeklyCacheDataContract> GetFreshWeekly()
        {
            if (cacheStorage.IsWeeklyFresh())
            {             
                return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);
            }


            await refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (cacheStorage.IsWeeklyFresh())
                {
                    return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);
                }
                loggingService.WriteInfo("requesting weekly data refresh");

                LocationCacheData loc = cacheStorage.LocationCacheData;

                AstronomyTimeSeriesDto dto = await ipGeoClient.GetAstronomyRangeAsync(
                    loc.Latitude,
                    loc.Longitude,
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(5),
                    loc.IanaTimeZoneId
                );


                cacheStorage.RefreshWeeklyData(dto);
            }
            catch(Exception ex) {
            
                loggingService.WriteError("Refreshing weekly data failed", ex);
                if (cacheStorage.WeeklyCacheData != null)
                    return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);
                throw;
            }
            finally
            {
                refreshGate.Release();
            }

            return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData, cacheStorage.LocationCacheData.UserTimeZoneInfo);
        }

    }

    public class AstronomyWorker
    {
        private readonly CacheStorage cache;
        private readonly IIpGeoClient client;
        private readonly ILogger log;

        public AstronomyWorker(CacheStorage cache, IIpGeoClient client, ILogger log)
        {
            this.cache = cache;
            this.client = client;
            this.log = log;
        }

        public async Task InitialFetchAsync()
        {
            try
            {
                await FetchAndUpdateAstronomyCacheAsync().ConfigureAwait(false);
                log.WriteInfo("Initial data fetched from API");
            }
            catch (Exception ex)
            {
                log.WriteError("Initial fetch failed", ex);                
            }
        }

        public async Task FetchAndUpdateAstronomyCacheAsync()
        {
            var location = cache.LocationCacheData;
            var beg = DateTimeOffset.UtcNow.AddDays(-1);
            var end = DateTimeOffset.UtcNow.AddDays(5);

            var timeSeriesDto = await client.GetAstronomyRangeAsync(
                location.Latitude, location.Longitude, beg, end, location.IanaTimeZoneId
            ).ConfigureAwait(false);

            var astronomyDto = await client.GetAstronomyAsync(
                location.Latitude, location.Longitude, location.IanaTimeZoneId
            ).ConfigureAwait(false);

            cache.RefreshInstantData(astronomyDto.Astronomy);
            cache.RefreshWeeklyData(timeSeriesDto);
        }
    }
    public class AppTester
    {
        public static void RunAsConsole()
        {
            var cacheStorage = new CacheStorage();
            var logging = new LoggingService("AstronomyService", "Application");
            var ipGeo = new IpGeoClient(new HttpClient(), logging);
            var svc = new AstronomyService(cacheStorage, ipGeo, logging);


            using (var host = new ServiceHost(svc))
            {
                host.Open(); 

                
                Console.WriteLine("WCF host is running. Press ENTER to stop.");
                Console.ReadLine();

                host.Close();
            }
        }

    }
    public partial class AppService : ServiceBase
    {
        private CacheStorage cacheStorage;
        private IIpGeoClient ipGeoClient;
        private AstronomyService astronomyService;
        private ServiceHost serviceHost;
        private ILogger loggingService;
        private AstronomyWorker worker;

        public AppService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            loggingService = new LoggingService("AstronomyService", "Application");
            cacheStorage = new CacheStorage();
            ipGeoClient = new IpGeoClient(new HttpClient(), loggingService);

           
            var wcf = new AstronomyService(cacheStorage, ipGeoClient, loggingService);
            serviceHost = new ServiceHost(wcf);
            serviceHost.Open();

          
            worker = new AstronomyWorker(cacheStorage, ipGeoClient, loggingService);
            loggingService.WriteInfo("Service starting");

           
            _ = Task.Run(() => worker.InitialFetchAsync());

        }



        public async Task FetchAndUpdateAstronomyCache()
        {
            LocationCacheData loc = cacheStorage.LocationCacheData;

            AstronomyTimeSeriesDto timeSeriesDto = await ipGeoClient.GetAstronomyRangeAsync(
                loc.Latitude,
                loc.Longitude,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(5),
                loc.IanaTimeZoneId
            );

            AstronomyResponseDto astronomyDto = await ipGeoClient.GetAstronomyAsync(loc.Latitude, loc.Longitude, loc.IanaTimeZoneId);


            cacheStorage.RefreshInstantData(astronomyDto.Astronomy);
            cacheStorage.RefreshWeeklyData(timeSeriesDto);

            loggingService.WriteInfo("Initial data fetched from API");
        }

        protected override void OnStop()
        {
            loggingService?.WriteInfo("Service stopping");

            try { serviceHost?.Close(); }
            catch { serviceHost?.Abort(); }
        }


    }
}
