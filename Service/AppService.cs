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
        private readonly IpGeoClient ipGeoClient;
        private LoggingService loggingService;
        private readonly SemaphoreSlim refreshGate = new SemaphoreSlim(1, 1);

        public AstronomyService(CacheStorage cacheStorage, IpGeoClient ipGeoClient, LoggingService loggingService)
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

                AstronomyTimeSeriesDto timeSeriesDto = await ipGeoClient.getAstronomyWeekAsync(
                    loc.Latitude,
                    loc.Longitude,
                    DateTimeOffset.Now.AddDays(-1),
                    DateTimeOffset.Now.AddDays(5),
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
                return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData);
            }


            await refreshGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (cacheStorage.IsInstantFresh(TimeSpan.FromMinutes(15)))
                {
                    return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData);

                }
                loggingService.WriteInfo("requesting instant data refresh");

                LocationCacheData loc = cacheStorage.LocationCacheData;
                AstronomyResponseDto dto = await ipGeoClient.GetAstronomyAsync(
                    loc.Latitude,
                    loc.Longitude,
                    loc.IanaTimeZoneId
                );

                /*
                loggingService.WriteInfo("updating local data to: " + dto.Location.City + "  in: " + dto.Location.CountryName);
                cacheStorage.SetLocation(new LocationCacheData
                {
                    LastUpdateTime = DateTimeOffset.UtcNow,
                    IanaTimeZoneId = loc.IanaTimeZoneId,
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude,
                    City = dto.Location.City,
                    CountryName = dto.Location.CountryName,
                });*/

                cacheStorage.RefreshInstantData(dto.Astronomy);
            }
            catch(Exception ex)
            {
                loggingService.WriteError("Refreshing instant data failed", ex);
                if (cacheStorage.InstantCacheData != null)
                    return ContractMapper.InstantCacheToContract(cacheStorage.InstantCacheData);
                throw;
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
                loggingService.WriteInfo("requesting weekly data refresh");

                LocationCacheData loc = cacheStorage.LocationCacheData;

                AstronomyTimeSeriesDto dto = await ipGeoClient.getAstronomyWeekAsync(
                    loc.Latitude,
                    loc.Longitude,
                    DateTimeOffset.Now.AddDays(-1),
                    DateTimeOffset.Now.AddDays(5),
                    loc.IanaTimeZoneId
                );

                /*
                cacheStorage.SetLocation(new LocationCacheData
                {
                    LastUpdateTime = DateTimeOffset.UtcNow,
                    IanaTimeZoneId = loc.IanaTimeZoneId,
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude,
                    City = dto.Location.City,
                    CountryName = dto.Location.CountryName,
                });*/
                cacheStorage.RefreshWeeklyData(dto);
            }
            catch(Exception ex) {
            
                loggingService.WriteError("Refreshing weekly data failed", ex);
                if (cacheStorage.InstantCacheData != null)
                    return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData);
                throw;
            }
            finally
            {
                refreshGate.Release();
            }

            return ContractMapper.WeeklyCacheToContract(cacheStorage.WeeklyCacheData);
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
                host.Open(); // <-- TO jest moment “serwer działa”

                
                Console.WriteLine("WCF host is running. Press ENTER to stop.");
                Console.ReadLine();

                host.Close();
            }
        }

    }
    public partial class AppService : ServiceBase
    {
        private CacheStorage cacheStorage;
        private IpGeoClient ipGeoClient;
        private AstronomyService astronomyService;
        private ServiceHost serviceHost;
        private LoggingService loggingService;
       
        public AppService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {            
            loggingService = new LoggingService("AstronomyService" , "Application");            
            cacheStorage = new CacheStorage();

            ipGeoClient = new IpGeoClient(new System.Net.Http.HttpClient(), loggingService);
            astronomyService = new AstronomyService(cacheStorage, ipGeoClient, loggingService);

            serviceHost = new ServiceHost(astronomyService);
            serviceHost.Open();

            loggingService.WriteInfo("Service starting");

            Task.Run(async () =>
            {
                try
                {
                   await FetchAndUpdateAstronomyCache();
                }
                catch (Exception ex)
                {
                    loggingService.WriteInfo("Service failed to start");
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
