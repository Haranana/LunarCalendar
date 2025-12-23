using Contracts;
using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Services
{
    public sealed class AstronomyServiceClient : IDisposable
    {
        private readonly ChannelFactory<IAstronomyService> _factory;

        public AstronomyServiceClient()
        {
            _factory = new ChannelFactory<IAstronomyService>("AstronomyTcp");
        }

        private async Task<T> Call<T>(Func<IAstronomyService, Task<T>> fn)
        {
            var ch = _factory.CreateChannel();
            var cc = (IClientChannel)ch;

            try
            {
                var result = await fn(ch).ConfigureAwait(false);
                cc.Close();
                return result;
            }
            catch
            {
                cc.Abort();
                throw;
            }
        }

        private async Task Call(Func<IAstronomyService, Task> fn)
        {
            var ch = _factory.CreateChannel();
            var cc = (IClientChannel)ch;

            try
            {
                await fn(ch).ConfigureAwait(false);
                cc.Close();
            }
            catch
            {
                cc.Abort();
                throw;
            }
        }

        public Task<InstantCacheDataContract> GetFreshInstantAsync()
            => Call(s => s.GetFreshInstant());

        public Task<WeeklyCacheDataContract> GetFreshWeeklyAsync()
            => Call(s => s.GetFreshWeekly());

        public Task UpdateLocationDataAsync(double lat, double lon)
            => Call(s => s.UpdateLocationData(lat, lon));

        // masz to jako sync w kontrakcie, więc przerzucamy na threadpool:
        public Task<LocationCacheDataContract> GetLocationDataAsync()
            => Task.Run(() => {
                var ch = _factory.CreateChannel();
                var cc = (IClientChannel)ch;
                try { var r = ch.GetLocationData(); cc.Close(); return r; }
                catch { cc.Abort(); throw; }
            });

        public void Dispose()
        {
            try { _factory.Close(); } catch { _factory.Abort(); }
        }
    }
}
