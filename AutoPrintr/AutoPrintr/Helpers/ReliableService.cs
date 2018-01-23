using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPrintr.Helpers
{
    public abstract class ReliableService<T> : IDisposable where T: class
    {
        private readonly int _timeoutStep;
        private readonly int _timeoutPing;
        private ChannelFactory<T> _factory;
        private T _channel;

        protected ReliableService(int timeoutStepInMilliseconds = 100,
            int timeoutPingInMilliseconds = 2000)
        {
            _timeoutStep = timeoutStepInMilliseconds;
            _timeoutPing = timeoutPingInMilliseconds;
        }

        protected void InitializeFactory(ChannelFactory<T> newFactory)
        {
            _factory = newFactory ?? throw new ArgumentNullException(nameof(newFactory));
        }

        protected void Connect(Action<T> subscribe)
        {
            _channel = _factory.CreateChannel();
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

                    await Task.Delay(_timeoutPing, token);
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
                    if (_channel == null)
                    {
                        _channel = _factory?.CreateChannel() ?? throw new Exception($"ReliableService of {typeof(T)} is disposed.");
                    }

                    return lambda(_channel);
                }
                catch (Exception e)
                {
                    if (attempt >= 3)
                        throw;

                    _channel = null;
                }

                Thread.Sleep(attempt * _timeoutStep);
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