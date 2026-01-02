using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Services
{
    public interface IAstronomyServiceClient
    {
        Task<InstantCacheDataContract> GetFreshInstantAsync();

        Task<WeeklyCacheDataContract> GetFreshWeeklyAsync();

        Task UpdateLocationDataAsync(double lat, double lon);

        Task<LocationCacheDataContract> GetLocationDataAsync();
    }
    public class AstronomyServiceClient : IDisposable , IAstronomyServiceClient
    {
        private readonly ChannelFactory<IAstronomyService> channelFactory;

        public AstronomyServiceClient()
        {
            channelFactory = new ChannelFactory<IAstronomyService>("AstronomyTcp");
        }

        private async Task<T> Call<T>(Func<IAstronomyService, Task<T>> fn)
        {
            var channel = channelFactory.CreateChannel();
            var clientChannel = (IClientChannel)channel;

            try
            {
                var result = await fn(channel).ConfigureAwait(false);
                clientChannel.Close();
                return result;
            }
            catch
            {
                clientChannel.Abort();
                throw;
            }
        }

        private async Task Call(Func<IAstronomyService, Task> fn)
        {
            var ch = channelFactory.CreateChannel();
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

        public Task<InstantCacheDataContract> GetFreshInstantAsync() => Call(s => s.GetFreshInstant());

        public Task<WeeklyCacheDataContract> GetFreshWeeklyAsync() => Call(s => s.GetFreshWeekly());

        public Task UpdateLocationDataAsync(double lat, double lon) => Call(s => s.UpdateLocationData(lat, lon));

        public Task<LocationCacheDataContract> GetLocationDataAsync() => Task.Run(() => {
                var ch = channelFactory.CreateChannel();
                var cc = (IClientChannel)ch;
                try { var r = ch.GetLocationData(); cc.Close(); return r; }
                catch { cc.Abort(); throw; }
            });

        public void Dispose()
        {
            try { 
                channelFactory.Close(); 
            } catch { 
                channelFactory.Abort(); 
            }
        }
    }
}
