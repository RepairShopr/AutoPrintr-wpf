using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPrintr.Helpers
{
    public abstract class ReliableService<T> : IDisposable where T: class
    {
        private readonly int _timeout;
        private ChannelFactory<T> _factory;
        private T _channel;

        protected ReliableService(int timeoutPingInMilliseconds = 2000)
        {
            _timeout = timeoutPingInMilliseconds;
        }

        protected void InitializeFactory(ChannelFactory<T> newFactory)
        {
            _factory = newFactory ?? throw new ArgumentNullException(nameof(newFactory));
        }

        protected void Connect(Action<T> subscribe)
        {
            _channel = _factory?.CreateChannel();
            subscribe(_channel);
        }

        protected async Task PingByTimeout(Action<T> ping, Action<T> subscribe, CancellationToken token)
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        if (_channel == null)
                        {
                            Connect(subscribe);
                        }

                        ping(_channel);
                    }
                    catch (Exception)
                    {
                        _channel = null;
                    }

                    await Task.Delay(_timeout, token);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        protected void TryCall(Action<T> lambda)
        {
            TryCall(channelArg => 
            {
                lambda(channelArg);
                return 0;
            });
        }

        protected TResult TryCall<TResult>(Func<T, TResult> lambda)
        {
            var attempt = 1;
            while (true)
            {
                try
                {
                    if (_channel != null)
                    {
                        return lambda(_channel);
                    }
                }
                catch (Exception)
                {
                    if (attempt >= 3)
                        throw;
                }

                if (attempt >= 3)
                {
                    throw new Exception("The WCF channel is not initialized.");
                }

                Thread.Sleep(_timeout);
                attempt++;
            }
        }

        public void Dispose()
        {
            _channel = null;
            if (_factory != null)
            {
                _factory.Close();
                _factory = null;
            }
        }
    }
}