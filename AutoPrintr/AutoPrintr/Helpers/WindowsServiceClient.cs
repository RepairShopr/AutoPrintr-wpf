using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AutoPrintr.Helpers
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false, IncludeExceptionDetailInFaults = true)]
    internal class WindowsServiceClient : ReliableService<IWindowsService>, IWindowsServiceCallback, IWindowsServiceClient
    {
        #region Properties
        private readonly Dispatcher _dispatcher;
        private readonly ILoggerService _loggerService;
        private CancellationTokenSource _cts;
        private Task _task;
        private Action _connectionFailed;

        public Action<Job> JobChangedAction { get; set; }
        #endregion

        #region Constructors
        public WindowsServiceClient(ILoggerService loggerService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _loggerService = loggerService;

            InitializeFactory(new DuplexChannelFactory<IWindowsService>(
                new InstanceContext(this), "WindowsServiceEndpoint"));
        }
        #endregion

        #region Methods
        public async Task ConnectAsync(Action connectionFailed)
        {
            if (_cts != null && _task != null)
            {
                await DisconnectAsync();
            }

            _connectionFailed = connectionFailed;
            _cts = new CancellationTokenSource();
            _task = Task.Run(Ping, _cts.Token);
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _cts.Cancel();
                await _task;
                TryCall(service => service.Disconnect());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
            }
        }

        public Task<IEnumerable<Printer>> GetPrintersAsync()
        {
            try
            {
                return TryCall(service => service.GetPrinters());
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                return null;
            }
        }

        public async Task<IEnumerable<Job>> GetJobsAsync()
        {
            try
            {
                return await TryCall(service => service.GetJobs());
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                return null;
            }
        }

        public Task<bool> PrintAsync(Job job)
            => Task.Run(() => TryCall(service =>
            {
                try
                {
                    service.Print(job);
                    return true;
                }
                catch (CommunicationException ex)
                {
                    Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    return false;
                }
            }));

        public Task<bool> DeleteJobsAsync(Job[] jobs)
            => Task.Run(() => TryCall(service =>
            {
                try
                {
                    service.DeleteJobs(jobs);
                    return true;
                }
                catch (CommunicationException ex)
                {
                    Debug.WriteLine($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    _loggerService.WriteWarning($"Error in {nameof(WindowsServiceClient)}: {ex.ToString()}");
                    return false;
                }
            }));

        public void JobChanged(Job job)
        {
            _dispatcher.Invoke(() =>
            {
                JobChangedAction?.Invoke(job);
            });
        }

        public void ConnectionFailed()
        {
            _dispatcher.Invoke(() =>
            {
                _connectionFailed?.Invoke();
            });
        }

        private Task Ping()
        {
            return PingByTimeout(service => service.Connect(), service => service.Ping(), _cts.Token);
        }
        #endregion
    }
}